using System;

namespace LfrlAnvil.Reactive.Decorators;

public class EventListenerTakeWhileDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly Func<TEvent, bool> _predicate;

    public EventListenerTakeWhileDecorator(Func<TEvent, bool> predicate)
    {
        _predicate = predicate;
    }

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