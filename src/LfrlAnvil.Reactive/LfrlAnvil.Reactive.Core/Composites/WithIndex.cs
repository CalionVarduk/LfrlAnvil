using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Composites;

/// <summary>
/// Represents an event with an index.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public readonly struct WithIndex<TEvent>
{
    /// <summary>
    /// Creates a new <see cref="WithIndex{TEvent}"/> instance.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="index">Attached index.</param>
    public WithIndex(TEvent @event, int index)
    {
        Event = @event;
        Index = index;
    }

    /// <summary>
    /// Underlying event.
    /// </summary>
    public TEvent Event { get; }

    /// <summary>
    /// Attached index.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="WithIndex{TEvent}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Index}]: {Event}";
    }
}
