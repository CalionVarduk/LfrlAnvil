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

public sealed class ReactiveTimer : ConcurrentEventSource<WithInterval<long>, EventPublisher<WithInterval<long>>>
{
    public static readonly Duration DefaultSpinWaitDurationHint = Duration.FromMicroseconds( 1 );

    private readonly ITimestampProvider _timestampProvider;
    private readonly ManualResetEventSlim _reset;
    private readonly Duration _spinWaitDurationHint;
    private readonly long _expectedLastIndex;
    private Timestamp _prevStartTimestamp;
    private Timestamp _expectedNextTimestamp;
    private long _prevIndex;
    private InterlockedEnum<ReactiveTimerState> _state;

    public ReactiveTimer(ITimestampProvider timestampProvider, Duration interval, long count = long.MaxValue)
        : this( timestampProvider, interval, DefaultSpinWaitDurationHint, count ) { }

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

    public Duration Interval { get; }
    public long Count { get; }
    public ReactiveTimerState State => _state.Value;

    public bool Start()
    {
        return TryStartCore( Interval ).Result == StartResult.Started;
    }

    public bool Start(Duration delay)
    {
        Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        return TryStartCore( delay ).Result == StartResult.Started;
    }

    public Task StartAsync()
    {
        var (task, result) = TryStartCore( Interval, Task.Factory );
        return GetStartedTask( task, result );
    }

    public Task StartAsync(TaskScheduler scheduler)
    {
        var taskFactory = new TaskFactory( scheduler );
        var (task, result) = TryStartCore( Interval, taskFactory );
        return GetStartedTask( task, result );
    }

    public Task StartAsync(Duration delay)
    {
        Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        var (task, result) = TryStartCore( delay, Task.Factory );
        return GetStartedTask( task, result );
    }

    public Task StartAsync(TaskScheduler scheduler, Duration delay)
    {
        Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        var taskFactory = new TaskFactory( scheduler );
        var (task, result) = TryStartCore( delay, taskFactory );
        return GetStartedTask( task, result );
    }

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
