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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;

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
                entered = _participants.IsEmpty;
                entry.NodeId = _participants.AddLast( entry );
                if ( entered )
                    return new AsyncMutexToken( entry, version );

                cancellationTokenRegistration = cancellationToken.CanBeCanceled
                    ? cancellationToken.UnsafeRegister(
                        static o =>
                        {
                            Assume.IsNotNull( o );
                            var state = ReinterpretCast.To<CancellationState>( o );
                            state.Entry.Mutex.Cancel( state.Entry, state.Version );
                        },
                        new CancellationState( entry, version ) )
                    : default;
            }

            try
            {
                entered = await entry.GetTask().ConfigureAwait( false );
                if ( entered )
                    return new AsyncMutexToken( entry, version );
            }
            finally
            {
                if ( ! entered )
                {
                    Reset( entry, version );
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
        }

        return true;
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
    private void Cancel(Entry entry, ulong version)
    {
        using ( AcquireLock() )
        {
            if ( entry.Version == version )
                entry.Complete( false );
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

    internal sealed class Entry : IValueTaskSource<bool>
    {
        private ManualResetValueTaskSourceCore<bool> _core;

        internal Entry(AsyncMutex mutex)
        {
            Mutex = mutex;
            _core = new ManualResetValueTaskSourceCore<bool> { RunContinuationsAsynchronously = true };
            Version = 0;
            NodeId = -1;
        }

        internal readonly AsyncMutex Mutex;
        internal ulong Version;
        internal int NodeId;

        private ValueTaskSourceStatus Status => _core.GetStatus( _core.Version );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal ValueTask<bool> GetTask()
        {
            return new ValueTask<bool>( this, _core.Version );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Complete(bool entered)
        {
            Assume.NotEquals( NodeId, -1 );
            if ( Status == ValueTaskSourceStatus.Pending )
                _core.SetResult( entered );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool Exit(ulong version)
        {
            return Mutex.Exit( this, version );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Reset()
        {
            Version = unchecked( Version + 1 );
            NodeId = -1;
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
