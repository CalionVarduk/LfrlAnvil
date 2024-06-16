// Copyright 2024 Łukasz Furlepa
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
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents a disposable timer that can be listened to.
/// </summary>
public sealed class ReactiveTimer : ConcurrentEventSource<WithInterval<long>, EventPublisher<WithInterval<long>>>
{
    /// <summary>
    /// Specifies the default <see cref="SpinWait"/> duration hint. Equal to <b>1 microsecond</b>.
    /// </summary>
    public static readonly Duration DefaultSpinWaitDurationHint = Duration.FromMicroseconds( 1 );

    private readonly ITimestampProvider _timestampProvider;
    private readonly ManualResetEventSlim _reset;
    private readonly Duration _spinWaitDurationHint;
    private readonly long _expectedLastIndex;
    private Timestamp _prevStartTimestamp;
    private Timestamp _expectedNextTimestamp;
    private long _prevIndex;
    private InterlockedEnum<ReactiveTimerState> _state;

    /// <summary>
    /// Creates a new <see cref="ReactiveTimer"/> instance.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider used for time tracking.</param>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="count">Number of events this timer will emit in total. Equal to <see cref="Int64.MaxValue"/> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="count"/> is less than <b>1</b>
    /// or when <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds.
    /// </exception>
    public ReactiveTimer(ITimestampProvider timestampProvider, Duration interval, long count = long.MaxValue)
        : this( timestampProvider, interval, DefaultSpinWaitDurationHint, count ) { }

    /// <summary>
    /// Creates a new <see cref="ReactiveTimer"/> instance.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider used for time tracking.</param>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for this timer.</param>
    /// <param name="count">Number of events this timer will emit in total. Equal to <see cref="Int64.MaxValue"/> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="count"/> is less than <b>1</b>
    /// or when <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
    public ReactiveTimer(ITimestampProvider timestampProvider, Duration interval, Duration spinWaitDurationHint, long count = long.MaxValue)
        : base( new EventPublisher<WithInterval<long>>() )
    {
        Ensure.IsGreaterThan( count, 0 );
        Ensure.IsInRange( interval, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        Ensure.IsGreaterThanOrEqualTo( spinWaitDurationHint, Duration.Zero );

        Interval = interval;
        Count = count;
        _state = new InterlockedEnum<ReactiveTimerState>( ReactiveTimerState.Idle );

        _timestampProvider = timestampProvider;
        _reset = new ManualResetEventSlim( false );
        _expectedLastIndex = Count - 1;
        _spinWaitDurationHint = spinWaitDurationHint;
        _prevStartTimestamp = Timestamp.Zero;
        _expectedNextTimestamp = Timestamp.Zero;
        _prevIndex = -1;
    }

    /// <summary>
    /// Interval between subsequent timer events.
    /// </summary>
    public Duration Interval { get; }

    /// <summary>
    /// Number of events this timer will emit in total.
    /// </summary>
    public long Count { get; }

    /// <summary>
    /// Specifies the current state of this timer.
    /// </summary>
    public ReactiveTimerState State => _state.Value;

    /// <summary>
    /// Attempts to start this timer synchronously.
    /// </summary>
    /// <returns><b>true</b> when timer was started, otherwise <b>false</b>.</returns>
    public bool Start()
    {
        return TryStartCore( Interval ).Result == StartResult.Started;
    }

    /// <summary>
    /// Attempts to start this timer synchronously with an initial <paramref name="delay"/>.
    /// </summary>
    /// <param name="delay">Time that must elapse before emitting the first event.</param>
    /// <returns><b>true</b> when timer was started, otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="delay"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds.
    /// </exception>
    public bool Start(Duration delay)
    {
        Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        return TryStartCore( delay ).Result == StartResult.Started;
    }

    /// <summary>
    /// Attempts to start this timer asynchronously.
    /// </summary>
    /// <returns>
    /// New <see cref="Task"/> instance that completes when this timer is done or is stopped
    /// or <see cref="Task.CompletedTask"/> when this timer has been disposed
    /// or cancelled <see cref="Task"/> when this timer is already running.
    /// </returns>
    public Task StartAsync()
    {
        var (task, result) = TryStartCore( Interval, Task.Factory );
        return GetStartedTask( task, result );
    }

    /// <summary>
    /// Attempts to start this timer asynchronously.
    /// </summary>
    /// <param name="scheduler">Task scheduler.</param>
    /// <returns>
    /// New <see cref="Task"/> instance that completes when this timer is done or is stopped
    /// or <see cref="Task.CompletedTask"/> when this timer has been disposed
    /// or cancelled <see cref="Task"/> when this timer is already running.
    /// </returns>
    public Task StartAsync(TaskScheduler scheduler)
    {
        var taskFactory = new TaskFactory( scheduler );
        var (task, result) = TryStartCore( Interval, taskFactory );
        return GetStartedTask( task, result );
    }

    /// <summary>
    /// Attempts to start this timer asynchronously with an initial <paramref name="delay"/>.
    /// </summary>
    /// <param name="delay">Time that must elapse before emitting the first event.</param>
    /// <returns>
    /// New <see cref="Task"/> instance that completes when this timer is done or is stopped
    /// or <see cref="Task.CompletedTask"/> when this timer has been disposed
    /// or cancelled <see cref="Task"/> when this timer is already running.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="delay"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds.
    /// </exception>
    public Task StartAsync(Duration delay)
    {
        Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        var (task, result) = TryStartCore( delay, Task.Factory );
        return GetStartedTask( task, result );
    }

    /// <summary>
    /// Attempts to start this timer asynchronously with an initial <paramref name="delay"/>.
    /// </summary>
    /// <param name="scheduler">Task scheduler.</param>
    /// <param name="delay">Time that must elapse before emitting the first event.</param>
    /// <returns>
    /// New <see cref="Task"/> instance that completes when this timer is done or is stopped
    /// or <see cref="Task.CompletedTask"/> when this timer has been disposed
    /// or cancelled <see cref="Task"/> when this timer is already running.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="delay"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds.
    /// </exception>
    public Task StartAsync(TaskScheduler scheduler, Duration delay)
    {
        Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        var taskFactory = new TaskFactory( scheduler );
        var (task, result) = TryStartCore( delay, taskFactory );
        return GetStartedTask( task, result );
    }

    /// <summary>
    /// Attempts to stop this timer.
    /// </summary>
    /// <returns><b>true</b> when timer was marked for stopping, otherwise <b>false</b>.</returns>
    public bool Stop()
    {
        using ( ExclusiveLock.Enter( Sync ) )
        {
            if ( ! _state.Write( ReactiveTimerState.Stopping, ReactiveTimerState.Running ) )
                return false;

            _reset.Set();
            return true;
        }
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        using ( ExclusiveLock.Enter( Sync ) )
            DisposeCore();
    }

    private void DisposeCore()
    {
        if ( Base.IsDisposed )
            return;

        Base.Dispose();
        Stop();
        _reset.Dispose();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Task GetStartedTask(Task? task, StartResult result)
    {
        if ( task is not null )
            return task;

        return result == StartResult.Disposed ? Task.CompletedTask : Task.FromCanceled( new CancellationToken( true ) );
    }

    private (Task? Task, StartResult Result) TryStartCore(Duration delay, TaskFactory? taskFactory = null)
    {
        using ( ExclusiveLock.Enter( Sync ) )
        {
            if ( Base.IsDisposed || _prevIndex == _expectedLastIndex )
                return (null, StartResult.Disposed);

            if ( ! _state.Write( ReactiveTimerState.Running, ReactiveTimerState.Idle ) )
                return (null, StartResult.AlreadyRunning);

            _reset.Reset();
            _prevStartTimestamp = _timestampProvider.GetNow();
            _expectedNextTimestamp = _prevStartTimestamp + delay;
        }

        if ( taskFactory is not null )
            return (taskFactory.StartNew( RunBlockingTimer ), StartResult.Started);

        RunBlockingTimer();
        return (Task.CompletedTask, StartResult.Started);
    }

    private void RunBlockingTimer()
    {
        Duration delay;
        Timestamp expectedNextTimestamp;
        using ( ExclusiveLock.Enter( Sync ) )
        {
            if ( _state.Write( ReactiveTimerState.Idle, ReactiveTimerState.Stopping ) )
                return;

            expectedNextTimestamp = _expectedNextTimestamp;
            delay = Duration.Zero.Max( expectedNextTimestamp - _prevStartTimestamp - _spinWaitDurationHint );
        }

        while ( true )
        {
            try
            {
                _reset.Wait( delay );
            }
            catch ( ObjectDisposedException ) { }

            if ( _state.Write( ReactiveTimerState.Idle, ReactiveTimerState.Stopping ) )
                break;

            var timestamp = _timestampProvider.GetNow();
            while ( timestamp < expectedNextTimestamp )
            {
                Thread.SpinWait( 1 );
                timestamp = _timestampProvider.GetNow();
            }

            WithInterval<long> nextEvent;
            long skippedEventCount;
            using ( ExclusiveLock.Enter( Sync ) )
            {
                var interval = timestamp - _prevStartTimestamp;
                _prevStartTimestamp = timestamp;

                var offsetFromExpectedTimestamp = timestamp - expectedNextTimestamp;
                skippedEventCount = offsetFromExpectedTimestamp.Ticks / Interval.Ticks;
                var eventIndex = Math.Min( _prevIndex + skippedEventCount + 1, _expectedLastIndex );
                nextEvent = new WithInterval<long>( eventIndex, timestamp, interval );
            }

            try
            {
                Base.Publish( nextEvent );
            }
            catch ( ObjectDisposedException ) { }

            using ( ExclusiveLock.Enter( Sync ) )
            {
                if ( nextEvent.Event == _expectedLastIndex )
                {
                    _prevIndex = nextEvent.Event;
                    _state.Write( ReactiveTimerState.Idle );
                    DisposeCore();
                    break;
                }

                var endTimestamp = _timestampProvider.GetNow();
                var actualOffsetFromExpectedTimestamp = endTimestamp - expectedNextTimestamp;
                var actualSkippedEventCount = actualOffsetFromExpectedTimestamp.Ticks / Interval.Ticks;
                expectedNextTimestamp += new Duration( Interval.Ticks * (actualSkippedEventCount + 1) );
                _expectedNextTimestamp = expectedNextTimestamp;
                _prevIndex = Math.Min( nextEvent.Event + (actualSkippedEventCount - skippedEventCount), _expectedLastIndex );

                if ( _state.Write( ReactiveTimerState.Idle, ReactiveTimerState.Stopping ) )
                    break;

                delay = Duration.Zero.Max( expectedNextTimestamp - endTimestamp - _spinWaitDurationHint );
                _reset.Reset();
            }
        }
    }

    private enum StartResult : byte
    {
        Started = 0,
        Disposed = 1,
        AlreadyRunning = 2
    }
}
