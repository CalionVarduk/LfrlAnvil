namespace LfrlAnvil.Reactive.Queues;

/// <summary>
/// Represents a generic mutable event queue that allows to modify registered events.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TPoint">Queue point type.</typeparam>
/// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
public interface IMutableReorderableEventQueue<TEvent, TPoint, TPointDelta>
    : IMutableEventQueue<TEvent, TPoint, TPointDelta>, IReorderableEventQueue<TEvent, TPoint, TPointDelta> { }
