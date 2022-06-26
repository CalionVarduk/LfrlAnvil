using System;

namespace LfrlAnvil.Reactive.Exceptions;

public class EventPublisherAlreadyExistsException : ArgumentException
{
    public EventPublisherAlreadyExistsException(Type eventType)
        : base( Resources.EventPublisherAlreadyExists( eventType ) )
    {
        EventType = eventType;
    }

    public Type EventType { get; }
}
