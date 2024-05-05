using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Queues;

/// <summary>
/// Creates instances of <see cref="QueueEventSource{TEvent,TPoint,TPointDelta}"/> type.
/// </summary>
public static class QueueEventSource
{
    /// <summary>
    /// Creates a new <see cref="QueueEventSource{TEvent,TPoint,TPointDelta}"/> instance from the provided <paramref name="queue"/>.
    /// </summary>
    /// <param name="queue">Underlying queue.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TPoint">Queue point type.</typeparam>
    /// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
    /// <returns>New <see cref="QueueEventSource{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static QueueEventSource<TEvent, TPoint, TPointDelta> Create<TEvent, TPoint, TPointDelta>(
        IMutableEventQueue<TEvent, TPoint, TPointDelta> queue)
    {
        return new QueueEventSource<TEvent, TPoint, TPointDelta>( queue );
    }

    /// <summary>
    /// Creates a new <see cref="ReorderableQueueEventSource{TEvent,TPoint,TPointDelta}"/> instance
    /// from the provided <paramref name="queue"/>.
    /// </summary>
    /// <param name="queue">Underlying queue.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TPoint">Queue point type.</typeparam>
    /// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
    /// <returns>New <see cref="ReorderableQueueEventSource{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReorderableQueueEventSource<TEvent, TPoint, TPointDelta> Create<TEvent, TPoint, TPointDelta>(
        IMutableReorderableEventQueue<TEvent, TPoint, TPointDelta> queue)
    {
        return new ReorderableQueueEventSource<TEvent, TPoint, TPointDelta>( queue );
    }
}
