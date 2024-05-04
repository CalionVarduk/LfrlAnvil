namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents a type-erased disposable event publisher that can be listened to.
/// </summary>
public interface IEventPublisher : IEventSource
{
    /// <summary>
    /// Publishes an event that notifies all current event listeners.
    /// </summary>
    /// <param name="event">Event to publish.</param>
    void Publish(object? @event);
}

/// <summary>
/// Represents a generic disposable event source that can be listened to.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public interface IEventPublisher<TEvent> : IEventSource<TEvent>, IEventPublisher
{
    /// <summary>
    /// Publishes an event that notifies all current event listeners.
    /// </summary>
    /// <param name="event">Event to publish.</param>
    void Publish(TEvent @event);
}
