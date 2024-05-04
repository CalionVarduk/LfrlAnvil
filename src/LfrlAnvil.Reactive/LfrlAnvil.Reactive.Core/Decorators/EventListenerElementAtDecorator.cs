namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener with a single emitted event at the specified 0-based index in a sequence of emitted events.
/// Does not notify the decorated event listener if no such event was emitted.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerElementAtDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly int _index;

    /// <summary>
    /// Creates a new <see cref="EventListenerElementAtDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="index">0-based position of the desired event.</param>
    public EventListenerElementAtDecorator(int index)
    {
        _index = index;
    }

    /// <inheritdoc />
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
