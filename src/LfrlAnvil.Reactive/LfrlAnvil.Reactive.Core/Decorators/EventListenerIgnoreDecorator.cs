namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Does not notify the decorated event listener.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerIgnoreDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    /// <inheritdoc />
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
