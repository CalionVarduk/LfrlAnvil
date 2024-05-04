using System;

namespace LfrlAnvil.Reactive.Exceptions;

/// <summary>
/// Represents an error that occurred due to a missing event publisher.
/// </summary>
public class EventPublisherNotFoundException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="EventPublisherNotFoundException"/> instance.
    /// </summary>
    /// <param name="eventType">Event type.</param>
    public EventPublisherNotFoundException(Type eventType)
        : base( Resources.EventPublisherNotFound( eventType ) )
    {
        EventType = eventType;
    }

    /// <summary>
    /// Event type.
    /// </summary>
    public Type EventType { get; }
}
