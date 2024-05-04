using System;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener only with events emitted at the beginning of the sequence
/// until an event fails the provided predicate, before disposing the subscriber.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerTakeWhileDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly Func<TEvent, bool> _predicate;

    /// <summary>
    /// Creates a new <see cref="EventListenerTakeWhileDecorator{TEvent}"/> instance,
    /// </summary>
    /// <param name="predicate">Predicate that takes events until the first event that fails it (returns <b>false</b>).</param>
    public EventListenerTakeWhileDecorator(Func<TEvent, bool> predicate)
    {
        _predicate = predicate;
    }

    /// <inheritdoc />
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, subscriber, _predicate );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly IEventSubscriber _subscriber;
        private readonly Func<TEvent, bool> _predicate;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, Func<TEvent, bool> predicate)
            : base( next )
        {
            _subscriber = subscriber;
            _predicate = predicate;
        }

        public override void React(TEvent @event)
        {
            if ( _predicate( @event ) )
            {
                Next.React( @event );
                return;
            }

            _subscriber.Dispose();
        }
    }
}
