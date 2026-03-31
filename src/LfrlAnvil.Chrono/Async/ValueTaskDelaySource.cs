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
using LfrlAnvil.Async;
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Chrono.Async;

/// <summary>
/// Represents a source of value tasks that complete after a specified amount of time has passed.
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
    /// <param name="timestamps">Optional <see cref="Timestamp"/> provider.</param>
    /// <returns>New <see cref="ValueTaskDelaySource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ValueTaskDelaySource Start(ITimestampProvider? timestamps = null)
    {
        return Start( Task.Factory, timestamps );
    }

    /// <summary>
    /// Creates a new <see cref="ValueTaskDelaySource"/> instance.
    /// </summary>
    /// <param name="taskFactory"><see cref="TaskFactory"/> instance that will be used to start an underlying task.</param>
    /// <param name="timestamps">Optional <see cref="Timestamp"/> provider.</param>
    /// <returns>New <see cref="ValueTaskDelaySource"/> instance.</returns>
    [Pure]
    public static ValueTaskDelaySource Start(TaskFactory taskFactory, ITimestampProvider? timestamps = null)
    {
        var result = new ValueTaskDelaySource( timestamps ?? TimestampProvider.Shared );
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
                    _ = source.DisposeCore();
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
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    /// <remarks>All scheduled delays will be prematurely completed with <see cref="ValueTaskDelayResult.Disposed"/> result.</remarks>
    public async ValueTask DisposeAsync()
    {
        var task = DisposeCore();
        if ( task is not null )
            await task.ConfigureAwait( false );
    }

    /// <summary>
    /// Schedules a new delay value task.
    /// </summary>
    /// <param name="delay">Amount of time after which the returned value task completes.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> instance.</param>
    /// <returns>New <see cref="ValueTask{TResult}"/> instance which returns a <see cref="ValueTaskDelayResult"/> value.</returns>
    public async ValueTask<ValueTaskDelayResult> Schedule(Duration delay, CancellationToken cancellationToken = default)
    {
        Node? node = null;
        CancellationTokenRegistration cancellationTokenRegistration = default;
        try
        {
            ValueTask<ValueTaskDelayResult> task;
            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return ValueTaskDelayResult.Disposed;

                if ( ! _nodeCache.TryPop( out node ) )
                    node = new Node( this );

                var now = _timestamps.GetNow();
                node.Timestamp = now + delay;

                if ( node.Timestamp <= now )
                    node.OnImmediateCompletion( cancellationToken.IsCancellationRequested );
                else
                {
                    node.HeapIndex = NullableIndex.Create( _schedule.Count );
                    _schedule.Add( node );
                    FixUp( node.HeapIndex.Value );
                    cancellationTokenRegistration = node.RegisterCancellation( cancellationToken );

                    if ( node.HeapIndex.Value == 0 )
                        _reset.Set();
                }

                task = node.GetTask();
            }

            return await task.ConfigureAwait( false );
        }
        finally
        {
            cancellationTokenRegistration.TryDispose();
            if ( node is not null )
            {
                using ( AcquireLock() )
                    node.OnTaskAwaitFinished();
            }
        }
    }

    /// <summary>
    /// Creates a new <see cref="AsyncManualResetEvent"/> instance backed by this delay source.
    /// </summary>
    /// <param name="signaled">
    /// Specifies whether created event should start in signaled state. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New <see cref="AsyncManualResetEvent"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public AsyncManualResetEvent GetResetEvent(bool signaled = false)
    {
        using ( AcquireLock() )
        {
            if ( _isDisposed )
                return default;

            if ( ! _nodeCache.TryPop( out var node ) )
                node = new Node( this );

            var version = node.AsResetEvent( signaled );
            return new AsyncManualResetEvent( node, version );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _reset );
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

                _reset.Reset();
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

                    node.OnScheduledCompletion();
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
            node.OnSourceDisposed();

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
            l = (i << 1) + 1;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Task? DisposeCore()
    {
        Task? task;
        using ( AcquireLock() )
        {
            if ( _isDisposed )
                return null;

            _isDisposed = true;
            task = _task;
            _task = null;

            Clear();
            _reset.Set();
            _reset.TryDispose();
        }

        return task;
    }

    [Flags]
    private enum ResetEventState : byte
    {
        Enabled = 1,
        Set = 2,
        Awaited = 4,
        Disposed = 8
    }

    internal sealed class Node : IValueTaskSource<ValueTaskDelayResult>
    {
        internal readonly ValueTaskDelaySource Source;
        internal Timestamp Timestamp;
        internal NullableIndex HeapIndex;
        private ulong _resetEventFlags;
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
        internal void OnScheduledCompletion()
        {
            Timestamp = MaxTimestamp;
            HeapIndex = NullableIndex.Null;
            if ( Status == ValueTaskSourceStatus.Pending )
                _core.SetResult( ValueTaskDelayResult.Completed );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnImmediateCompletion(bool cancelled)
        {
            Assume.False( HasState( ResetEventState.Enabled ) );
            Assume.False( HeapIndex.HasValue );

            Timestamp = MaxTimestamp;
            if ( Status == ValueTaskSourceStatus.Pending )
                _core.SetResult( cancelled ? ValueTaskDelayResult.Cancelled : ValueTaskDelayResult.Completed );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnSourceDisposed()
        {
            Assume.True( HeapIndex.HasValue );
            Timestamp = MaxTimestamp;
            HeapIndex = NullableIndex.Null;

            if ( HasState( ResetEventState.Enabled ) )
            {
                Assume.False( HasState( ResetEventState.Set ) );
                Assume.True( HasState( ResetEventState.Awaited ) );
                SetState( ResetEventState.Disposed );
            }

            if ( Status == ValueTaskSourceStatus.Pending )
                _core.SetResult( ValueTaskDelayResult.Disposed );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTaskAwaitFinished()
        {
            _core.Reset();
            if ( Source._isDisposed )
                return;

            Assume.False( HeapIndex.HasValue );
            Assume.Equals( Timestamp, MaxTimestamp );
            InvalidateVersion();
            Source._nodeCache.Push( this );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal CancellationTokenRegistration RegisterCancellation(CancellationToken cancellationToken)
        {
            Assume.False( HasState( ResetEventState.Enabled ) );
            return cancellationToken.CanBeCanceled
                ? cancellationToken.UnsafeRegister(
                    static o =>
                    {
                        Assume.IsNotNull( o );
                        var state = ReinterpretCast.To<CancellationState>( o );
                        state.Node.OnCancelled( state.Version );
                    },
                    new CancellationState( this, GetVersion() ) )
                : default;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal ulong AsResetEvent(bool signaled)
        {
            Assume.Equals( _resetEventFlags & 15, 0UL );
            SetState( signaled ? ResetEventState.Enabled | ResetEventState.Set : ResetEventState.Enabled );
            return GetVersion();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnResetEventDispose(ulong version)
        {
            using ( Source.AcquireLock() )
            {
                if ( ! HasVersion( version ) || HasState( ResetEventState.Disposed ) )
                    return;

                if ( HeapIndex.HasValue )
                {
                    Assume.False( HasState( ResetEventState.Set ) );
                    Assume.True( HasState( ResetEventState.Awaited ) );

                    Source.RemoveFromSchedule( HeapIndex.Value );
                    HeapIndex = NullableIndex.Null;
                    Timestamp = MaxTimestamp;

                    SetState( ResetEventState.Disposed );
                    if ( Status == ValueTaskSourceStatus.Pending )
                        _core.SetResult( ValueTaskDelayResult.Disposed );
                }
                else
                {
                    Assume.Equals( Timestamp, MaxTimestamp );
                    if ( HasState( ResetEventState.Awaited ) )
                        SetState( ResetEventState.Disposed );
                    else
                    {
                        InvalidateVersion();
                        if ( ! Source._isDisposed )
                            Source._nodeCache.Push( this );
                    }
                }
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool OnResetEventSet(ulong version)
        {
            using ( Source.AcquireLock() )
            {
                if ( Source._isDisposed || ! HasVersion( version ) || HasAnyState( ResetEventState.Disposed | ResetEventState.Set ) )
                    return false;

                SetState( ResetEventState.Set );
                if ( HeapIndex.HasValue )
                {
                    Assume.True( HasState( ResetEventState.Awaited ) );

                    Source.RemoveFromSchedule( HeapIndex.Value );
                    HeapIndex = NullableIndex.Null;
                    Timestamp = MaxTimestamp;

                    if ( Status == ValueTaskSourceStatus.Pending )
                        _core.SetResult( ValueTaskDelayResult.Cancelled );
                }

                return true;
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool OnResetEventReset(ulong version)
        {
            using ( Source.AcquireLock() )
            {
                if ( Source._isDisposed
                    || ! HasVersion( version )
                    || HasState( ResetEventState.Disposed )
                    || ! HasState( ResetEventState.Set ) )
                    return false;

                RemoveState( ResetEventState.Set );
                return true;
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal ValueTask<ValueTaskDelayResult> OnResetEventWait(ulong version, ref bool success, Duration? delay)
        {
            using ( Source.AcquireLock() )
            {
                if ( Source._isDisposed || ! HasVersion( version ) || HasState( ResetEventState.Disposed ) )
                    return ValueTask.FromResult( ValueTaskDelayResult.Disposed );

                if ( HasState( ResetEventState.Awaited ) )
                    return ValueTask.FromResult( ( ValueTaskDelayResult )AsyncManualResetEventResult.AlreadyAwaited );

                if ( HasState( ResetEventState.Set ) )
                    return ValueTask.FromResult( ValueTaskDelayResult.Cancelled );

                Assume.False( HeapIndex.HasValue );
                Assume.Equals( Timestamp, MaxTimestamp );

                if ( delay is null )
                    Timestamp = MaxTimestamp.Subtract( Duration.FromTicks( 1 ) );
                else
                {
                    if ( delay.Value <= Duration.Zero )
                        return ValueTask.FromResult( ValueTaskDelayResult.Completed );

                    var now = Source._timestamps.GetNow();
                    Timestamp = now + delay.Value;
                }

                SetState( ResetEventState.Awaited );
                HeapIndex = NullableIndex.Create( Source._schedule.Count );
                Source._schedule.Add( this );

                Source.FixUp( HeapIndex.Value );
                if ( HeapIndex.Value == 0 )
                    Source._reset.Set();

                success = true;
                return GetTask();
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnResetEventTaskAwaitFinished(ulong version)
        {
            using ( Source.AcquireLock() )
            {
                if ( ! HasVersion( version ) )
                    return;

                Assume.True( HasState( ResetEventState.Awaited ) );
                Assume.False( HeapIndex.HasValue );
                Assume.Equals( Timestamp, MaxTimestamp );

                _core.Reset();
                if ( ! HasState( ResetEventState.Disposed ) )
                {
                    RemoveState( ResetEventState.Awaited );
                    return;
                }

                InvalidateVersion();
                if ( ! Source._isDisposed )
                    Source._nodeCache.Push( this );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void OnCancelled(ulong version)
        {
            using ( Source.AcquireLock() )
            {
                if ( Source._isDisposed || ! HeapIndex.HasValue || version != GetVersion() )
                    return;

                Assume.False( HasState( ResetEventState.Enabled ) );
                Source.RemoveFromSchedule( HeapIndex.Value );
                HeapIndex = NullableIndex.Null;
                Timestamp = MaxTimestamp;
                if ( Status == ValueTaskSourceStatus.Pending )
                    _core.SetResult( ValueTaskDelayResult.Cancelled );
            }
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private bool HasState(ResetEventState state)
        {
            return (_resetEventFlags & ( ulong )state) == ( ulong )state;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private bool HasAnyState(ResetEventState state)
        {
            return (_resetEventFlags & ( ulong )state) != 0;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void SetState(ResetEventState state)
        {
            _resetEventFlags |= ( ulong )state;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void RemoveState(ResetEventState state)
        {
            _resetEventFlags &= ~( ulong )state;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void InvalidateVersion()
        {
            var version = unchecked( GetVersion() + 1 );
            _resetEventFlags = version << 4;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ulong GetVersion()
        {
            return _resetEventFlags >> 4;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private bool HasVersion(ulong version)
        {
            return HasState( ResetEventState.Enabled ) && GetVersion() == version;
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

    private sealed record CancellationState(Node Node, ulong Version);
}
