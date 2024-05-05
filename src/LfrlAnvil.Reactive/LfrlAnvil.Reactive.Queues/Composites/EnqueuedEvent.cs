using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Queues.Composites;

/// <summary>
/// Represents an event enqueued in the <see cref="IEventQueue{TEvent,TPoint,TPointDelta}"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TPoint">Queue point type.</typeparam>
/// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
public readonly struct EnqueuedEvent<TEvent, TPoint, TPointDelta>
{
    private EnqueuedEvent(TEvent @event, TPoint dequeuePoint, TPointDelta? delta, int repetitions)
    {
        Event = @event;
        DequeuePoint = dequeuePoint;
        Delta = delta;
        Repetitions = repetitions;
    }

    /// <summary>
    /// Underlying event.
    /// </summary>
    public TEvent Event { get; }

    /// <summary>
    /// Queue point at which this event should be dequeued.
    /// </summary>
    public TPoint DequeuePoint { get; }

    /// <summary>
    /// Point delta used for moving <see cref="DequeuePoint"/> forward on each repetition of this event.
    /// </summary>
    public TPointDelta? Delta { get; }

    /// <summary>
    /// Number of repetitions of this event.
    /// </summary>
    public int Repetitions { get; }

    /// <summary>
    /// Specifies whether or not this event repeats infinitely.
    /// </summary>
    public bool IsInfinite => Repetitions == 0;

    /// <summary>
    /// Returns a string representation of this <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var repeatText = IsInfinite ? "inf" : $"x{Repetitions}";
        var deltaText = Repetitions != 1 ? $"({Delta} dt) " : string.Empty;
        return $"{Event} at {DequeuePoint} {deltaText}[{repeatText}]";
    }

    /// <summary>
    /// Creates a new <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance that happens exactly once.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="dequeuePoint">Queue point at which this event should be dequeued.</param>
    /// <returns>New <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static EnqueuedEvent<TEvent, TPoint, TPointDelta> CreateSingle(TEvent @event, TPoint dequeuePoint)
    {
        return new EnqueuedEvent<TEvent, TPoint, TPointDelta>( @event, dequeuePoint, default, 1 );
    }

    /// <summary>
    /// Creates a new <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance that repeats specified number of times.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="dequeuePoint">Queue point at which this event should be dequeued for the first time.</param>
    /// <param name="delta">Point delta used for moving <see cref="DequeuePoint"/> forward on each repetition of this event.</param>
    /// <param name="repetitions">Number of repetitions.</param>
    /// <returns>New <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="repetitions"/> is less than <b>1</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static EnqueuedEvent<TEvent, TPoint, TPointDelta> Create(
        TEvent @event,
        TPoint dequeuePoint,
        TPointDelta delta,
        int repetitions)
    {
        Ensure.IsGreaterThan( repetitions, 0 );
        return new EnqueuedEvent<TEvent, TPoint, TPointDelta>( @event, dequeuePoint, delta, repetitions );
    }

    /// <summary>
    /// Creates a new <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance that repeats infinitely.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="dequeuePoint">Queue point at which this event should be dequeued for the first time.</param>
    /// <param name="delta">Point delta used for moving <see cref="DequeuePoint"/> forward on each repetition of this event.</param>
    /// <returns>New <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static EnqueuedEvent<TEvent, TPoint, TPointDelta> CreateInfinite(TEvent @event, TPoint dequeuePoint, TPointDelta delta)
    {
        return new EnqueuedEvent<TEvent, TPoint, TPointDelta>( @event, dequeuePoint, delta, 0 );
    }

    /// <summary>
    /// Creates a new <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance with changed <see cref="DequeuePoint"/>.
    /// </summary>
    /// <param name="dequeuePoint">Next queue point at which this event should be dequeued.</param>
    /// <returns>New <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> WithDequeuePoint(TPoint dequeuePoint)
    {
        return new EnqueuedEvent<TEvent, TPoint, TPointDelta>( Event, dequeuePoint, Delta, Repetitions );
    }

    /// <summary>
    /// Creates a new <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance with changed <see cref="Delta"/>.
    /// </summary>
    /// <param name="delta">Point delta used for moving <see cref="DequeuePoint"/> forward on each repetition of this event.</param>
    /// <returns>New <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> WithDelta(TPointDelta delta)
    {
        return new EnqueuedEvent<TEvent, TPoint, TPointDelta>( Event, DequeuePoint, delta, Repetitions );
    }

    /// <summary>
    /// Creates a new <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance with changed <see cref="Repetitions"/>.
    /// </summary>
    /// <param name="repetitions">Number of repetitions.</param>
    /// <returns>New <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="repetitions"/> is less than <b>1</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> WithRepetitions(int repetitions)
    {
        Ensure.IsGreaterThan( repetitions, 0 );
        return new EnqueuedEvent<TEvent, TPoint, TPointDelta>( Event, DequeuePoint, Delta, repetitions );
    }

    /// <summary>
    /// Creates a new <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance that repeats infinitely.
    /// </summary>
    /// <returns>New <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> AsInfinite()
    {
        return new EnqueuedEvent<TEvent, TPoint, TPointDelta>( Event, DequeuePoint, Delta, 0 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal EnqueuedEvent<TEvent, TPoint, TPointDelta> Repeat(TPoint dequeuePoint)
    {
        Assume.IsNotNull( Delta );

        return IsInfinite
            ? CreateInfinite( Event, dequeuePoint, Delta )
            : Create( Event, dequeuePoint, Delta, Repetitions - 1 );
    }
}
