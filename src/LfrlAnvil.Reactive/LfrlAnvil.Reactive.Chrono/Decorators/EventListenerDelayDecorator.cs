using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Internal;

namespace LfrlAnvil.Reactive.Chrono.Decorators;

/// <summary>
/// Notifies decorated event listener with delayed emitted events.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerDelayDecorator<TEvent> : IEventListenerDecorator<TEvent, WithInterval<TEvent>>
{
    private readonly ITimestampProvider _timestampProvider;
    private readonly Duration _delay;
    private readonly TaskScheduler? _scheduler;
    private readonly Duration _spinWaitDurationHint;

    /// <summary>
    /// Creates a new <see cref="EventListenerDelayDecorator{TEvent}"/> instance,
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider to use for time tracking.</param>
    /// <param name="delay">Event delay.</param>
    /// <param name="scheduler">Optional task scheduler.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for the underlying timer.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="delay"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
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

    /// <inheritdoc />
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
