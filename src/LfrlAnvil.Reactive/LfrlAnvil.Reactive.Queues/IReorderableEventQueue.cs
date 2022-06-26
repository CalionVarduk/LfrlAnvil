using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues;

public interface IReorderableEventQueue<TEvent, TPoint, TPointDelta>
    : IEventQueue<TEvent, TPoint, TPointDelta>, IReadOnlyReorderableEventQueue<TEvent, TPoint, TPointDelta>
{
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? SetDequeuePoint(TEvent @event, TPoint dequeuePoint);
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? DelayDequeuePoint(TEvent @event, TPointDelta delta);
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? AdvanceDequeuePoint(TEvent @event, TPointDelta delta);
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? SetRepetitions(TEvent @event, int repetitions);
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? IncreaseRepetitions(TEvent @event, int count);
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? DecreaseRepetitions(TEvent @event, int count);
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? MakeInfinite(TEvent @event);
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? SetDelta(TEvent @event, TPointDelta delta);
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? IncreaseDelta(TEvent @event, TPointDelta delta);
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? DecreaseDelta(TEvent @event, TPointDelta delta);
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? Remove(TEvent @event);
}
