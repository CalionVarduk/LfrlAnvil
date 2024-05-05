using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono.Composites;

/// <summary>
/// Represents an event with <see cref="Timestamp"/> and <see cref="Interval"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public readonly struct WithInterval<TEvent>
{
    /// <summary>
    /// Creates a new <see cref="WithInterval{TEvent}"/> instance.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="timestamp"><see cref="LfrlAnvil.Chrono.Timestamp"/> associated with this event.</param>
    /// <param name="interval">Time elapsed since the last event.</param>
    public WithInterval(TEvent @event, Timestamp timestamp, Duration interval)
    {
        Event = @event;
        Timestamp = timestamp;
        Interval = interval;
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
    /// Time elapsed since the last event.
    /// </summary>
    public Duration Interval { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="WithInterval{TEvent}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Timestamp} ({Interval} dt)] {Event}";
    }
}
