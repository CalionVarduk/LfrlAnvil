using System;

namespace LfrlAnvil.Reactive.Decorators;

public class EventListenerTakeDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly int _count;

    public EventListenerTakeDecorator(int count)
    {
        _count = Math.Max( count, 0 );
    }

    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, subscriber, _count );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly IEventSubscriber _subscriber;
        private readonly int _count;
        private int _taken;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, int count)
            : base( next )
        {
            _subscriber = subscriber;
            _count = count;
            _taken = 0;

            if ( _count == 0 )
                _subscriber.Dispose();
        }

        public override void React(TEvent @event)
        {
            Next.React( @event );

            if ( ++_taken == _count )
                _subscriber.Dispose();
        }
    }
}