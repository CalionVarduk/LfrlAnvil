using System;

namespace LfrlAnvil.Reactive.Queues.Exceptions
{
    public class EventDequeueException : InvalidOperationException
    {
        public EventDequeueException()
            : base( Resources.NoEventsCanBeDequeuedAtThisPoint ) { }
    }
}
