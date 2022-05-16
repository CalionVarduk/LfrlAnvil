using System;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.Events
{
    public interface IEventSource : IEventStream, IDisposable
    {
        bool HasSubscribers { get; }
        IReadOnlyCollection<IEventSubscriber> Subscribers { get; }
    }

    public interface IEventSource<out TEvent> : IEventStream<TEvent>, IEventSource { }
}
