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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Async;
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Chrono.Async;

/// <summary>
/// Represents a source of <see cref="DelayValueTask"/> instances,
/// which are value tasks that complete after a specified amount time has passed.
/// </summary>
public sealed class ValueTaskDelaySource : IDisposable, IAsyncDisposable
{
    private static Timestamp MaxTimestamp => new Timestamp( long.MaxValue );
    private static Duration MaxScheduleDelay => Duration.FromMilliseconds( int.MaxValue );

    private readonly ManualResetEventSlim _reset;
    private readonly ITimestampProvider _timestamps;
    private StackSlim<Node> _nodeCache;
    private ListSlim<Node> _schedule;
    private Task? _task;
    private bool _isDisposed;

    private ValueTaskDelaySource(ITimestampProvider timestamps)
    {
        _timestamps = timestamps;
        _reset = new ManualResetEventSlim( false );
        _nodeCache = StackSlim<Node>.Create();
        _schedule = ListSlim<Node>.Create();
    }

    /// <summary>
    /// Creates a new <see cref="ValueTaskDelaySource"/> instance.
    /// </summary>
    /// <param name="timestamps"><see cref="Timestamp"/> provider.</param>
    /// <returns>New <see cref="ValueTaskDelaySource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ValueTaskDelaySource Start(ITimestampProvider timestamps)
    {
        return Start( Task.Factory, timestamps );
    }

    /// <summary>
    /// Creates a new <see cref="ValueTaskDelaySource"/> instance.
    /// </summary>
    /// <param name="taskFactory"><see cref="TaskFactory"/> instance that will be used to start an underlying task.</param>
    /// <param name="timestamps"><see cref="Timestamp"/> provider.</param>
    /// <returns>New <see cref="ValueTaskDelaySource"/> instance.</returns>
    [Pure]
    public static ValueTaskDelaySource Start(TaskFactory taskFactory, ITimestampProvider timestamps)
    {
        var result = new ValueTaskDelaySource( timestamps );
        var task = taskFactory.StartNew(
            static o =>
            {
                Assume.IsNotNull( o );
                var source = ReinterpretCast.To<ValueTaskDelaySource>( o );
                try
                {
                    source.RunCore();
                }
                catch
                {
                    using ( source.AcquireLock() )
                        source._task = null;

                    source.Dispose();
                }
            },
            result,
            TaskCreationOptions.LongRunning );

        using ( result.AcquireLock() )
        {
            if ( ! result._isDisposed )
                result._task = task;
        }

        return result;
    }

    /// <inheritdoc/>
    /// <remarks>All scheduled delays will be prematurely completed with <see cref="ValueTaskDelayResult.Disposed"/> result.</remarks>
    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    /// <inheritdoc/>
    /// <remarks>All scheduled delays will be prematurely completed with <see cref="ValueTaskDelayResult.Disposed"/> result.</remarks>
    public async ValueTask DisposeAsync()
    {
        Task? task;
        using ( AcquireLock() )
        {
            if ( _isDisposed )
                return;

            _isDisposed = true;
            task = _task;
            _task = null;

            Clear();
            _reset.Set();
            _reset.TryDispose();
        }

        if ( task is not null )
            await task.ConfigureAwait( false );
    }

    /// <summary>
    /// Schedules a new delay task.
    /// </summary>
    /// <param name="delay">Amount of time after which the returned task completes.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> instance.</param>
    /// <returns>New <see cref="DelayValueTask"/> instance.</returns>
    public DelayValueTask Schedule(Duration delay, CancellationToken cancellationToken = default)
    {
        using ( AcquireLock() )
        {
            if ( _isDisposed )
                return default;

            if ( ! _nodeCache.TryPop( out var node ) )
                node = new Node( this );

            var now = _timestamps.GetNow();
            node.Timestamp = now + delay;

            if ( node.Timestamp <= now )
            {
                if ( cancellationToken.IsCancellationRequested )
                    node.SetCancelled();
                else
                    node.SetCompleted();
            }
            else
            {
                node.HeapIndex = NullableIndex.Create( _schedule.Count );
                _schedule.Add( node );
                FixUp( node.HeapIndex.Value );
                node.CancellationRegistration = cancellationToken.UnsafeRegister( Node.CancelUnsafe, node );

                if ( node.HeapIndex.Value == 0 )
                    _reset.Set();
            }

            return new DelayValueTask( node, node.GetTask() );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _reset, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    private void RunCore()
    {
        Duration delay;
        using ( AcquireLock() )
        {
            if ( _isDisposed )
                return;

            delay = GetNextDelay();
        }

        while ( true )
        {
            try
            {
                _reset.Wait( delay );
            }
            catch ( ObjectDisposedException )
            {
                return;
            }

            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return;

                var now = _timestamps.GetNow();
                while ( ! _schedule.IsEmpty )
                {
                    ref var first = ref _schedule.First();
                    if ( first.Timestamp > now )
                        break;

                    var node = first;
                    ref var last = ref Unsafe.Add( ref first, _schedule.Count - 1 );
                    last.HeapIndex = NullableIndex.Create( 0 );
                    first = last;
                    _schedule.RemoveLast();
                    FixDown( 0 );

                    node.SetCompleted();
                }

                delay = GetNextDelay();
            }
        }
    }

    private Duration GetNextDelay()
    {
        var now = _timestamps.GetNow();
        var next = MaxTimestamp;
        if ( ! _schedule.IsEmpty )
        {
            var node = _schedule.First();
            next = node.Timestamp;
        }

        var delay = next - now;
        return delay.Clamp( Duration.Zero, MaxScheduleDelay );
    }

    private void Clear()
    {
        _nodeCache.Clear();
        foreach ( var node in _schedule )
            node.SetDisposed();

        _schedule.Clear();
    }

    private void RemoveFromSchedule(int index)
    {
        var lastIndex = _schedule.Count - 1;
        Assume.IsInRange( index, 0, lastIndex );

        if ( index == lastIndex )
        {
            _schedule.RemoveLast();
            return;
        }

        ref var first = ref _schedule.First();
        ref var node = ref Unsafe.Add( ref first, index );
        ref var last = ref Unsafe.Add( ref first, lastIndex );

        var timestamp = node.Timestamp;
        var lastTimestamp = last.Timestamp;
        last.HeapIndex = node.HeapIndex;
        node = last;
        _schedule.RemoveLast();

        if ( timestamp < lastTimestamp )
            FixDown( index );
        else
            FixUp( index );
    }

    private void FixUp(int i)
    {
        var p = (i - 1) >> 1;
        ref var first = ref _schedule.First();

        while ( i > 0 )
        {
            ref var child = ref Unsafe.Add( ref first, i );
            ref var parent = ref Unsafe.Add( ref first, p );
            if ( child.Timestamp >= parent.Timestamp )
                break;

            (child.HeapIndex, parent.HeapIndex) = (parent.HeapIndex, child.HeapIndex);
            (child, parent) = (parent, child);
            i = p;
            p = (p - 1) >> 1;
        }
    }

    private void FixDown(int i)
    {
        var l = (i << 1) + 1;
        ref var first = ref _schedule.First();

        while ( l < _schedule.Count )
        {
            ref var parent = ref Unsafe.Add( ref first, i );
            ref var child = ref Unsafe.Add( ref first, l );

            var t = i;
            ref var target = ref parent;
            if ( child.Timestamp < target.Timestamp )
            {
                t = l;
                target = ref child;
            }

            var r = l + 1;
            if ( r < _schedule.Count )
            {
                child = ref Unsafe.Add( ref first, r )!;
                if ( child.Timestamp < target.Timestamp )
                {
                    t = r;
                    target = ref child;
                }
            }

            if ( i == t )
                break;

            (target.HeapIndex, parent.HeapIndex) = (parent.HeapIndex, target.HeapIndex);
            (target, parent) = (parent, target);
            i = t;
            l = (l << 1) + 1;
        }
    }

    internal sealed class Node : IValueTaskSource<ValueTaskDelayResult>
    {
        internal readonly ValueTaskDelaySource Source;
        internal Timestamp Timestamp;
        internal NullableIndex HeapIndex;
        internal CancellationTokenRegistration CancellationRegistration;
        private ManualResetValueTaskSourceCore<ValueTaskDelayResult> _core;

        internal Node(ValueTaskDelaySource source)
        {
            Source = source;
            _core = new ManualResetValueTaskSourceCore<ValueTaskDelayResult> { RunContinuationsAsynchronously = true };
            Timestamp = MaxTimestamp;
            HeapIndex = NullableIndex.Null;
        }

        private ValueTaskSourceStatus Status => _core.GetStatus( _core.Version );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal ValueTask<ValueTaskDelayResult> GetTask()
        {
            return new ValueTask<ValueTaskDelayResult>( this, _core.Version );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SetCompleted()
        {
            if ( Status == ValueTaskSourceStatus.Pending )
            {
                CancellationRegistration.TryDispose();
                CancellationRegistration = default;
                Timestamp = MaxTimestamp;
                HeapIndex = NullableIndex.Null;
                _core.SetResult( ValueTaskDelayResult.Completed );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SetDisposed()
        {
            if ( Status == ValueTaskSourceStatus.Pending )
            {
                CancellationRegistration.TryDispose();
                CancellationRegistration = default;
                Timestamp = MaxTimestamp;
                HeapIndex = NullableIndex.Null;
                _core.SetResult( ValueTaskDelayResult.Disposed );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SetCancelled()
        {
            if ( Status == ValueTaskSourceStatus.Pending )
            {
                CancellationRegistration = default;
                Timestamp = MaxTimestamp;
                HeapIndex = NullableIndex.Null;
                _core.SetResult( ValueTaskDelayResult.Cancelled );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal ValueTaskDelayResult GetResult(in ConfiguredValueTaskAwaitable<ValueTaskDelayResult>.ConfiguredValueTaskAwaiter awaiter)
        {
            using ( Source.AcquireLock() )
            {
                try
                {
                    return awaiter.GetResult();
                }
                finally
                {
                    _core.Reset();
                    if ( ! Source._isDisposed )
                    {
                        Assume.False( HeapIndex.HasValue );
                        Assume.Equals( Timestamp, MaxTimestamp );
                        Source._nodeCache.Push( this );
                    }
                }
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static void CancelUnsafe(object? state)
        {
            Assume.IsNotNull( state );
            var node = ReinterpretCast.To<Node>( state );
            node.Cancel();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void Cancel()
        {
            using ( Source.AcquireLock() )
            {
                if ( Source._isDisposed || ! HeapIndex.HasValue )
                    return;

                Source.RemoveFromSchedule( HeapIndex.Value );
                SetCancelled();
            }
        }

        ValueTaskDelayResult IValueTaskSource<ValueTaskDelayResult>.GetResult(short token)
        {
            return _core.GetResult( token );
        }

        ValueTaskSourceStatus IValueTaskSource<ValueTaskDelayResult>.GetStatus(short token)
        {
            return _core.GetStatus( token );
        }

        void IValueTaskSource<ValueTaskDelayResult>.OnCompleted(
            Action<object?> continuation,
            object? state,
            short token,
            ValueTaskSourceOnCompletedFlags flags)
        {
            _core.OnCompleted( continuation, state, token, flags );
        }
    }
}
