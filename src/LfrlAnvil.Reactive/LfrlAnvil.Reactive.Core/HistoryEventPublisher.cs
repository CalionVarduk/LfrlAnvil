using System;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive;

/// <inheritdoc cref="IHistoryEventPublisher{TEvent}" />
public class HistoryEventPublisher<TEvent> : EventPublisher<TEvent>, IHistoryEventPublisher<TEvent>
{
    private readonly Queue<TEvent> _history;

    /// <summary>
    /// Creates a new <see cref="HistoryEventPublisher{TEvent}"/> instance.
    /// </summary>
    /// <param name="capacity">Specifies the maximum number of events this event publisher can record.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="capacity"/> is less than <b>1</b>.</exception>
    public HistoryEventPublisher(int capacity)
    {
        Ensure.IsGreaterThan( capacity, 0 );
        Capacity = capacity;
        _history = new Queue<TEvent>();
    }

    /// <inheritdoc />
    public int Capacity { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<TEvent> History => _history;

    /// <inheritdoc />
    public void ClearHistory()
    {
        _history.Clear();
    }

    /// <inheritdoc />
    protected override void OnDispose()
    {
        base.OnDispose();
        ClearHistory();
        _history.TrimExcess();
    }

    /// <inheritdoc />
    protected override void OnPublish(TEvent @event)
    {
        if ( _history.Count == Capacity )
            _history.Dequeue();

        _history.Enqueue( @event );
        base.OnPublish( @event );
    }

    /// <inheritdoc />
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
