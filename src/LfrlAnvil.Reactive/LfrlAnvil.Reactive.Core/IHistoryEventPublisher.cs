using System.Collections.Generic;

namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents a generic disposable event source that can be listened to, capable of recording previously published events.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public interface IHistoryEventPublisher<TEvent> : IEventPublisher<TEvent>
{
    /// <summary>
    /// Specifies the maximum number of events this event publisher can record.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Collection of recorded previously published events.
    /// </summary>
    IReadOnlyCollection<TEvent> History { get; }

    /// <summary>
    /// Removes all recorded events.
    /// </summary>
    void ClearHistory();
}
