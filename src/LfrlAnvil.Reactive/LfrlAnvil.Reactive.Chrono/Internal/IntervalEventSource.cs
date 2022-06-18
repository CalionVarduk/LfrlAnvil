using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;

namespace LfrlAnvil.Reactive.Chrono.Internal
{
    public sealed class IntervalEventSource : EventSource<WithInterval<long>>
    {
        private readonly ITimestampProvider _timestampProvider;
        private readonly Duration _interval;
        private readonly TaskScheduler? _scheduler;
        private readonly Duration _spinWaitDurationHint;
        private readonly long _count;

        internal IntervalEventSource(
            ITimestampProvider timestampProvider,
            Duration interval,
            TaskScheduler? scheduler,
            Duration spinWaitDurationHint,
            long count)
        {
            Ensure.IsGreaterThan( count, 0, nameof( count ) );
            Ensure.IsInRange( interval, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ), nameof( interval ) );
            Ensure.IsGreaterThanOrEqualTo( spinWaitDurationHint, Duration.Zero, nameof( spinWaitDurationHint ) );

            _timestampProvider = timestampProvider;
            _interval = interval;
            _scheduler = scheduler;
            _spinWaitDurationHint = spinWaitDurationHint;
            _count = count;
        }

        protected override IEventListener<WithInterval<long>> OverrideListener(
            IEventSubscriber subscriber,
            IEventListener<WithInterval<long>> listener)
        {
            if ( IsDisposed )
                return listener;

            var timer = new ReactiveTimer( _timestampProvider, _interval, _spinWaitDurationHint, _count );
            return new EventListener( listener, subscriber, timer, _scheduler );
        }

        private sealed class EventListener : DecoratedEventListener<WithInterval<long>, WithInterval<long>>
        {
            private ReactiveTimer? _timer;

            internal EventListener(
                IEventListener<WithInterval<long>> next,
                IEventSubscriber subscriber,
                ReactiveTimer timer,
                TaskScheduler? scheduler)
                : base( next )
            {
                _timer = timer;
                var timerListener = new TimerListener( this, subscriber );
                _timer.Listen( timerListener );

                if ( scheduler is null )
                {
                    _timer.StartAsync();
                    return;
                }

                _timer.StartAsync( scheduler );
            }

            public override void React(WithInterval<long> @event)
            {
                Next.React( @event );
            }

            public override void OnDispose(DisposalSource source)
            {
                _timer!.Dispose();
                _timer = null;
                base.OnDispose( source );
            }
        }

        private sealed class TimerListener : EventListener<WithInterval<long>>
        {
            private EventListener? _mainListener;
            private IEventSubscriber? _mainSubscriber;

            internal TimerListener(EventListener mainListener, IEventSubscriber mainSubscriber)
            {
                _mainListener = mainListener;
                _mainSubscriber = mainSubscriber;
            }

            public override void React(WithInterval<long> @event)
            {
                _mainListener!.React( @event );
            }

            public override void OnDispose(DisposalSource _)
            {
                _mainSubscriber!.Dispose();
                _mainSubscriber = null;
                _mainListener = null;
            }
        }
    }
}
