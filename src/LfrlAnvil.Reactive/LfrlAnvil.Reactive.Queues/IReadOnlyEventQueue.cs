using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Queues
{
    public interface IReadOnlyEventQueue<TEvent, TPoint, TPointDelta> : IReadOnlyCollection<EnqueuedEvent<TEvent, TPoint, TPointDelta>>
    {
        TPoint StartPoint { get; }
        TPoint CurrentPoint { get; }
        IComparer<TPoint> Comparer { get; }

        [Pure]
        EnqueuedEvent<TEvent, TPoint, TPointDelta>? GetNext();

        [Pure]
        IEnumerable<EnqueuedEvent<TEvent, TPoint, TPointDelta>> GetEvents(TPoint endPoint);
    }
}
