using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono.Composites;

/// <summary>
/// Represents an event with <see cref="Timestamp"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public readonly struct WithTimestamp<TEvent>
{
    /// <summary>
    /// Creates a new <see cref="WithTimestamp{TEvent}"/> instance.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="timestamp"><see cref="LfrlAnvil.Chrono.Timestamp"/> associated with this event.</param>
    public WithTimestamp(TEvent @event, Timestamp timestamp)
    {
        Event = @event;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Underlying event.
    /// </summary>
    public TEvent Event { get; }

    /// <summary>
    /// <see cref="LfrlAnvil.Chrono.Timestamp"/> associated with this event.
    /// </summary>
    public Timestamp Timestamp { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="WithTimestamp{TEvent}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Timestamp}] {Event}";
    }
}
