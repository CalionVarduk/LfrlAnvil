using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues;

public class ReorderableQueueEventSource<TEvent, TPoint, TPointDelta> : EventSource<FromQueue<TEvent, TPoint, TPointDelta>>
{
    private readonly IMutableReorderableEventQueue<TEvent, TPoint, TPointDelta> _queue;

    public ReorderableQueueEventSource(IMutableReorderableEventQueue<TEvent, TPoint, TPointDelta> queue)
    {
        _queue = queue;
    }

    public IReorderableEventQueue<TEvent, TPoint, TPointDelta> Queue => _queue;

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

    protected override void OnDispose()
    {
        _queue.Clear();
        base.OnDispose();
    }
}