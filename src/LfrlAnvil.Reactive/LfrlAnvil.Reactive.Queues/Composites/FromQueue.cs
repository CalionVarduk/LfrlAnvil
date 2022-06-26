using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Queues.Composites;

public readonly struct FromQueue<TEvent, TPoint, TPointDelta>
{
    public FromQueue(EnqueuedEvent<TEvent, TPoint, TPointDelta> enqueued, TPoint currentQueuePoint, TPointDelta delta)
    {
        Enqueued = enqueued;
        CurrentQueuePoint = currentQueuePoint;
        Delta = delta;
    }

    public EnqueuedEvent<TEvent, TPoint, TPointDelta> Enqueued { get; }
    public TPoint CurrentQueuePoint { get; }
    public TPointDelta Delta { get; }

    [Pure]
    public override string ToString()
    {
        return $"{Enqueued} [queue: {CurrentQueuePoint} ({Delta} dt)]";
    }
}
