using System;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.Events
{
    public interface IEventSource : IEventStream, IDisposable
    {
        bool HasSubscribers { get; }
        IReadOnlyCollection<IEventSubscriber> Subscribers { get; }

        void Publish(object? @event);
    }

    public interface IEventSource<TEvent> : IEventStream<TEvent>, IEventSource
    {
        void Publish(TEvent @event);
    }
}
