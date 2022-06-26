using System.Collections.Generic;

namespace LfrlAnvil.Reactive;

public class HistoryEventPublisher<TEvent> : EventPublisher<TEvent>, IHistoryEventPublisher<TEvent>
{
    private readonly Queue<TEvent> _history;

    public HistoryEventPublisher(int capacity)
    {
        Ensure.IsGreaterThan( capacity, 0, nameof( capacity ) );
        Capacity = capacity;
        _history = new Queue<TEvent>();
    }

    public int Capacity { get; }
    public IReadOnlyCollection<TEvent> History => _history;

    public void ClearHistory()
    {
        _history.Clear();
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        ClearHistory();
        _history.TrimExcess();
    }

    protected override void OnPublish(TEvent @event)
    {
        if ( _history.Count == Capacity )
            _history.Dequeue();

        _history.Enqueue( @event );
        base.OnPublish( @event );
    }

    protected override void OnSubscriberAdded(IEventSubscriber subscriber, IEventListener<TEvent> listener)
    {
        base.OnSubscriberAdded( subscriber, listener );

        foreach ( var @event in _history )
        {
            if ( subscriber.IsDisposed )
                return;

            listener.React( @event );
        }
    }
}