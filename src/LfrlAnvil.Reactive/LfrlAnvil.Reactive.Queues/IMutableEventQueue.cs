using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues;

public interface IMutableEventQueue<TEvent, TPoint, TPointDelta> : IEventQueue<TEvent, TPoint, TPointDelta>
{
    void Move(TPointDelta delta);
    void Clear();
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? Dequeue();
}