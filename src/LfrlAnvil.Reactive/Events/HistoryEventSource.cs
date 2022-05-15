using System;
using System.Collections.Generic;
using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Events
{
    public class HistoryEventSource<TEvent> : EventSource<TEvent>
    {
        private readonly LinkedList<TEvent> _history;

        public HistoryEventSource(int capacity)
        {
            if ( capacity <= 0 )
                throw new ArgumentOutOfRangeException( nameof( capacity ), Resources.InvalidCapacity( capacity ) );

            Capacity = capacity;
            _history = new LinkedList<TEvent>();
        }

        public int Capacity { get; }
        public IReadOnlyCollection<TEvent> History => _history;

        public void ClearHistory()
        {
            _history.Clear();
        }

        protected override void OnDispose()
        {
            ClearHistory();
        }

        protected override void OnPublish(TEvent @event)
        {
            if ( _history.Count == Capacity )
                _history.RemoveFirst();

            _history.AddLast( @event );
            base.OnPublish( @event );
        }

        protected override void OnSubscriberAdded(IEventSubscriber subscriber, IEventListener<TEvent> listener)
        {
            foreach ( var @event in _history )
            {
                if ( subscriber.IsDisposed )
                    break;

                listener.React( @event );
            }
        }
    }
}
