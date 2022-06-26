using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;

namespace LfrlAnvil.Reactive.Chrono.Decorators;

public sealed class EventListenerWithZonedDateTimeDecorator<TEvent> : IEventListenerDecorator<TEvent, WithZonedDateTime<TEvent>>
{
    private readonly IZonedClock _clock;

    public EventListenerWithZonedDateTimeDecorator(IZonedClock clock)
    {
        _clock = clock;
    }

    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<WithZonedDateTime<TEvent>> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _clock );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, WithZonedDateTime<TEvent>>
    {
        private readonly IZonedClock _clock;

        internal EventListener(IEventListener<WithZonedDateTime<TEvent>> next, IZonedClock clock)
            : base( next )
        {
            _clock = clock;
        }

        public override void React(TEvent @event)
        {
            var dateTime = _clock.GetNow();
            var nextEvent = new WithZonedDateTime<TEvent>( @event, dateTime );
            Next.React( nextEvent );
        }
    }
}
