using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono.Composites;

/// <summary>
/// Represents an event with <see cref="DateTime"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public readonly struct WithZonedDateTime<TEvent>
{
    /// <summary>
    /// Creates a new <see cref="WithZonedDateTime{TEvent}"/> instance.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="dateTime"><see cref="ZonedDateTime"/> associated with this event.</param>
    public WithZonedDateTime(TEvent @event, ZonedDateTime dateTime)
    {
        Event = @event;
        DateTime = dateTime;
    }

    /// <summary>
    /// Underlying event.
    /// </summary>
    public TEvent Event { get; }

    /// <summary>
    /// <see cref="ZonedDateTime"/> associated with this event.
    /// </summary>
    public ZonedDateTime DateTime { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="WithZonedDateTime{TEvent}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{DateTime}] {Event}";
    }
}
