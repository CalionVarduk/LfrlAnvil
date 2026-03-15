// Copyright 2025-2026 Łukasz Furlepa
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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Async;

/// <summary>
/// Represents a source of a fair asynchronous mutex primitive.
/// </summary>
/// <remarks>Lock is not reentrant.</remarks>
public sealed class AsyncMutex
{
    private readonly object _sync = new object();
    private LinkedListSlim<Entry> _participants;
    private StackSlim<Entry> _entryCache;

    /// <summary>
    /// Creates a new <see cref="AsyncMutex"/> instance.
    /// </summary>
    public AsyncMutex()
    {
        _participants = LinkedListSlim<Entry>.Create();
        _entryCache = StackSlim<Entry>.Create();
    }

    /// <summary>
    /// Returns the total number of lock participants, which includes current lock holder and all waiters.
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
    /// Asynchronously acquires an exclusive lock from this mutex.
    /// </summary>
    /// <param name="cancellationToken">
    /// Optional <see cref="CancellationToken"/> that can be used to cancel pending mutex acquisition.
    /// </param>
    /// <returns>New <see cref="ValueTask{TResult}"/> instance which returns an <see cref="AsyncMutexToken"/> value.</returns>
    /// <exception cref="OperationCanceledException">
    /// When provided <paramref name="cancellationToken"/> was cancelled before the lock was acquired.
    /// </exception>
    public async ValueTask<AsyncMutexToken> EnterAsync(CancellationToken cancellationToken = default)
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
            entered = _participants.IsEmpty;
            entry.NodeId = _participants.AddLast( entry );
            if ( ! entered )
            {
                entry.CancellationTokenRegistration = cancellationToken.UnsafeRegister(
                    static o =>
                    {
                        Assume.IsNotNull( o );
                        var e = ReinterpretCast.To<Entry>( o );
                        e.Mutex.Cancel( e );
                    },
                    entry );
            }
        }

        if ( entered )
            return new AsyncMutexToken( entry, version );

        entered = await entry.Source.GetTask().ConfigureAwait( false );
        if ( entered )
            return new AsyncMutexToken( entry, version );

        Reset( entry, version );
        ExceptionThrower.Throw( new OperationCanceledException( cancellationToken ) );
        return default;
    }

    /// <summary>
    /// Attempts to synchronously acquire an exclusive lock from this mutex.
    /// </summary>
    /// <param name="entered"><b>out</b> parameter which specifies whether the lock was acquired.</param>
    /// <returns>
    /// New <see cref="AsyncMutexToken"/> value. When <paramref name="entered"/> is <b>false</b>,
    /// then returned instanced will be a default value.
    /// </returns>
    public AsyncMutexToken TryEnter(out bool entered)
    {
        Entry? entry;
        ulong version;
        using ( AcquireLock() )
        {
            entered = _participants.IsEmpty;
            if ( ! entered )
                return default;

            if ( ! _entryCache.TryPop( out entry ) )
                entry = new Entry( this );

            version = entry.Version;
            entry.NodeId = _participants.AddLast( entry );
        }

        return new AsyncMutexToken( entry, version );
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool Exit(Entry entry, ulong version)
    {
        using ( AcquireLock() )
        {
            if ( version != entry.Version || ! _participants.IsFirst( entry.NodeId ) )
                return false;

            Recycle( entry );
            var next = _participants.First;
            next?.Value.Complete( true );
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

            var wasFirst = _participants.IsFirst( entry.NodeId );
            Recycle( entry );
            if ( ! wasFirst )
                return;

            var next = _participants.First;
            next?.Value.Complete( true );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void Cancel(Entry entry)
    {
        using ( AcquireLock() )
            entry.Complete( false );
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

    internal sealed class Entry
    {
        internal Entry(AsyncMutex mutex)
        {
            Mutex = mutex;
            Source = new ManualResetValueTaskSource<bool>();
            CancellationTokenRegistration = default;
            Version = 0;
            NodeId = -1;
        }

        internal readonly AsyncMutex Mutex;
        internal readonly ManualResetValueTaskSource<bool> Source;
        internal CancellationTokenRegistration CancellationTokenRegistration;
        internal ulong Version;
        internal int NodeId;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Complete(bool entered)
        {
            if ( NodeId == -1 || Source.Status != ValueTaskSourceStatus.Pending )
                return;

            CancellationTokenRegistration.Dispose();
            CancellationTokenRegistration = default;
            Source.SetResult( entered );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool Exit(ulong version)
        {
            return Mutex.Exit( this, version );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Reset()
        {
            Assume.Equals( CancellationTokenRegistration, default );
            ++Version;
            NodeId = -1;
            Source.Reset();
        }
    }
}
