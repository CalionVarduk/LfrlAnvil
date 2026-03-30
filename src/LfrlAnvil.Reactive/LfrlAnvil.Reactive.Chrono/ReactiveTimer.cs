// Copyright 2024-2026 Łukasz Furlepa
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
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Extensions;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Internal;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents a disposable timer that can be listened to.
/// </summary>
public sealed class ReactiveTimer : ConcurrentEventSource<WithInterval<long>, EventPublisher<WithInterval<long>>>, IAsyncDisposable
{
    /// <summary>
    /// Specifies the default <see cref="SpinWait"/> duration hint. Equal to <b>1 microsecond</b>.
    /// </summary>
    public static Duration DefaultSpinWaitDurationHint => Duration.FromMicroseconds( 1 );

    private readonly ITimestampProvider _timestampProvider;
    private readonly Duration _spinWaitDurationHint;
    private readonly long _expectedLastIndex;
    private DelaySource _delaySource;
    private AsyncManualResetEvent _reset;
    private Timestamp _prevStartTimestamp;
    private Timestamp _expectedNextTimestamp;
    private Task? _task;
    private long _prevIndex;
    private ReactiveTimerState _state;

    /// <summary>
    /// Creates a new <see cref="ReactiveTimer"/> instance.
    /// </summary>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="timestampProvider">Optional timestamp provider used for time tracking.</param>
    /// <param name="delaySource">Optional value task delay source to use for scheduling delays.</param>
    /// <param name="count">Number of events this timer will emit in total. Equal to <see cref="long.MaxValue"/> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="count"/> is less than <b>1</b>
    /// or when <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds.
    /// </exception>
    public ReactiveTimer(
        Duration interval,
        ITimestampProvider? timestampProvider = null,
        ValueTaskDelaySource? delaySource = null,
        long count = long.MaxValue)
        : this( interval, DefaultSpinWaitDurationHint, timestampProvider, delaySource, count ) { }

    /// <summary>
    /// Creates a new <see cref="ReactiveTimer"/> instance.
    /// </summary>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for this timer.</param>
    /// <param name="timestampProvider">Optional timestamp provider used for time tracking.</param>
    /// <param name="delaySource">Optional value task delay source to use for scheduling delays.</param>
    /// <param name="count">Number of events this timer will emit in total. Equal to <see cref="Int64.MaxValue"/> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="count"/> is less than <b>1</b>
    /// or when <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
    public ReactiveTimer(
        Duration interval,
        Duration spinWaitDurationHint,
        ITimestampProvider? timestampProvider = null,
        ValueTaskDelaySource? delaySource = null,
        long count = long.MaxValue)
        : base( new EventPublisher<WithInterval<long>>() )
    {
        Ensure.IsGreaterThan( count, 0 );
        Ensure.IsInRange( interval, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        Ensure.IsGreaterThanOrEqualTo( spinWaitDurationHint, Duration.Zero );

        Interval = interval;
        Count = count;
        _state = ReactiveTimerState.Idle;

        _timestampProvider = timestampProvider ?? TimestampProvider.Shared;
        _delaySource = delaySource is null ? DelaySource.Owned() : DelaySource.External( delaySource );
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
    public ReactiveTimerState State
    {
        get
        {
            using ( ExclusiveLock.Enter( Sync ) )
                return _state;
        }
    }

    /// <summary>
    /// Attempts to start this timer.
    /// </summary>
    /// <returns>
    /// <b>false</b> when timer was not started because it was not in <see cref="ReactiveTimerState.Idle"/> state,
    /// otherwise <b>true</b>.
    /// </returns>
    /// <exception cref="ObjectDisposedException">The timer has been disposed.</exception>
    public bool Start()
    {
        return RunCore( Interval );
    }

    /// <summary>
    /// Attempts to start this timer with an initial <paramref name="delay"/>.
    /// </summary>
    /// <param name="delay">Time that must elapse before emitting the first event.</param>
    /// <returns>
    /// <b>false</b> when timer was not started because it was not in <see cref="ReactiveTimerState.Idle"/> state,
    /// otherwise <b>true</b>.
    /// </returns>
    /// <exception cref="ObjectDisposedException">The timer has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="delay"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds.
    /// </exception>
    public bool Start(Duration delay)
    {
        Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        return RunCore( delay );
    }

    /// <summary>
    /// Attempts to stop this timer.
    /// </summary>
    /// <returns><b>true</b> when timer was marked for stopping, otherwise <b>false</b>.</returns>
    public bool Stop()
    {
        using ( ExclusiveLock.Enter( Sync ) )
        {
            if ( _state != ReactiveTimerState.Running )
                return false;

            _state = ReactiveTimerState.Stopping;
            _reset.Set();
            return true;
        }
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        DisposeAsync().AsTask().ConfigureAwait( false ).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return DisposeAsyncCore();
    }

    private async ValueTask DisposeAsyncCore(bool ignoreTask = false)
    {
        Task? task;
        ValueTaskDelaySource? ownedDelaySource;

        using ( ExclusiveLock.Enter( Sync ) )
        {
            if ( Base.IsDisposed )
                return;

            ownedDelaySource = _delaySource.DiscardOwnedSource();
            if ( ignoreTask )
            {
                task = null;
                MakeIdle();
                Base.Dispose();
            }
            else
            {
                task = _task;
                _task = null;

                Base.Dispose();
                if ( _state == ReactiveTimerState.Running )
                {
                    _state = ReactiveTimerState.Stopping;
                    _reset.Set();
                }
            }
        }

        if ( task is not null )
            await task.ConfigureAwait( false );

        if ( ownedDelaySource is not null )
            await ownedDelaySource.DisposeAsync().ConfigureAwait( false );
    }

    private bool RunCore(Duration delay)
    {
        using ( ExclusiveLock.Enter( Sync ) )
        {
            ObjectDisposedException.ThrowIf( Base.IsDisposed, this );
            if ( _state != ReactiveTimerState.Idle )
                return false;

            Assume.IsNull( _task );
            Assume.Equals( _reset, default );

            _state = ReactiveTimerState.Running;
            var delaySource = _delaySource.GetSource();
            _reset = delaySource.GetResetEvent();

            _prevStartTimestamp = _timestampProvider.GetNow();
            _expectedNextTimestamp = _prevStartTimestamp + delay;
        }

        var task = RunTimer();
        using ( ExclusiveLock.Enter( Sync ) )
        {
            if ( Base.IsDisposed )
                MakeIdle();
            else if ( _state == ReactiveTimerState.Running )
                _task = task;
        }

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void MakeIdle()
    {
        _task = null;
        _state = ReactiveTimerState.Idle;
        _reset.Dispose();
        _reset = default;
    }

    private bool TrySpinUntilTimestampReached(Timestamp expectedNextTimestamp, out Timestamp timestamp)
    {
        timestamp = _timestampProvider.GetNow();
        if ( timestamp >= expectedNextTimestamp )
            return true;

        var spinWait = new SpinWait();
        var lockAcquiredAt = timestamp;
        var lockAcquisitionInterval = Duration.FromMicroseconds( 500 );
        do
        {
            if ( timestamp - lockAcquiredAt > lockAcquisitionInterval )
            {
                using ( ExclusiveLock.Enter( Sync ) )
                {
                    if ( _state == ReactiveTimerState.Stopping )
                    {
                        MakeIdle();
                        return false;
                    }

                    Assume.Equals( _state, ReactiveTimerState.Running );
                }

                lockAcquiredAt = timestamp;
            }

            spinWait.SpinOnce();
            timestamp = _timestampProvider.GetNow();
        }
        while ( timestamp < expectedNextTimestamp );

        return true;
    }

    private async Task RunTimer()
    {
        Duration delay;
        Timestamp expectedNextTimestamp;

        using ( ExclusiveLock.Enter( Sync ) )
        {
            if ( _state == ReactiveTimerState.Stopping )
            {
                MakeIdle();
                return;
            }

            Assume.Equals( _state, ReactiveTimerState.Running );
            expectedNextTimestamp = _expectedNextTimestamp;
            delay = Duration.Zero.Max( expectedNextTimestamp - _prevStartTimestamp - _spinWaitDurationHint );
        }

        while ( true )
        {
            var result = await _reset.WaitAsync( delay ).ConfigureAwait( false );
            if ( result == AsyncManualResetEventResult.Disposed )
            {
                if ( ! Base.IsDisposed )
                    await DisposeAsyncCore( ignoreTask: true ).ConfigureAwait( false );

                return;
            }

            if ( ! TrySpinUntilTimestampReached( expectedNextTimestamp, out var timestamp ) )
                return;

            var dispose = false;
            using ( ExclusiveLock.Enter( Sync ) )
            {
                if ( _state == ReactiveTimerState.Stopping )
                {
                    MakeIdle();
                    return;
                }

                Assume.Equals( _state, ReactiveTimerState.Running );
                var interval = timestamp - _prevStartTimestamp;
                _prevStartTimestamp = timestamp;

                var offsetFromExpectedTimestamp = timestamp - expectedNextTimestamp;
                var skippedEventCount = offsetFromExpectedTimestamp.Ticks / Interval.Ticks;
                var eventIndex = Math.Min( _prevIndex + skippedEventCount + 1, _expectedLastIndex );
                var nextEvent = new WithInterval<long>( eventIndex, timestamp, interval );

                Base.Publish( nextEvent );

                if ( nextEvent.Event == _expectedLastIndex )
                {
                    _prevIndex = nextEvent.Event;
                    dispose = true;
                }
                else
                {
                    var endTimestamp = _timestampProvider.GetNow();
                    var actualOffsetFromExpectedTimestamp = endTimestamp - expectedNextTimestamp;
                    var actualSkippedEventCount = actualOffsetFromExpectedTimestamp.Ticks / Interval.Ticks;
                    expectedNextTimestamp += Duration.FromTicks( Interval.Ticks * (actualSkippedEventCount + 1) );
                    _expectedNextTimestamp = expectedNextTimestamp;
                    _prevIndex = Math.Min( nextEvent.Event + (actualSkippedEventCount - skippedEventCount), _expectedLastIndex );

                    delay = Duration.Zero.Max( expectedNextTimestamp - endTimestamp - _spinWaitDurationHint );
                    _reset.Reset();
                }
            }

            if ( dispose )
            {
                await DisposeAsyncCore( ignoreTask: true ).ConfigureAwait( false );
                return;
            }
        }
    }
}
