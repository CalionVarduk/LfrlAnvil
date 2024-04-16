using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Queues;

namespace LfrlAnvil.Reactive.Chrono;

public class EventQueue<TEvent> : EventQueueBase<TEvent, Timestamp, Duration>
{
    public EventQueue(Timestamp startPoint)
        : base( startPoint ) { }

    [Pure]
    protected sealed override Timestamp AddDelta(Timestamp point, Duration delta)
    {
        return point.Add( delta );
    }
}
