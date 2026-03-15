// Copyright 2026 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Async;

/// <summary>
/// Represents a source of a fair asynchronous reader-writer lock.
/// </summary>
/// <remarks>Lock is not reentrant.</remarks>
public sealed class AsyncReaderWriterLock
{
    private readonly object _sync = new object();
    private LinkedListSlim<Entry> _participants;
    private StackSlim<Entry> _entryCache;

    /// <summary>
    /// Creates a new <see cref="AsyncMutex"/> instance.
    /// </summary>
    public AsyncReaderWriterLock()
    {
        _participants = LinkedListSlim<Entry>.Create();
        _entryCache = StackSlim<Entry>.Create();
    }

    /// <summary>
    /// Returns the total number of lock participants, which includes current lock holders and all waiters.
    /// </summary>
    public int Participants
    {
        get
        {
            using ( AcquireLock() )
                return _participants.Count;
        }
    }

    /// <summary>
    /// Attempts to discard unused resources.
    /// </summary>
    public void TrimExcess()
    {
        using ( AcquireLock() )
        {
            _entryCache = StackSlim<Entry>.Create();
            _participants.ResetCapacity();
        }
    }

    /// <summary>
    /// Asynchronously acquires a read lock from this reader-writer lock.
    /// </summary>
    /// <param name="cancellationToken">
    /// Optional <see cref="CancellationToken"/> that can be used to cancel pending read lock acquisition.
    /// </param>
    /// <returns>
    /// New <see cref="ValueTask{TResult}"/> instance which returns an <see cref="AsyncReaderWriterLockReadToken"/> value.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// When provided <paramref name="cancellationToken"/> was cancelled before the read lock was acquired.
    /// </exception>
    public async ValueTask<AsyncReaderWriterLockReadToken> EnterReadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        bool entered;
        Entry? entry;
        ulong version;
        using ( AcquireLock() )
        {
            if ( ! _entryCache.TryPop( out entry ) )
                entry = new Entry( this );

            version = entry.Version;
            entered = CanEnterReadImmediately();
            entry.NodeId = _participants.AddLast( entry );
            if ( entered )
            {
                entry.Type = EntryType.EnteredRead;
                return new AsyncReaderWriterLockReadToken( entry, version );
            }

            entry.Type = EntryType.PendingRead;
            entry.CancellationTokenRegistration = cancellationToken.UnsafeRegister(
                static o =>
                {
                    Assume.IsNotNull( o );
                    var e = ReinterpretCast.To<Entry>( o );
                    e.Lock.Cancel( e );
                },
                entry );
        }

        entered = await entry.Source.GetTask().ConfigureAwait( false );
        if ( entered )
            return new AsyncReaderWriterLockReadToken( entry, version );

        Reset( entry, version );
        ExceptionThrower.Throw( new OperationCanceledException( cancellationToken ) );
        return default;
    }

    /// <summary>
    /// Attempts to synchronously acquire a read lock from this reader-writer lock.
    /// </summary>
    /// <param name="entered"><b>out</b> parameter which specifies whether the read lock was acquired.</param>
    /// <returns>
    /// New <see cref="AsyncReaderWriterLockReadToken"/> value. When <paramref name="entered"/> is <b>false</b>,
    /// then returned instanced will be a default value.
    /// </returns>
    public AsyncReaderWriterLockReadToken TryEnterRead(out bool entered)
    {
        Entry? entry;
        ulong version;
        using ( AcquireLock() )
        {
            entered = CanEnterReadImmediately();
            if ( ! entered )
                return default;

            if ( ! _entryCache.TryPop( out entry ) )
                entry = new Entry( this );

            version = entry.Version;
            entry.Type = EntryType.EnteredRead;
            entry.NodeId = _participants.AddLast( entry );
        }

        return new AsyncReaderWriterLockReadToken( entry, version );
    }

    /// <summary>
    /// Asynchronously acquires a write lock from this reader-writer lock.
    /// </summary>
    /// <param name="cancellationToken">
    /// Optional <see cref="CancellationToken"/> that can be used to cancel pending write lock acquisition.
    /// </param>
    /// <returns>
    /// New <see cref="ValueTask{TResult}"/> instance which returns an <see cref="AsyncReaderWriterLockWriteToken"/> value.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// When provided <paramref name="cancellationToken"/> was cancelled before the write lock was acquired.
    /// </exception>
    public async ValueTask<AsyncReaderWriterLockWriteToken> EnterWriteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        bool entered;
        Entry? entry;
        ulong version;
        using ( AcquireLock() )
        {
            if ( ! _entryCache.TryPop( out entry ) )
                entry = new Entry( this );

            version = entry.Version;
            entered = CanEnterWriteImmediately();
            entry.NodeId = _participants.AddLast( entry );
            if ( entered )
            {
                entry.Type = EntryType.EnteredWrite;
                return new AsyncReaderWriterLockWriteToken( entry, version );
            }

            entry.Type = EntryType.PendingWrite;
            entry.CancellationTokenRegistration = cancellationToken.UnsafeRegister(
                static o =>
                {
                    Assume.IsNotNull( o );
                    var e = ReinterpretCast.To<Entry>( o );
                    e.Lock.Cancel( e );
                },
                entry );
        }

        entered = await entry.Source.GetTask().ConfigureAwait( false );
        if ( entered )
            return new AsyncReaderWriterLockWriteToken( entry, version );

        Reset( entry, version );
        ExceptionThrower.Throw( new OperationCanceledException( cancellationToken ) );
        return default;
    }

    /// <summary>
    /// Attempts to synchronously acquire a write lock from this reader-writer lock.
    /// </summary>
    /// <param name="entered"><b>out</b> parameter which specifies whether the write lock was acquired.</param>
    /// <returns>
    /// New <see cref="AsyncReaderWriterLockWriteToken"/> value. When <paramref name="entered"/> is <b>false</b>,
    /// then returned instanced will be a default value.
    /// </returns>
    public AsyncReaderWriterLockWriteToken TryEnterWrite(out bool entered)
    {
        Entry? entry;
        ulong version;
        using ( AcquireLock() )
        {
            entered = CanEnterWriteImmediately();
            if ( ! entered )
                return default;

            if ( ! _entryCache.TryPop( out entry ) )
                entry = new Entry( this );

            version = entry.Version;
            entry.Type = EntryType.EnteredWrite;
            entry.NodeId = _participants.AddLast( entry );
        }

        return new AsyncReaderWriterLockWriteToken( entry, version );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanEnterReadImmediately()
    {
        if ( _participants.IsEmpty )
            return true;

        var last = _participants.Last;
        Assume.IsNotNull( last );
        return last.Value.Value.Type == EntryType.EnteredRead;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanEnterWriteImmediately()
    {
        return _participants.IsEmpty;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool Exit(Entry entry, ulong version, EntryType type)
    {
        using ( AcquireLock() )
        {
            if ( version != entry.Version )
                return false;

            if ( entry.Type == EntryType.EnteredRead )
            {
                if ( entry.NodeId == -1 || entry.Type != type )
                    return false;

                ExitRead( entry );
            }
            else if ( entry.Type == EntryType.EnteredWrite )
            {
                if ( ! _participants.IsFirst( entry.NodeId ) || entry.Type != type )
                    return false;

                ExitWrite( entry );
            }
            else
                return false;

            return true;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void Reset(Entry entry, ulong version)
    {
        using ( AcquireLock() )
        {
            if ( version != entry.Version || entry.NodeId == -1 )
                return;

            if ( entry.Type == EntryType.PendingRead )
                ResetRead( entry );
            else if ( entry.Type == EntryType.PendingWrite )
                ResetWrite( entry );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void Cancel(Entry entry)
    {
        using ( AcquireLock() )
            entry.Complete( false );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ExitRead(Entry entry)
    {
        Assume.Equals( entry.Type, EntryType.EnteredRead );

        var wasFirst = _participants.IsFirst( entry.NodeId );
        var node = _participants.GetNode( entry.NodeId );
        Assume.IsNotNull( node );
        var next = node.Value.Next;

        Recycle( entry );

        if ( next is null )
            return;

        Assume.NotEquals( next.Value.Value.Type, EntryType.EnteredWrite );
        if ( wasFirst )
        {
            if ( next.Value.Value.Type == EntryType.PendingWrite )
                next.Value.Value.Complete( true );
            else if ( next.Value.Value.Type == EntryType.PendingRead )
                CompletePendingReadRange( next.Value );
        }
        else
        {
            Assume.True( next.Value.Prev?.Value.Type == EntryType.EnteredRead );
            if ( next.Value.Value.Type == EntryType.PendingRead )
                CompletePendingReadRange( next.Value );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ExitWrite(Entry entry)
    {
        Assume.Equals( entry.Type, EntryType.EnteredWrite );
        Assume.True( _participants.IsFirst( entry.NodeId ) );

        Recycle( entry );

        var next = _participants.First;
        if ( next is null )
            return;

        Assume.True( next.Value.Value.Type is EntryType.PendingWrite or EntryType.PendingRead );

        if ( next.Value.Value.Type == EntryType.PendingWrite )
            next.Value.Value.Complete( true );
        else
            CompletePendingReadRange( next.Value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ResetRead(Entry entry)
    {
        Assume.Equals( entry.Type, EntryType.PendingRead );

        var wasFirst = _participants.IsFirst( entry.NodeId );
        var node = _participants.GetNode( entry.NodeId );
        Assume.IsNotNull( node );
        var next = node.Value.Next;

        Recycle( entry );

        if ( next is null )
            return;

        Assume.True( next.Value.Value.Type is EntryType.PendingWrite or EntryType.PendingRead );

        if ( wasFirst )
        {
            if ( next.Value.Value.Type == EntryType.PendingWrite )
                next.Value.Value.Complete( true );
            else
                CompletePendingReadRange( next.Value );
        }
        else if ( next.Value.Value.Type == EntryType.PendingRead )
        {
            var prev = next.Value.Prev;
            Assume.IsNotNull( prev );
            if ( prev.Value.Value.Type == EntryType.EnteredRead )
                CompletePendingReadRange( next.Value );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ResetWrite(Entry entry)
    {
        Assume.Equals( entry.Type, EntryType.PendingWrite );

        var wasFirst = _participants.IsFirst( entry.NodeId );
        var node = _participants.GetNode( entry.NodeId );
        Assume.IsNotNull( node );
        var next = node.Value.Next;

        Recycle( entry );

        if ( next is null )
            return;

        Assume.True( next.Value.Value.Type is EntryType.PendingWrite or EntryType.PendingRead );

        if ( wasFirst )
        {
            if ( next.Value.Value.Type == EntryType.PendingWrite )
                next.Value.Value.Complete( true );
            else
                CompletePendingReadRange( next.Value );
        }
        else if ( next.Value.Value.Type == EntryType.PendingRead )
        {
            var prev = next.Value.Prev;
            Assume.IsNotNull( prev );
            if ( prev.Value.Value.Type == EntryType.EnteredRead )
                CompletePendingReadRange( next.Value );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void CompletePendingReadRange(LinkedListSlimNode<Entry> first)
    {
        Assume.Equals( first.Value.Type, EntryType.PendingRead );

        first.Value.Complete( true );
        var next = first.Next;

        while ( next is not null && next.Value.Value.Type == EntryType.PendingRead )
        {
            next.Value.Value.Complete( true );
            next = next.Value.Next;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void Recycle(Entry entry)
    {
        _participants.Remove( entry.NodeId );
        entry.Reset();
        _entryCache.Push( entry );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _sync );
    }

    internal enum EntryType : byte
    {
        PendingRead = 1,
        EnteredRead = 2,
        PendingWrite = 3,
        EnteredWrite = 4
    }

    internal sealed class Entry
    {
        internal Entry(AsyncReaderWriterLock @lock)
        {
            Lock = @lock;
            Source = new ManualResetValueTaskSource<bool>();
            CancellationTokenRegistration = default;
            Version = 0;
            NodeId = -1;
            Type = default;
        }

        internal readonly AsyncReaderWriterLock Lock;
        internal readonly ManualResetValueTaskSource<bool> Source;
        internal CancellationTokenRegistration CancellationTokenRegistration;
        internal ulong Version;
        internal int NodeId;
        internal EntryType Type;

        internal bool IsEntered => (( byte )Type & 1) == 0;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Complete(bool entered)
        {
            if ( NodeId == -1 || Source.Status != ValueTaskSourceStatus.Pending )
                return;

            Assume.False( IsEntered );
            if ( entered )
                ++Type;

            CancellationTokenRegistration.Dispose();
            CancellationTokenRegistration = default;
            Source.SetResult( entered );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool Exit(ulong version, EntryType type)
        {
            return Lock.Exit( this, version, type );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Reset()
        {
            Assume.NotEquals( Type, default );
            Assume.Equals( CancellationTokenRegistration, default );
            ++Version;
            NodeId = -1;
            Type = default;
            Source.Reset();
        }
    }
}
