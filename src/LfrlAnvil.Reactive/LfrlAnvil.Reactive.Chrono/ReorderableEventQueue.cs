using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Queues;

namespace LfrlAnvil.Reactive.Chrono;

public class ReorderableEventQueue<TEvent> : ReorderableEventQueueBase<TEvent, Timestamp, Duration>
    where TEvent : notnull
{
    public ReorderableEventQueue(Timestamp startPoint)
        : base( startPoint ) { }

    public ReorderableEventQueue(Timestamp startPoint, IEqualityComparer<TEvent> eventComparer)
        : base( startPoint, eventComparer, Comparer<Timestamp>.Default ) { }

    [Pure]
    protected sealed override Timestamp AddDelta(Timestamp point, Duration delta)
    {
        return point.Add( delta );
    }

    [Pure]
    protected sealed override Timestamp SubtractDelta(Timestamp point, Duration delta)
    {
        return point.Subtract( delta );
    }

    [Pure]
    protected sealed override Duration Add(Duration a, Duration b)
    {
        return a.Add( b );
    }

    [Pure]
    protected sealed override Duration Subtract(Duration a, Duration b)
    {
        return a.Subtract( b );
    }
}
