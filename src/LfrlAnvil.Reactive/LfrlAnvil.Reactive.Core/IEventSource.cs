using System;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents a type-erased disposable event source that can be listened to.
/// </summary>
public interface IEventSource : IEventStream, IDisposable
{
    /// <summary>
    /// Specifies whether or not this event source has any event subscribers.
    /// </summary>
    bool HasSubscribers { get; }

    /// <summary>
    /// Collection of currently attached event subscribers to this event source.
    /// </summary>
    IReadOnlyCollection<IEventSubscriber> Subscribers { get; }
}

/// <summary>
/// Represents a generic disposable event source that can be listened to.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public interface IEventSource<out TEvent> : IEventStream<TEvent>, IEventSource { }
