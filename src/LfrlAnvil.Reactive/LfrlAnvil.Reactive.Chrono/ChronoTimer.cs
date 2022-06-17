using System;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Chrono
{
    public sealed class ChronoTimer : ConcurrentEventSource<WithInterval<long>, EventPublisher<WithInterval<long>>>
    {
        public static readonly Duration DefaultSpinWaitDurationHint = Duration.FromMilliseconds( 1 );

        private readonly ITimestampProvider _timestampProvider;
        private readonly ManualResetEventSlim _reset;
        private readonly Duration _spinWaitDurationHint;
        private readonly long _expectedLastIndex;
        private Timestamp _prevStartTimestamp;
        private Timestamp _prevEndTimestamp;
        private Timestamp _expectedNextTimestamp;
        private long _prevIndex;

        public ChronoTimer(ITimestampProvider timestampProvider, Duration interval)
            : this( timestampProvider, interval, long.MaxValue ) { }

        public ChronoTimer(ITimestampProvider timestampProvider, Duration interval, long count)
            : this( timestampProvider, interval, DefaultSpinWaitDurationHint, count ) { }

        public ChronoTimer(ITimestampProvider timestampProvider, Duration interval, Duration spinWaitDurationHint)
            : this( timestampProvider, interval, spinWaitDurationHint, long.MaxValue ) { }

        public ChronoTimer(ITimestampProvider timestampProvider, Duration interval, Duration spinWaitDurationHint, long count)
            : base( new EventPublisher<WithInterval<long>>() )
        {
            Ensure.IsGreaterThan( count, 0, nameof( count ) );
            Ensure.IsInRange( interval, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ), nameof( interval ) );
            Ensure.IsGreaterThanOrEqualTo( spinWaitDurationHint, Duration.Zero, nameof( spinWaitDurationHint ) );

            Interval = interval;
            Count = count;
            IsRunning = false;

            _timestampProvider = timestampProvider;
            _reset = new ManualResetEventSlim( false );
            _expectedLastIndex = Count - 1;
            _spinWaitDurationHint = spinWaitDurationHint;
            _prevStartTimestamp = Timestamp.Zero;
            _prevEndTimestamp = Timestamp.Zero;
            _expectedNextTimestamp = Timestamp.Zero;
            _prevIndex = -1;
        }

        public Duration Interval { get; }
        public long Count { get; }
        public bool IsRunning { get; private set; }

        public void Start()
        {
            EnsureNotDisposed();
            StartInternal( Interval );
        }

        public void Start(Duration delay)
        {
            EnsureNotDisposed();
            Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ), nameof( delay ) );
            StartInternal( delay );
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
            Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ), nameof( delay ) );
            return StartInternal( delay, Task.Factory );
        }

        public Task? StartAsync(TaskScheduler scheduler, Duration delay)
        {
            EnsureNotDisposed();
            Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ), nameof( delay ) );

            var taskFactory = new TaskFactory( scheduler );
            return StartInternal( delay, taskFactory );
        }

        public void Stop()
        {
            EnsureNotDisposed();

            lock ( Sync )
            {
                if ( ! IsRunning )
                    return;

                IsRunning = false;
                _reset.Set();
            }
        }

        public override void Dispose()
        {
            lock ( Sync )
            {
                if ( Base.IsDisposed )
                    return;

                IsRunning = false;
                _reset.Set();
                _reset.Dispose();
                Base.Dispose();
            }
        }

        private Task? StartInternal(Duration delay, TaskFactory? taskFactory = null)
        {
            lock ( Sync )
            {
                if ( IsRunning || _prevIndex == _expectedLastIndex )
                    return null;

                IsRunning = true;
                _prevStartTimestamp = _timestampProvider.GetNow();
                _prevEndTimestamp = _prevStartTimestamp;
                _expectedNextTimestamp = _prevEndTimestamp + delay;
                _reset.Reset();
            }

            if ( taskFactory is not null )
                return taskFactory.StartNew( RunBlockingTimer );

            RunBlockingTimer();
            return null;
        }

        private void RunBlockingTimer()
        {
            while ( true )
            {
                Duration initialDelay;

                lock ( Sync )
                {
                    if ( ! IsRunning )
                        break;

                    initialDelay = Duration.Zero.Max( _expectedNextTimestamp - _prevEndTimestamp - _spinWaitDurationHint );
                }

                _reset.Wait( (TimeSpan)initialDelay );

                lock ( Sync )
                {
                    if ( ! IsRunning )
                        break;

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

                    _prevEndTimestamp = _timestampProvider.GetNow();
                    var actualOffsetFromExpectedTimestamp = _prevEndTimestamp - _expectedNextTimestamp;
                    var actualSkippedEventCount = actualOffsetFromExpectedTimestamp.Ticks / Interval.Ticks;
                    _expectedNextTimestamp += new Duration( Interval.Ticks * (actualSkippedEventCount + 1) );
                    _prevIndex = Math.Min( eventIndex + (actualSkippedEventCount - skippedEventCount), _expectedLastIndex );
                    _reset.Reset();
                }
            }
        }
    }
}
