using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues;

public interface IReadOnlyReorderableEventQueue<TEvent, TPoint, TPointDelta> : IReadOnlyEventQueue<TEvent, TPoint, TPointDelta>
{
    IEqualityComparer<TEvent> EventComparer { get; }

    [Pure]
    bool Contains(TEvent @event);

    [Pure]
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? GetEvent(TEvent @event);
}
