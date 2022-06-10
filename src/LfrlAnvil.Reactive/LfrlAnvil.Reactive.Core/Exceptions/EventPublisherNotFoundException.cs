using System;

namespace LfrlAnvil.Reactive.Exceptions
{
    public class EventPublisherNotFoundException : ArgumentException
    {
        public EventPublisherNotFoundException(Type eventType)
            : base( ExceptionResources.EventPublisherNotFound( eventType ) )
        {
            EventType = eventType;
        }

        public Type EventType { get; }
    }
}
