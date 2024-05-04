namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener with the first emitted event, unless no events have been emitted.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerFirstDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    /// <inheritdoc />
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
