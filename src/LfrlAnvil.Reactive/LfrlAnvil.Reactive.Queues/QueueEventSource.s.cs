using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Queues;

public static class QueueEventSource
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static QueueEventSource<TEvent, TPoint, TPointDelta> Create<TEvent, TPoint, TPointDelta>(
        IMutableEventQueue<TEvent, TPoint, TPointDelta> queue)
    {
        return new QueueEventSource<TEvent, TPoint, TPointDelta>( queue );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReorderableQueueEventSource<TEvent, TPoint, TPointDelta> Create<TEvent, TPoint, TPointDelta>(
        IMutableReorderableEventQueue<TEvent, TPoint, TPointDelta> queue)
    {
        return new ReorderableQueueEventSource<TEvent, TPoint, TPointDelta>( queue );
    }
}