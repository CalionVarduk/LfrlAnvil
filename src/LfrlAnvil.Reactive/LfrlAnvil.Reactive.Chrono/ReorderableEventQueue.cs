using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Queues;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents a generic event queue that allows to modify registered events
/// with <see cref="Timestamp"/> point and <see cref="Duration"/> delta.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class ReorderableEventQueue<TEvent> : ReorderableEventQueueBase<TEvent, Timestamp, Duration>
    where TEvent : notnull
{
    /// <summary>
    /// Creates a new <see cref="ReorderableEventQueue{TEvent}"/> instance with <see cref="EqualityComparer{T}.Default"/> event comparer.
    /// </summary>
    /// <param name="startPoint">Specifies the starting <see cref="Timestamp"/> of this queue.</param>
    public ReorderableEventQueue(Timestamp startPoint)
        : base( startPoint ) { }

    /// <summary>
    /// Creates a new <see cref="ReorderableEventQueue{TEvent}"/> instance.
    /// </summary>
    /// <param name="startPoint">Specifies the starting <see cref="Timestamp"/> of this queue.</param>
    /// <param name="eventComparer">Event equality comparer.</param>
    public ReorderableEventQueue(Timestamp startPoint, IEqualityComparer<TEvent> eventComparer)
        : base( startPoint, eventComparer, Comparer<Timestamp>.Default ) { }

    /// <inheritdoc />
    [Pure]
    protected sealed override Timestamp AddDelta(Timestamp point, Duration delta)
    {
        return point.Add( delta );
    }

    /// <inheritdoc />
    [Pure]
    protected sealed override Timestamp SubtractDelta(Timestamp point, Duration delta)
    {
        return point.Subtract( delta );
    }

    /// <inheritdoc />
    [Pure]
    protected sealed override Duration Add(Duration a, Duration b)
    {
        return a.Add( b );
    }

    /// <inheritdoc />
    [Pure]
    protected sealed override Duration Subtract(Duration a, Duration b)
    {
        return a.Subtract( b );
    }
}
