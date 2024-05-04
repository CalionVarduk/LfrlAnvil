using System;

namespace LfrlAnvil.Reactive.Exceptions;

/// <summary>
/// Represents an error that occurred due to a duplicated event publisher.
/// </summary>
public class EventPublisherAlreadyExistsException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="EventPublisherAlreadyExistsException"/> instance.
    /// </summary>
    /// <param name="eventType">Event type.</param>
    public EventPublisherAlreadyExistsException(Type eventType)
        : base( Resources.EventPublisherAlreadyExists( eventType ) )
    {
        EventType = eventType;
    }

    /// <summary>
    /// Event type.
    /// </summary>
    public Type EventType { get; }
}
