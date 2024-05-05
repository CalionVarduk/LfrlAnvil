using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues;

/// <summary>
/// Represents a generic mutable event queue.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TPoint">Queue point type.</typeparam>
/// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
public interface IMutableEventQueue<TEvent, TPoint, TPointDelta> : IEventQueue<TEvent, TPoint, TPointDelta>
{
    /// <summary>
    /// Moves the <see cref="IReadOnlyEventQueue{TEvent,TPoint,TPointDelta}.CurrentPoint"/> forward.
    /// </summary>
    /// <param name="delta">
    /// Point delta to move the <see cref="IReadOnlyEventQueue{TEvent,TPoint,TPointDelta}.CurrentPoint"/> forward by.
    /// </param>
    void Move(TPointDelta delta);

    /// <summary>
    /// Removes all events from this queue.
    /// </summary>
    void Clear();

    /// <summary>
    /// Attempts to dequeue the next event that should be processed.
    /// </summary>
    /// <returns>
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance or null when no events are waiting for processing.
    /// </returns>
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? Dequeue();
}
