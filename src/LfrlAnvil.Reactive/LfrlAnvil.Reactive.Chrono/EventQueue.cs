using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Queues;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents a generic event queue with <see cref="Timestamp"/> point and <see cref="Duration"/> delta.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventQueue<TEvent> : EventQueueBase<TEvent, Timestamp, Duration>
{
    /// <summary>
    /// Creates a new <see cref="EventQueue{TEvent}"/> instance.
    /// </summary>
    /// <param name="startPoint">Specifies the starting <see cref="Timestamp"/> of this queue.</param>
    public EventQueue(Timestamp startPoint)
        : base( startPoint ) { }

    /// <inheritdoc />
    [Pure]
    protected sealed override Timestamp AddDelta(Timestamp point, Duration delta)
    {
        return point.Add( delta );
    }
}
