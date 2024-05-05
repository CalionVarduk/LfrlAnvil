using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues;

/// <summary>
/// Represents a generic read-only event queue.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TPoint">Queue point type.</typeparam>
/// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
public interface IReadOnlyEventQueue<TEvent, TPoint, TPointDelta> : IReadOnlyCollection<EnqueuedEvent<TEvent, TPoint, TPointDelta>>
{
    /// <summary>
    /// Specifies the starting point of this queue.
    /// </summary>
    TPoint StartPoint { get; }

    /// <summary>
    /// Specifies the current point that this queue is in.
    /// </summary>
    TPoint CurrentPoint { get; }

    /// <summary>
    /// Queue point comparer.
    /// </summary>
    IComparer<TPoint> Comparer { get; }

    /// <summary>
    /// Attempts to return information about the next event to happen.
    /// </summary>
    /// <returns><see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance or null when this queue is empty.</returns>
    [Pure]
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? GetNext();

    /// <summary>
    /// Returns information about all currently registered events in this queue,
    /// from <see cref="CurrentPoint"/> to the specified <paramref name="endPoint"/>.
    /// </summary>
    /// <param name="endPoint">Largest event point to include in the result.</param>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    IEnumerable<EnqueuedEvent<TEvent, TPoint, TPointDelta>> GetEvents(TPoint endPoint);
}
