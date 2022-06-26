namespace LfrlAnvil.Reactive.Decorators;

public class EventListenerElementAtDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly int _index;

    public EventListenerElementAtDecorator(int index)
    {
        _index = index;
    }

    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, subscriber, _index );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly IEventSubscriber _subscriber;
        private readonly int _index;
        private int _currentIndex;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, int index)
            : base( next )
        {
            _subscriber = subscriber;
            _index = index;
            _currentIndex = -1;

            if ( _index < 0 )
                _subscriber.Dispose();
        }

        public override void React(TEvent @event)
        {
            if ( ++_currentIndex < _index )
                return;

            Next.React( @event );
            _subscriber.Dispose();
        }
    }
}
