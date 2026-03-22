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
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Async;

/// <summary>
/// Represents a source of a fair asynchronous reader-writer lock.
/// </summary>
/// <remarks>Lock is not reentrant.</remarks>
public sealed class AsyncReaderWriterLock
{
    private readonly object _sync = new object();
    private LinkedListSlim<Entry> _participants;
    private LinkedListSlim<Entry> _upgradeableReadNodes;
    private StackSlim<Entry> _entryCache;

    /// <summary>
    /// Creates a new <see cref="AsyncReaderWriterLock"/> instance.
    /// </summary>
    public AsyncReaderWriterLock()
    {
        _participants = LinkedListSlim<Entry>.Create();
        _upgradeableReadNodes = LinkedListSlim<Entry>.Create();
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

        CancellationTokenRegistration cancellationTokenRegistration = default;
        try
        {
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
                cancellationTokenRegistration = entry.SetupCancellationToken( cancellationToken );
            }

            try
            {
                entered = await entry.GetTask().ConfigureAwait( false );
                if ( entered )
                    return new AsyncReaderWriterLockReadToken( entry, version );
            }
            finally
            {
                if ( ! entered )
                {
                    ResetRead( entry, version );
                    ExceptionThrower.Throw( new OperationCanceledException( cancellationToken ) );
                }
            }
        }
        finally
        {
            cancellationTokenRegistration.TryDispose();
        }

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
        using ( AcquireLock() )
        {
            entered = CanEnterReadImmediately();
            if ( ! entered )
                return default;

            if ( ! _entryCache.TryPop( out var entry ) )
                entry = new Entry( this );

            var version = entry.Version;
            entry.Type = EntryType.EnteredRead;
            entry.NodeId = _participants.AddLast( entry );
            return new AsyncReaderWriterLockReadToken( entry, version );
        }
    }

    /// <summary>
    /// Asynchronously acquires an upgradeable read lock from this reader-writer lock.
    /// </summary>
    /// <param name="cancellationToken">
    /// Optional <see cref="CancellationToken"/> that can be used to cancel pending upgradeable read lock acquisition.
    /// </param>
    /// <returns>
    /// New <see cref="ValueTask{TResult}"/> instance which returns an <see cref="AsyncReaderWriterLockUpgradeableReadToken"/> value.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// When provided <paramref name="cancellationToken"/> was cancelled before the upgradeable read lock was acquired.
    /// </exception>
    public async ValueTask<AsyncReaderWriterLockUpgradeableReadToken> EnterUpgradeableReadAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        CancellationTokenRegistration cancellationTokenRegistration = default;
        try
        {
            bool entered;
            Entry? entry;
            ulong version;
            using ( AcquireLock() )
            {
                if ( ! _entryCache.TryPop( out entry ) )
                    entry = new Entry( this );

                version = entry.Version;
                entered = CanEnterUpgradeableReadImmediately();
                entry.NodeId = _participants.AddLast( entry );
                entry.UpgradeableReadNodeId = _upgradeableReadNodes.AddLast( entry );
                if ( entered )
                {
                    entry.Type = EntryType.EnteredUpgradeableRead;
                    return new AsyncReaderWriterLockUpgradeableReadToken( entry, version );
                }

                entry.Type = EntryType.PendingUpgradeableRead;
                cancellationTokenRegistration = entry.SetupCancellationToken( cancellationToken );
            }

            try
            {
                entered = await entry.GetTask().ConfigureAwait( false );
                if ( entered )
                    return new AsyncReaderWriterLockUpgradeableReadToken( entry, version );
            }
            finally
            {
                if ( ! entered )
                {
                    ResetUpgradeableRead( entry, version );
                    ExceptionThrower.Throw( new OperationCanceledException( cancellationToken ) );
                }
            }
        }
        finally
        {
            cancellationTokenRegistration.TryDispose();
        }

        return default;
    }

    /// <summary>
    /// Attempts to synchronously acquire an upgradeable read lock from this reader-writer lock.
    /// </summary>
    /// <param name="entered"><b>out</b> parameter which specifies whether the upgradeable read lock was acquired.</param>
    /// <returns>
    /// New <see cref="AsyncReaderWriterLockUpgradeableReadToken"/> value. When <paramref name="entered"/> is <b>false</b>,
    /// then returned instanced will be a default value.
    /// </returns>
    public AsyncReaderWriterLockUpgradeableReadToken TryEnterUpgradeableRead(out bool entered)
    {
        using ( AcquireLock() )
        {
            entered = CanEnterUpgradeableReadImmediately();
            if ( ! entered )
                return default;

            if ( ! _entryCache.TryPop( out var entry ) )
                entry = new Entry( this );

            var version = entry.Version;
            entry.Type = EntryType.EnteredUpgradeableRead;
            entry.NodeId = _participants.AddLast( entry );
            entry.UpgradeableReadNodeId = _upgradeableReadNodes.AddLast( entry );
            return new AsyncReaderWriterLockUpgradeableReadToken( entry, version );
        }
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

        CancellationTokenRegistration cancellationTokenRegistration = default;
        try
        {
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
                cancellationTokenRegistration = entry.SetupCancellationToken( cancellationToken );
            }

            try
            {
                entered = await entry.GetTask().ConfigureAwait( false );
                if ( entered )
                    return new AsyncReaderWriterLockWriteToken( entry, version );
            }
            finally
            {
                if ( ! entered )
                {
                    ResetWrite( entry, version );
                    ExceptionThrower.Throw( new OperationCanceledException( cancellationToken ) );
                }
            }
        }
        finally
        {
            cancellationTokenRegistration.TryDispose();
        }

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
        using ( AcquireLock() )
        {
            entered = CanEnterWriteImmediately();
            if ( ! entered )
                return default;

            if ( ! _entryCache.TryPop( out var entry ) )
                entry = new Entry( this );

            var version = entry.Version;
            entry.Type = EntryType.EnteredWrite;
            entry.NodeId = _participants.AddLast( entry );
            return new AsyncReaderWriterLockWriteToken( entry, version );
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
            _upgradeableReadNodes.ResetCapacity();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal async ValueTask<AsyncReaderWriterLockUpgradedReadToken> UpgradeReadAsync(
        Entry entry,
        ulong version,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        CancellationTokenRegistration cancellationTokenRegistration = default;
        try
        {
            bool entered;
            using ( AcquireLock() )
            {
                entered = CanUpgradeReadImmediately( entry, version );
                if ( entered )
                {
                    entry.Type = EntryType.EnteredUpgradedRead;
                    return new AsyncReaderWriterLockUpgradedReadToken( entry, version );
                }

                entry.ResetCore();
                entry.Type = EntryType.EnteredUpgradingRead;
                cancellationTokenRegistration = entry.SetupCancellationToken( cancellationToken );
            }

            try
            {
                entered = await entry.GetTask().ConfigureAwait( false );
                if ( entered )
                    return new AsyncReaderWriterLockUpgradedReadToken( entry, version );
            }
            finally
            {
                if ( ! entered )
                {
                    ResetUpgradingRead( entry, version );
                    ExceptionThrower.Throw( new OperationCanceledException( cancellationToken ) );
                }
            }
        }
        finally
        {
            cancellationTokenRegistration.TryDispose();
        }

        return default;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal AsyncReaderWriterLockUpgradedReadToken TryUpgradeRead(Entry entry, ulong version, out bool entered)
    {
        using ( AcquireLock() )
        {
            entered = CanUpgradeReadImmediately( entry, version );
            if ( ! entered )
                return default;

            entry.Type = EntryType.EnteredUpgradedRead;
            return new AsyncReaderWriterLockUpgradedReadToken( entry, version );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanEnterReadImmediately()
    {
        var node = _participants.Last;
        if ( node is null )
            return true;

        var entry = node.Value.Value;
        switch ( entry.Type )
        {
            case EntryType.EnteredRead:
                node = _upgradeableReadNodes.First;
                if ( node is null )
                    return true;

                entry = node.Value.Value;
                return entry.Type != EntryType.EnteredUpgradingRead;

            case EntryType.EnteredUpgradeableRead:
                return true;
        }

        return false;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanEnterUpgradeableReadImmediately()
    {
        var node = _participants.Last;
        if ( node is null )
            return true;

        if ( ! _upgradeableReadNodes.IsEmpty )
            return false;

        var entry = node.Value.Value;
        return entry.Type == EntryType.EnteredRead;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanUpgradeReadImmediately(Entry target, ulong version)
    {
        var node = _participants.First;
        if ( node is null || target.Version != version )
            ExceptionThrower.Throw( new InvalidOperationException( ExceptionResources.CannotUpgradeDisposedReaderWriterReadLock ) );

        if ( target.Type != EntryType.EnteredUpgradeableRead )
            ExceptionThrower.Throw(
                new InvalidOperationException(
                    target.Type == EntryType.EnteredUpgradedRead
                        ? ExceptionResources.CannotUpgradeAlreadyUpgradedReaderWriterReadLock
                        : ExceptionResources.CannotUpgradeAlreadyUpgradingReaderWriterReadLock ) );

        var entry = node.Value.Value;
        if ( ! ReferenceEquals( entry, target ) )
            return false;

        node = node.Value.Next;
        if ( node is null )
            return true;

        entry = node.Value.Value;
        return entry.Type != EntryType.EnteredRead;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool CanEnterWriteImmediately()
    {
        return _participants.IsEmpty;
    }

    private bool ExitRead(Entry entry, ulong version)
    {
        using ( AcquireLock() )
        {
            if ( version != entry.Version || entry.NodeId == -1 )
                return false;

            ExitReadCore( entry );
        }

        return true;
    }

    private bool ExitWrite(Entry entry, ulong version)
    {
        using ( AcquireLock() )
        {
            if ( version != entry.Version || ! _participants.IsFirst( entry.NodeId ) )
                return false;

            ExitWriteCore( entry );
        }

        return true;
    }

    private bool ExitUpgradeableRead(Entry entry, ulong version)
    {
        using ( AcquireLock() )
        {
            if ( version != entry.Version || entry.NodeId == -1 )
                return false;

            if ( entry.Type != EntryType.EnteredUpgradeableRead )
                ExceptionThrower.Throw(
                    new InvalidOperationException(
                        entry.Type == EntryType.EnteredUpgradedRead
                            ? ExceptionResources.CannotReleaseUpgradedReaderWriterReadLock
                            : ExceptionResources.CannotReleaseUpgradingReaderWriterReadLock ) );

            ExitUpgradeableReadCore( entry );
        }

        return true;
    }

    private void ExitUpgradedRead(Entry entry, ulong version)
    {
        using ( AcquireLock() )
        {
            if ( version != entry.Version || ! _participants.IsFirst( entry.NodeId ) )
                return;

            if ( entry.Type != EntryType.EnteredUpgradedRead )
                ExceptionThrower.Throw( new InvalidOperationException( ExceptionResources.CannotReleaseNotUpgradedReaderWriterReadLock ) );

            ExitUpgradedReadCore( entry );
        }
    }

    private void ResetRead(Entry entry, ulong version)
    {
        using ( AcquireLock() )
        {
            if ( version != entry.Version || entry.NodeId == -1 )
                return;

            if ( entry.Type == EntryType.EnteredRead )
            {
                ExitReadCore( entry );
                return;
            }

            Assume.Equals( entry.Type, EntryType.PendingRead );
            Assume.True(
                _participants.GetNode( entry.NodeId ) is
                {
                    Next: null or { Value.Type: EntryType.PendingRead or EntryType.PendingWrite or EntryType.PendingUpgradeableRead },
                    Prev.Value.Type: not EntryType.EnteredUpgradeableRead
                } );

            Recycle( entry );
        }
    }

    private void ResetWrite(Entry entry, ulong version)
    {
        using ( AcquireLock() )
        {
            if ( version != entry.Version || entry.NodeId == -1 )
                return;

            if ( entry.Type == EntryType.EnteredWrite )
            {
                ExitWriteCore( entry );
                return;
            }

            Assume.Equals( entry.Type, EntryType.PendingWrite );

            var node = _participants.GetNode( entry.NodeId );
            Assume.IsNotNull( node );
            var next = node.Value.Next;

            Recycle( entry );

            if ( next is null )
                return;

            var nextEntry = next.Value.Value;
            Assume.True( nextEntry.Type is EntryType.PendingRead or EntryType.PendingWrite or EntryType.PendingUpgradeableRead );

            var prev = next.Value.Prev;
            Assume.IsNotNull( prev );
            var prevEntry = prev.Value.Value;

            switch ( prevEntry.Type )
            {
                case EntryType.EnteredRead:
                    switch ( nextEntry.Type )
                    {
                        case EntryType.PendingRead:
                            nextEntry.CompletePending( EntryType.EnteredRead );
                            next = CompletePendingReadRange( next.Value.Next );
                            if ( next is not null )
                                CompletePendingUpgradeableReadFollowedByPendingReadRange( next.Value );

                            break;

                        case EntryType.PendingUpgradeableRead:
                            if ( TryCompletePendingUpgradeableRead( nextEntry ) )
                                CompletePendingReadRange( next.Value.Next );

                            break;
                    }

                    break;

                case EntryType.EnteredUpgradeableRead:
                    if ( nextEntry.Type == EntryType.PendingRead )
                    {
                        nextEntry.CompletePending( EntryType.EnteredRead );
                        CompletePendingReadRange( next.Value.Next );
                    }

                    break;
            }
        }
    }

    private void ResetUpgradeableRead(Entry entry, ulong version)
    {
        using ( AcquireLock() )
        {
            if ( version != entry.Version || entry.NodeId == -1 )
                return;

            if ( entry.Type == EntryType.EnteredUpgradeableRead )
            {
                ExitUpgradeableReadCore( entry );
                return;
            }

            Assume.Equals( entry.Type, EntryType.PendingUpgradeableRead );

            var node = _participants.GetNode( entry.NodeId );
            Assume.IsNotNull( node );
            var next = node.Value.Next;

            Recycle( entry );

            if ( next is null )
                return;

            var nextEntry = next.Value.Value;
            Assume.True( nextEntry.Type is EntryType.PendingRead or EntryType.PendingWrite or EntryType.PendingUpgradeableRead );

            var prev = next.Value.Prev;
            Assume.IsNotNull( prev );
            var prevEntry = prev.Value.Value;

            switch ( prevEntry.Type )
            {
                case EntryType.EnteredRead:
                    Assume.True(
                        _upgradeableReadNodes.First?.Value.Type is EntryType.EnteredUpgradeableRead or EntryType.EnteredUpgradingRead );

                    if ( nextEntry.Type == EntryType.PendingRead )
                    {
                        node = _upgradeableReadNodes.First;
                        if ( node.Value.Value.Type == EntryType.EnteredUpgradeableRead )
                        {
                            nextEntry.CompletePending( EntryType.EnteredRead );
                            CompletePendingReadRange( next.Value.Next );
                        }
                    }

                    break;

                case EntryType.EnteredUpgradeableRead:
                    if ( nextEntry.Type == EntryType.PendingRead )
                    {
                        nextEntry.CompletePending( EntryType.EnteredRead );
                        CompletePendingReadRange( next.Value.Next );
                    }

                    break;
            }
        }
    }

    private void ResetUpgradingRead(Entry entry, ulong version)
    {
        using ( AcquireLock() )
        {
            if ( version != entry.Version || entry.NodeId == -1 )
                return;

            if ( entry.Type == EntryType.EnteredUpgradedRead )
            {
                ExitUpgradedReadCore( entry );
                return;
            }

            Assume.Equals( entry.Type, EntryType.EnteredUpgradingRead );

            entry.Type = EntryType.EnteredUpgradeableRead;
            var node = _participants.GetNode( entry.NodeId );
            Assume.IsNotNull( node );
            var next = node.Value.Next;

            if ( next is null )
                return;

            var nextEntry = next.Value.Value;
            Assume.True(
                nextEntry.Type is EntryType.PendingRead
                    or EntryType.EnteredRead
                    or EntryType.PendingWrite
                    or EntryType.PendingUpgradeableRead );

            Assume.True( node.Value.Prev is null || node.Value.Prev.Value.Value.Type == EntryType.EnteredRead );

            if ( nextEntry.Type == EntryType.EnteredRead )
            {
                next = next.Value.Next;
                while ( next is not null )
                {
                    nextEntry = next.Value.Value;
                    if ( nextEntry.Type != EntryType.EnteredRead )
                        break;

                    next = next.Value.Next;
                }
            }

            CompletePendingReadRange( next );
        }
    }

    private void ExitReadCore(Entry entry)
    {
        Assume.Equals( entry.Type, EntryType.EnteredRead );

        var next = _participants.GetNode( entry.NodeId );
        Assume.IsNotNull( next );
        var prev = next.Value.Prev;
        next = next.Value.Next;

        Recycle( entry );

        if ( next is null )
        {
            if ( prev is not null && _participants.IsFirst( prev.Value.Index ) )
            {
                Assume.True(
                    prev.Value.Value.Type is EntryType.EnteredRead
                        or EntryType.EnteredUpgradeableRead
                        or EntryType.EnteredUpgradingRead );

                var prevEntry = prev.Value.Value;
                if ( prevEntry.Type == EntryType.EnteredUpgradingRead )
                    prevEntry.CompletePending( EntryType.EnteredUpgradedRead );
            }

            return;
        }

        var nextEntry = next.Value.Value;
        Assume.False( nextEntry.Type is EntryType.EnteredWrite or EntryType.EnteredUpgradedRead );

        if ( prev is null )
        {
            Assume.False( nextEntry.Type is EntryType.PendingRead or EntryType.PendingUpgradeableRead );

            switch ( nextEntry.Type )
            {
                case EntryType.PendingWrite:
                    nextEntry.CompletePending( EntryType.EnteredWrite );
                    break;

                case EntryType.EnteredUpgradingRead:
                    next = next.Value.Next;
                    if ( next is null )
                        nextEntry.CompletePending( EntryType.EnteredUpgradedRead );
                    else
                    {
                        var nextNextEntry = next.Value.Value;
                        if ( nextNextEntry.Type != EntryType.EnteredRead )
                            nextEntry.CompletePending( EntryType.EnteredUpgradedRead );
                    }

                    break;
            }
        }
        else
        {
            Assume.True(
                prev.Value.Value.Type is EntryType.EnteredRead or EntryType.EnteredUpgradeableRead or EntryType.EnteredUpgradingRead );

            if ( _participants.IsFirst( prev.Value.Index ) && nextEntry.Type != EntryType.EnteredRead )
            {
                var prevEntry = prev.Value.Value;
                if ( prevEntry.Type == EntryType.EnteredUpgradingRead )
                {
                    Assume.False( nextEntry.Type is EntryType.EnteredUpgradeableRead or EntryType.EnteredUpgradingRead );
                    prevEntry.CompletePending( EntryType.EnteredUpgradedRead );
                    return;
                }
            }

            switch ( nextEntry.Type )
            {
                case EntryType.PendingRead:
                {
                    Assume.True( _upgradeableReadNodes.First?.Value.Type == EntryType.EnteredUpgradingRead );
                    break;
                }
                case EntryType.PendingUpgradeableRead:
                    Assume.True(
                        _upgradeableReadNodes.First?.Value.Type is EntryType.EnteredUpgradeableRead or EntryType.EnteredUpgradingRead );

                    break;

                case EntryType.EnteredUpgradeableRead:
                case EntryType.EnteredUpgradingRead:
                    Assume.Equals( prev.Value.Value.Type, EntryType.EnteredRead );
                    break;
            }
        }
    }

    private void ExitWriteCore(Entry entry)
    {
        Assume.Equals( entry.Type, EntryType.EnteredWrite );
        Assume.True( _participants.IsFirst( entry.NodeId ) );

        Recycle( entry );

        var next = _participants.First;
        if ( next is null )
            return;

        var nextEntry = next.Value.Value;
        Assume.True( nextEntry.Type is EntryType.PendingWrite or EntryType.PendingRead or EntryType.PendingUpgradeableRead );

        switch ( nextEntry.Type )
        {
            case EntryType.PendingRead:
                nextEntry.CompletePending( EntryType.EnteredRead );
                next = CompletePendingReadRange( next.Value.Next );
                if ( next is not null )
                    CompletePendingUpgradeableReadFollowedByPendingReadRange( next.Value );

                break;

            case EntryType.PendingWrite:
                nextEntry.CompletePending( EntryType.EnteredWrite );
                break;

            case EntryType.PendingUpgradeableRead:
                nextEntry.CompletePending( EntryType.EnteredUpgradeableRead );
                CompletePendingReadRange( next.Value.Next );
                break;
        }
    }

    private void ExitUpgradeableReadCore(Entry entry)
    {
        Assume.Equals( entry.Type, EntryType.EnteredUpgradeableRead );
        Assume.True( _upgradeableReadNodes.IsFirst( entry.UpgradeableReadNodeId ) );

        var wasFirst = _participants.IsFirst( entry.NodeId );
        var next = _participants.GetNode( entry.NodeId );
        Assume.IsNotNull( next );
        next = next.Value.Next;

        Recycle( entry );

        if ( next is null )
            return;

        var nextEntry = next.Value.Value;
        Assume.True( nextEntry.Type is EntryType.EnteredRead or EntryType.PendingWrite or EntryType.PendingUpgradeableRead );

        if ( wasFirst )
        {
            switch ( nextEntry.Type )
            {
                case EntryType.EnteredRead:
                    next = _upgradeableReadNodes.First;
                    if ( next is not null )
                    {
                        nextEntry = next.Value.Value;
                        Assume.Equals( nextEntry.Type, EntryType.PendingUpgradeableRead );
                        next = _participants.GetNode( nextEntry.NodeId );
                        Assume.IsNotNull( next );
                        var prev = next.Value.Prev;
                        Assume.IsNotNull( prev );
                        var prevEntry = prev.Value.Value;
                        if ( prevEntry.Type == EntryType.EnteredRead )
                        {
                            nextEntry.CompletePending( EntryType.EnteredUpgradeableRead );
                            CompletePendingReadRange( next.Value.Next );
                        }
                    }

                    break;

                case EntryType.PendingWrite:
                    nextEntry.CompletePending( EntryType.EnteredWrite );
                    break;

                case EntryType.PendingUpgradeableRead:
                    nextEntry.CompletePending( EntryType.EnteredUpgradeableRead );
                    CompletePendingReadRange( next.Value.Next );
                    break;
            }
        }
        else
        {
            Assume.True( next.Value.Prev?.Value.Type == EntryType.EnteredRead );

            switch ( nextEntry.Type )
            {
                case EntryType.EnteredRead:
                    next = _upgradeableReadNodes.First;
                    if ( next is not null )
                    {
                        nextEntry = next.Value.Value;
                        Assume.Equals( nextEntry.Type, EntryType.PendingUpgradeableRead );
                        next = _participants.GetNode( nextEntry.NodeId );
                        Assume.IsNotNull( next );
                        var prev = next.Value.Prev;
                        Assume.IsNotNull( prev );
                        var prevEntry = prev.Value.Value;
                        if ( prevEntry.Type == EntryType.EnteredRead )
                        {
                            nextEntry.CompletePending( EntryType.EnteredUpgradeableRead );
                            CompletePendingReadRange( next.Value.Next );
                        }
                    }

                    break;

                case EntryType.PendingUpgradeableRead:
                    nextEntry.CompletePending( EntryType.EnteredUpgradeableRead );
                    CompletePendingReadRange( next.Value.Next );
                    break;
            }
        }
    }

    private void ExitUpgradedReadCore(Entry entry)
    {
        Assume.Equals( entry.Type, EntryType.EnteredUpgradedRead );
        Assume.True( _upgradeableReadNodes.IsFirst( entry.UpgradeableReadNodeId ) );
        Assume.True( _participants.IsFirst( entry.NodeId ) );

        entry.Type = EntryType.EnteredUpgradeableRead;
        var next = _participants.First;
        Assume.IsNotNull( next );
        next = next.Value.Next;

        Assume.True(
            next is null
            || next.Value.Value.Type is EntryType.PendingRead or EntryType.PendingWrite or EntryType.PendingUpgradeableRead );

        CompletePendingReadRange( next );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static LinkedListSlimNode<Entry>? CompletePendingReadRange(LinkedListSlimNode<Entry>? node)
    {
        while ( node is not null )
        {
            var entry = node.Value.Value;
            if ( entry.Type != EntryType.PendingRead )
                break;

            entry.CompletePending( EntryType.EnteredRead );
            node = node.Value.Next;
        }

        return node;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool TryCompletePendingUpgradeableRead(Entry entry)
    {
        Assume.Equals( entry.Type, EntryType.PendingUpgradeableRead );
        if ( ! _upgradeableReadNodes.IsFirst( entry.UpgradeableReadNodeId ) )
            return false;

        entry.CompletePending( EntryType.EnteredUpgradeableRead );
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void CompletePendingUpgradeableReadFollowedByPendingReadRange(LinkedListSlimNode<Entry> node)
    {
        var entry = node.Value;
        if ( entry.Type == EntryType.PendingUpgradeableRead && TryCompletePendingUpgradeableRead( entry ) )
            CompletePendingReadRange( node.Next );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void Recycle(Entry entry)
    {
        _participants.Remove( entry.NodeId );
        _upgradeableReadNodes.Remove( entry.UpgradeableReadNodeId );
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
        EnteredWrite = 4,
        PendingUpgradeableRead = 5,
        EnteredUpgradeableRead = 6,
        EnteredUpgradingRead = 7,
        EnteredUpgradedRead = 8
    }

    internal sealed class Entry : IValueTaskSource<bool>
    {
        private ManualResetValueTaskSourceCore<bool> _core;

        internal Entry(AsyncReaderWriterLock @lock)
        {
            Lock = @lock;
            _core = new ManualResetValueTaskSourceCore<bool> { RunContinuationsAsynchronously = true };
            Version = 0;
            NodeId = -1;
            UpgradeableReadNodeId = -1;
            Type = default;
        }

        internal readonly AsyncReaderWriterLock Lock;
        internal ulong Version;
        internal int NodeId;
        internal int UpgradeableReadNodeId;
        internal EntryType Type;

        private ValueTaskSourceStatus Status => _core.GetStatus( _core.Version );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal ValueTask<bool> GetTask()
        {
            return new ValueTask<bool>( this, _core.Version );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal CancellationTokenRegistration SetupCancellationToken(CancellationToken cancellationToken)
        {
            return cancellationToken.CanBeCanceled
                ? cancellationToken.UnsafeRegister(
                    static o =>
                    {
                        Assume.IsNotNull( o );
                        var state = ReinterpretCast.To<CancellationState>( o );
                        state.Entry.CancelPending( state.Version );
                    },
                    new CancellationState( this, Version ) )
                : default;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void CompletePending(EntryType enteredType)
        {
            Assume.NotEquals( NodeId, -1 );
            Assume.True(
                (Type == EntryType.PendingRead && enteredType == EntryType.EnteredRead)
                || (Type == EntryType.PendingWrite && enteredType == EntryType.EnteredWrite)
                || (Type == EntryType.PendingUpgradeableRead && enteredType == EntryType.EnteredUpgradeableRead)
                || (Type == EntryType.EnteredUpgradingRead && enteredType == EntryType.EnteredUpgradedRead) );

            Type = enteredType;
            if ( Status == ValueTaskSourceStatus.Pending )
                _core.SetResult( true );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void CancelPending(ulong version)
        {
            using ( Lock.AcquireLock() )
            {
                if ( Version != version )
                    return;

                Assume.NotEquals( NodeId, -1 );
                if ( Status == ValueTaskSourceStatus.Pending )
                {
                    Assume.True(
                        Type is EntryType.PendingRead
                            or EntryType.PendingWrite
                            or EntryType.PendingUpgradeableRead
                            or EntryType.EnteredUpgradingRead );

                    _core.SetResult( false );
                }
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool ExitRead(ulong version)
        {
            return Lock.ExitRead( this, version );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool ExitWrite(ulong version)
        {
            return Lock.ExitWrite( this, version );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool ExitUpgradeableRead(ulong version)
        {
            return Lock.ExitUpgradeableRead( this, version );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ExitUpgradedRead(ulong version)
        {
            Lock.ExitUpgradedRead( this, version );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Reset()
        {
            Assume.NotEquals( Type, default );
            Version = unchecked( Version + 1 );
            NodeId = -1;
            UpgradeableReadNodeId = -1;
            Type = default;
            ResetCore();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ResetCore()
        {
            _core.Reset();
        }

        bool IValueTaskSource<bool>.GetResult(short token)
        {
            return _core.GetResult( token );
        }

        ValueTaskSourceStatus IValueTaskSource<bool>.GetStatus(short token)
        {
            return _core.GetStatus( token );
        }

        void IValueTaskSource<bool>.OnCompleted(
            Action<object?> continuation,
            object? state,
            short token,
            ValueTaskSourceOnCompletedFlags flags)
        {
            _core.OnCompleted( continuation, state, token, flags );
        }
    }

    private sealed record CancellationState(Entry Entry, ulong Version);
}
