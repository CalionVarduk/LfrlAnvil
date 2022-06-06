namespace LfrlAnvil.Reactive.Decorators
{
    public class EventListenerIgnoreDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
    {
        public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
        {
            return new EventListener( listener );
        }

        private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
        {
            internal EventListener(IEventListener<TEvent> next)
                : base( next ) { }

            public override void React(TEvent _) { }
        }
    }
}
