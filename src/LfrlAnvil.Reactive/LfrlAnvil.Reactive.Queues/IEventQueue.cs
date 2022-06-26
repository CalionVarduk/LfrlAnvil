using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues;

public interface IEventQueue<TEvent, TPoint, TPointDelta> : IReadOnlyEventQueue<TEvent, TPoint, TPointDelta>
{
    EnqueuedEvent<TEvent, TPoint, TPointDelta> Enqueue(TEvent @event, TPointDelta delta, int repetitions);
    EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueAt(TEvent @event, TPoint dequeuePoint, TPointDelta delta, int repetitions);
    EnqueuedEvent<TEvent, TPoint, TPointDelta> Enqueue(TEvent @event, TPointDelta delta);
    EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueAt(TEvent @event, TPoint dequeuePoint);
    EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueInfinite(TEvent @event, TPointDelta delta);
    EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueInfiniteAt(TEvent @event, TPoint dequeuePoint, TPointDelta delta);
}
