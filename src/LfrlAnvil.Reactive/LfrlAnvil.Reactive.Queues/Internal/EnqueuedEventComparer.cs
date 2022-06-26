using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues.Internal;

internal sealed class EnqueuedEventComparer<TEvent, TPoint, TPointDelta> : IComparer<EnqueuedEvent<TEvent, TPoint, TPointDelta>>
{
    private readonly IComparer<TPoint> _pointComparer;

    internal EnqueuedEventComparer(IComparer<TPoint> pointComparer)
    {
        _pointComparer = pointComparer;
    }

    [Pure]
    public int Compare(EnqueuedEvent<TEvent, TPoint, TPointDelta> a, EnqueuedEvent<TEvent, TPoint, TPointDelta> b)
    {
        return _pointComparer.Compare( a.DequeuePoint, b.DequeuePoint );
    }
}