using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues;

/// <summary>
/// Represents a generic disposable event source that can be listened to
/// based on an underlying <see cref="IMutableEventQueue{TEvent,TPoint,TPointDelta}"/> instance.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TPoint">Queue point type.</typeparam>
/// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
public class QueueEventSource<TEvent, TPoint, TPointDelta> : EventSource<FromQueue<TEvent, TPoint, TPointDelta>>
{
    private readonly IMutableEventQueue<TEvent, TPoint, TPointDelta> _queue;

    /// <summary>
    /// Creates a new <see cref="QueueEventSource{TEvent,TPoint,TPointDelta}"/> instance.
    /// </summary>
    /// <param name="queue">Underlying queue.</param>
    public QueueEventSource(IMutableEventQueue<TEvent, TPoint, TPointDelta> queue)
    {
        _queue = queue;
    }

    /// <summary>
    /// Underlying queue.
    /// </summary>
    public IEventQueue<TEvent, TPoint, TPointDelta> Queue => _queue;

    /// <summary>
    /// Moves the <see cref="IReadOnlyEventQueue{TEvent,TPoint,TPointDelta}.CurrentPoint"/> of the <see cref="Queue"/> forward
    /// and emits events for all dequeued events.
    /// </summary>
    /// <param name="delta">
    /// Point delta to move the <see cref="IReadOnlyEventQueue{TEvent,TPoint,TPointDelta}.CurrentPoint"/> forward by.
    /// </param>
    public void Move(TPointDelta delta)
    {
        EnsureNotDisposed();
        _queue.Move( delta );

        var @event = _queue.Dequeue();
        while ( @event is not null )
        {
            var nextEvent = new FromQueue<TEvent, TPoint, TPointDelta>( @event.Value, _queue.CurrentPoint, delta );
            NotifyListeners( nextEvent );
            @event = _queue.Dequeue();
        }
    }

    /// <inheritdoc />
    protected override void OnDispose()
    {
        _queue.Clear();
        base.OnDispose();
    }
}
