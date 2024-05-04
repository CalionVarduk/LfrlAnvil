using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Composites;

/// <summary>
/// Represents a group of events associated with the same key.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TEvent">Event type.</typeparam>
public readonly struct EventGrouping<TKey, TEvent>
{
    /// <summary>
    /// Creates a new <see cref="EventGrouping{TKey,TEvent}"/> instance.
    /// </summary>
    /// <param name="key">Group's key.</param>
    /// <param name="event">Underlying event that was added last to the group.</param>
    /// <param name="allEvents">All underlying events associated with the <paramref name="key"/>.</param>
    public EventGrouping(TKey key, TEvent @event, ReadOnlyMemory<TEvent> allEvents)
    {
        Key = key;
        Event = @event;
        AllEvents = allEvents;
    }

    /// <summary>
    /// Group's key.
    /// </summary>
    public TKey Key { get; }

    /// <summary>
    /// Underlying event that was added last to the group.
    /// </summary>
    public TEvent Event { get; }

    /// <summary>
    /// All underlying events associated with the <see cref="Key"/>.
    /// </summary>
    public ReadOnlyMemory<TEvent> AllEvents { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="EventGrouping{TKey,TEvent}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Key}]: {Event} (Count = {AllEvents.Length})";
    }
}
