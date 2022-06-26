namespace LfrlAnvil.Reactive.Decorators;

public class EventListenerFirstDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, subscriber );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly IEventSubscriber _subscriber;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber)
            : base( next )
        {
            _subscriber = subscriber;
        }

        public override void React(TEvent @event)
        {
            Next.React( @event );
            _subscriber.Dispose();
        }
    }
}
