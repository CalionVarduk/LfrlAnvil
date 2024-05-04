using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Composites;

/// <summary>
/// Represents an event with a sender.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public readonly struct WithSender<TEvent>
{
    /// <summary>
    /// Creates a new <see cref="WithSender{TEvent}"/> instance.
    /// </summary>
    /// <param name="sender">Event's sender.</param>
    /// <param name="event">Underlying event.</param>
    public WithSender(object? sender, TEvent @event)
    {
        Sender = sender;
        Event = @event;
    }

    /// <summary>
    /// Event's sender.
    /// </summary>
    public object? Sender { get; }

    /// <summary>
    /// Underlying event.
    /// </summary>
    public TEvent Event { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="WithSender{TEvent}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{Sender} => {Event}";
    }
}
