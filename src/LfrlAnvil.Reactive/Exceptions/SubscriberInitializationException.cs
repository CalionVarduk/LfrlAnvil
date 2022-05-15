using System;

namespace LfrlAnvil.Reactive.Exceptions
{
    public class SubscriberInitializationException : InvalidOperationException
    {
        public SubscriberInitializationException()
            : base( Resources.SubscriberIsAlreadyInitialized ) { }
    }
}
