// Copyright 2025 Łukasz Furlepa
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
    /// <returns>New <see cref="ValueTask{TResult}"/> instance which returns an <see cref="AsyncMutexLock"/> value.</returns>
    /// <exception cref="OperationCanceledException">
    /// When provided <see cref="cancellationToken"/> was cancelled before the lock was acquired.
    /// </exception>
    public async ValueTask<AsyncMutexLock> EnterAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        bool entered;
        Entry? entry;
        using ( AcquireLock() )
        {
            if ( ! _entryCache.TryPop( out entry ) )
                entry = new Entry( this );

            entered = _participants.IsEmpty;
            entry.NodeId = _participants.AddLast( entry );
            if ( ! entered )
            {
                entry.CancellationTokenRegistration = cancellationToken.UnsafeRegister(
                    static o =>
                    {
                        Assume.IsNotNull( o );
                        var e = ReinterpretCast.To<Entry>( o );
                        e.Complete( false );
                    },
                    entry );
            }
        }

        if ( entered )
            return new AsyncMutexLock( entry );

        entered = await entry.Source.GetTask().ConfigureAwait( false );
        if ( entered )
            return new AsyncMutexLock( entry );

        Reset( entry );
        ExceptionThrower.Throw( new OperationCanceledException( cancellationToken ) );
        return default;
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
    private bool Exit(Entry entry)
    {
        using ( AcquireLock() )
        {
            if ( _participants.First?.Index != entry.NodeId )
                return false;

            _participants.Remove( entry.NodeId );

            entry.NodeId = -1;
            entry.Source.Reset();
            _entryCache.Push( entry );

            var next = _participants.First;
            next?.Value.Complete( true );
            return true;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void Reset(Entry entry)
    {
        using ( AcquireLock() )
        {
            if ( entry.NodeId == -1 )
                return;

            var wasFirst = _participants.First?.Index == entry.NodeId;
            _participants.Remove( entry.NodeId );

            entry.NodeId = -1;
            entry.Source.Reset();
            _entryCache.Push( entry );

            if ( ! wasFirst )
                return;

            var next = _participants.First;
            next?.Value.Complete( true );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _sync, spinWaitMultiplier: 4 );
    }

    internal sealed class Entry
    {
        internal Entry(AsyncMutex mutex)
        {
            Mutex = mutex;
            Source = new ManualResetValueTaskSource<bool>();
            CancellationTokenRegistration = default;
            NodeId = -1;
        }

        internal readonly AsyncMutex Mutex;
        internal readonly ManualResetValueTaskSource<bool> Source;
        internal CancellationTokenRegistration CancellationTokenRegistration;
        internal int NodeId;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Complete(bool entered)
        {
            using ( Mutex.AcquireLock() )
            {
                if ( NodeId == -1 )
                    return;

                CancellationTokenRegistration.Dispose();
                CancellationTokenRegistration = default;
                Source.TrySetResult( entered );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool Exit()
        {
            return Mutex.Exit( this );
        }
    }
}
