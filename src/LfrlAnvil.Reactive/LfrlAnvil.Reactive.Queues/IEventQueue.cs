using System;
using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues;

/// <summary>
/// Represents a generic event queue.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TPoint">Queue point type.</typeparam>
/// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
public interface IEventQueue<TEvent, TPoint, TPointDelta> : IReadOnlyEventQueue<TEvent, TPoint, TPointDelta>
{
    /// <summary>
    /// Adds a new repeatable event to this queue with its <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.DequeuePoint"/> equal to
    /// <see cref="IReadOnlyEventQueue{TEvent,TPoint,TPointDelta}.CurrentPoint"/> moved by the specified <paramref name="delta"/>.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="delta">
    /// Point delta used for moving <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.DequeuePoint"/>
    /// forward on each repetition of this event.
    /// </param>
    /// <param name="repetitions">Number of repetitions.</param>
    /// <returns>New <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="repetitions"/> is less than <b>1</b>.</exception>
    EnqueuedEvent<TEvent, TPoint, TPointDelta> Enqueue(TEvent @event, TPointDelta delta, int repetitions);

    /// <summary>
    /// Adds a new repeatable event to this queue.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="dequeuePoint">Queue point at which this event should be dequeued for the first time.</param>
    /// <param name="delta">
    /// Point delta used for moving <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.DequeuePoint"/>
    /// forward on each repetition of this event.
    /// </param>
    /// <param name="repetitions">Number of repetitions.</param>
    /// <returns>New <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="repetitions"/> is less than <b>1</b>.</exception>
    EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueAt(TEvent @event, TPoint dequeuePoint, TPointDelta delta, int repetitions);

    /// <summary>
    /// Adds a new event to this queue that happens exactly once with its
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.DequeuePoint"/> equal to
    /// <see cref="IReadOnlyEventQueue{TEvent,TPoint,TPointDelta}.CurrentPoint"/> moved by the specified <paramref name="delta"/>.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="delta">Point delta used for moving <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.DequeuePoint"/> forward.</param>
    /// <returns>New <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    EnqueuedEvent<TEvent, TPoint, TPointDelta> Enqueue(TEvent @event, TPointDelta delta);

    /// <summary>
    /// Adds a new event to this queue that happens exactly once.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="dequeuePoint">Queue point at which this event should be dequeued.</param>
    /// <returns>New <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueAt(TEvent @event, TPoint dequeuePoint);

    /// <summary>
    /// Adds a new infinitely repeatable event to this queue with its <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.DequeuePoint"/>
    /// equal to <see cref="IReadOnlyEventQueue{TEvent,TPoint,TPointDelta}.CurrentPoint"/> moved by the specified <paramref name="delta"/>.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="delta">
    /// Point delta used for moving <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.DequeuePoint"/>
    /// forward on each repetition of this event.
    /// </param>
    /// <returns>New <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueInfinite(TEvent @event, TPointDelta delta);

    /// <summary>
    /// Adds a new infinitely repeatable event to this queue.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="dequeuePoint">Queue point at which this event should be dequeued for the first time.</param>
    /// <param name="delta">
    /// Point delta used for moving <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.DequeuePoint"/>
    /// forward on each repetition of this event.
    /// </param>
    /// <returns>New <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueInfiniteAt(TEvent @event, TPoint dequeuePoint, TPointDelta delta);
}
