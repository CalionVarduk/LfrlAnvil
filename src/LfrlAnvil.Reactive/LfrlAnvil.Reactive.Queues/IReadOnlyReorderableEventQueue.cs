using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues;

/// <summary>
/// Represents a generic read-only event queue that allows to modify registered events.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TPoint">Queue point type.</typeparam>
/// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
public interface IReadOnlyReorderableEventQueue<TEvent, TPoint, TPointDelta> : IReadOnlyEventQueue<TEvent, TPoint, TPointDelta>
{
    /// <summary>
    /// Event equality comparer.
    /// </summary>
    IEqualityComparer<TEvent> EventComparer { get; }

    /// <summary>
    /// Checks whether or not this queue contains the specified <paramref name="event"/>.
    /// </summary>
    /// <param name="event">Event to check.</param>
    /// <returns><b>true</b> when <paramref name="event"/> exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(TEvent @event);

    /// <summary>
    /// Attempts to return <paramref name="event"/> information.
    /// </summary>
    /// <param name="event">Event to get information for.</param>
    /// <returns>
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance associated with the provided <paramref name="event"/>
    /// or null when event does not exist.
    /// </returns>
    [Pure]
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? GetEvent(TEvent @event);
}
