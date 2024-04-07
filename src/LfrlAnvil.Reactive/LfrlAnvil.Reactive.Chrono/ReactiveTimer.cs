using System;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Chrono;

public sealed class ReactiveTimer : ConcurrentEventSource<WithInterval<long>, EventPublisher<WithInterval<long>>>
{
    public static readonly Duration DefaultSpinWaitDurationHint = Duration.FromMilliseconds( 1 );
    private const byte StoppedState = 0;
    private const byte RunningState = 1;
    private const byte StoppingState = 2;

    private readonly ITimestampProvider _timestampProvider;
    private readonly ManualResetEventSlim _reset;
    private readonly Duration _spinWaitDurationHint;
    private readonly long _expectedLastIndex;
    private Timestamp _prevStartTimestamp;
    private Timestamp _expectedNextTimestamp;
    private long _prevIndex;
    private byte _state;

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
        _state = StoppedState;

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
    public bool IsRunning => _state == RunningState;
    public bool CanBeStarted => _state == StoppedState && ! Base.IsDisposed && _prevIndex != _expectedLastIndex;

    public bool Start()
    {
        EnsureNotDisposed();
        return StartInternal( Interval ) is not null;
    }

    public bool Start(Duration delay)
    {
        EnsureNotDisposed();
        Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        return StartInternal( delay ) is not null;
    }

    public Task? StartAsync()
    {
        EnsureNotDisposed();
        return StartInternal( Interval, Task.Factory );
    }

    public Task? StartAsync(TaskScheduler scheduler)
    {
        EnsureNotDisposed();
        var taskFactory = new TaskFactory( scheduler );
        return StartInternal( Interval, taskFactory );
    }

    public Task? StartAsync(Duration delay)
    {
        EnsureNotDisposed();
        Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        return StartInternal( delay, Task.Factory );
    }

    public Task? StartAsync(TaskScheduler scheduler, Duration delay)
    {
        EnsureNotDisposed();
        Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );

        var taskFactory = new TaskFactory( scheduler );
        return StartInternal( delay, taskFactory );
    }

    public bool Stop()
    {
        EnsureNotDisposed();

        lock ( Sync )
        {
            if ( ! IsRunning )
                return false;

            _state = StoppingState;
            _reset.Set();
            return true;
        }
    }

    public override void Dispose()
    {
        lock ( Sync )
        {
            if ( Base.IsDisposed )
                return;

            if ( IsRunning )
                _state = StoppingState;

            _reset.Set();
            _reset.Dispose();
            Base.Dispose();
        }
    }

    private Task? StartInternal(Duration delay, TaskFactory? taskFactory = null)
    {
        lock ( Sync )
        {
            if ( _state != StoppedState || _prevIndex == _expectedLastIndex )
                return null;

            _reset.Reset();
            _state = RunningState;
            _prevStartTimestamp = _timestampProvider.GetNow();
            _expectedNextTimestamp = _prevStartTimestamp + delay;
        }

        if ( taskFactory is not null )
            return taskFactory.StartNew( RunBlockingTimer );

        RunBlockingTimer();
        return Task.CompletedTask;
    }

    private void RunBlockingTimer()
    {
        Duration initialDelay;

        lock ( Sync )
        {
            if ( ! IsRunning )
            {
                _state = StoppedState;
                return;
            }

            initialDelay = Duration.Zero.Max( _expectedNextTimestamp - _prevStartTimestamp - _spinWaitDurationHint );
        }

        while ( true )
        {
            try
            {
                _reset.Wait( initialDelay );
            }
            catch ( ObjectDisposedException ) { }

            lock ( Sync )
            {
                if ( ! IsRunning )
                {
                    _state = StoppedState;
                    break;
                }

                var timestamp = _timestampProvider.GetNow();
                while ( timestamp < _expectedNextTimestamp )
                {
                    Thread.SpinWait( 1 );
                    timestamp = _timestampProvider.GetNow();
                }

                var interval = timestamp - _prevStartTimestamp;
                _prevStartTimestamp = timestamp;

                var offsetFromExpectedTimestamp = timestamp - _expectedNextTimestamp;
                var skippedEventCount = offsetFromExpectedTimestamp.Ticks / Interval.Ticks;
                var eventIndex = Math.Min( _prevIndex + skippedEventCount + 1, _expectedLastIndex );
                var nextEvent = new WithInterval<long>( eventIndex, timestamp, interval );

                Base.Publish( nextEvent );

                if ( eventIndex == _expectedLastIndex )
                {
                    _prevIndex = eventIndex;
                    Dispose();
                    break;
                }

                var endTimestamp = _timestampProvider.GetNow();
                var actualOffsetFromExpectedTimestamp = endTimestamp - _expectedNextTimestamp;
                var actualSkippedEventCount = actualOffsetFromExpectedTimestamp.Ticks / Interval.Ticks;
                _expectedNextTimestamp += new Duration( Interval.Ticks * (actualSkippedEventCount + 1) );
                _prevIndex = Math.Min( eventIndex + (actualSkippedEventCount - skippedEventCount), _expectedLastIndex );

                if ( ! IsRunning )
                {
                    _state = StoppedState;
                    break;
                }

                initialDelay = Duration.Zero.Max( _expectedNextTimestamp - endTimestamp - _spinWaitDurationHint );
                _reset.Reset();
            }
        }
    }
}
