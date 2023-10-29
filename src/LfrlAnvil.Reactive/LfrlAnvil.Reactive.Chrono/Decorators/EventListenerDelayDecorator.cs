using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Internal;

namespace LfrlAnvil.Reactive.Chrono.Decorators;

public sealed class EventListenerDelayDecorator<TEvent> : IEventListenerDecorator<TEvent, WithInterval<TEvent>>
{
    private readonly ITimestampProvider _timestampProvider;
    private readonly Duration _delay;
    private readonly TaskScheduler? _scheduler;
    private readonly Duration _spinWaitDurationHint;

    public EventListenerDelayDecorator(
        ITimestampProvider timestampProvider,
        Duration delay,
        TaskScheduler? scheduler,
        Duration spinWaitDurationHint)
    {
        Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        Ensure.IsGreaterThanOrEqualTo( spinWaitDurationHint, Duration.Zero );

        _timestampProvider = timestampProvider;
        _delay = delay;
        _scheduler = scheduler;
        _spinWaitDurationHint = spinWaitDurationHint;
    }

    public IEventListener<TEvent> Decorate(IEventListener<WithInterval<TEvent>> listener, IEventSubscriber subscriber)
    {
        var timeout = new IntervalEventSource( _timestampProvider, _delay, _scheduler, _spinWaitDurationHint, count: 1 );
        return new EventListener( listener, timeout );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, WithInterval<TEvent>>
    {
        private IntervalEventSource? _timeout;

        internal EventListener(IEventListener<WithInterval<TEvent>> next, IntervalEventSource timeout)
            : base( next )
        {
            _timeout = timeout;
        }

        public override void React(TEvent @event)
        {
            Assume.IsNotNull( _timeout );
            var timerListener = new TimerListener( this, @event );
            _timeout.Listen( timerListener );
        }

        public override void OnDispose(DisposalSource source)
        {
            Assume.IsNotNull( _timeout );
            _timeout.Dispose();
            _timeout = null;
            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTimerReact(TEvent @event, WithInterval<long> timerEvent)
        {
            var nextEvent = new WithInterval<TEvent>( @event, timerEvent.Timestamp, timerEvent.Interval );
            Next.React( nextEvent );
        }
    }

    private sealed class TimerListener : EventListener<WithInterval<long>>
    {
        private EventListener? _mainListener;
        private TEvent? _event;

        internal TimerListener(EventListener mainListener, TEvent @event)
        {
            _mainListener = mainListener;
            _event = @event;
        }

        public override void React(WithInterval<long> @event)
        {
            Assume.IsNotNull( _mainListener );
            _mainListener.OnTimerReact( _event!, @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _mainListener = null;
            _event = default;
        }
    }
}
