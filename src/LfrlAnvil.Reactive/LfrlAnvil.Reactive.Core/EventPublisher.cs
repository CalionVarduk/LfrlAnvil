using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive;

/// <inheritdoc cref="IEventPublisher{TEvent}" />
public class EventPublisher<TEvent> : EventSource<TEvent>, IEventPublisher<TEvent>
{
    /// <inheritdoc />
    public void Publish(TEvent @event)
    {
        EnsureNotDisposed();
        OnPublish( @event );
    }

    /// <summary>
    /// Allows to react to event publishing.
    /// </summary>
    /// <param name="event">Event to publish.</param>
    protected virtual void OnPublish(TEvent @event)
    {
        NotifyListeners( @event );
    }

    void IEventPublisher.Publish(object? @event)
    {
        Publish( Argument.CastTo<TEvent>( @event ) );
    }
}
