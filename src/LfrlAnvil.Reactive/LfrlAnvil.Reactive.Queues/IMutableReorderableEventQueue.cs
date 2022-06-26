namespace LfrlAnvil.Reactive.Queues;

public interface IMutableReorderableEventQueue<TEvent, TPoint, TPointDelta>
    : IMutableEventQueue<TEvent, TPoint, TPointDelta>, IReorderableEventQueue<TEvent, TPoint, TPointDelta> { }