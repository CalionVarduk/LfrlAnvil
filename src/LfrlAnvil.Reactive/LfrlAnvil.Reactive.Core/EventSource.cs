using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive
{
    public abstract class EventSource<TEvent> : IEventSource<TEvent>
    {
        private static IEventSource<TEvent>? _disposed;
        public static IEventSource<TEvent> Disposed => _disposed ??= CreateDisposed();

        private readonly List<EventSubscriber<TEvent>> _subscribers;
        private int _state;

        protected EventSource()
        {
            _state = 0;
            _subscribers = new List<EventSubscriber<TEvent>>( capacity: 1 );
        }

        public bool IsDisposed => _state == 1;
        public IReadOnlyCollection<IEventSubscriber> Subscribers => _subscribers;
        public bool HasSubscribers => _subscribers.Count > 0;

        public void Dispose()
        {
            if ( Interlocked.Exchange( ref _state, 1 ) == 1 )
                return;

            var subscriberCount = _subscribers.Count;
            var subscribers = ArrayPool<EventSubscriber<TEvent>>.Shared.Rent( subscriberCount );
            _subscribers.CopyTo( subscribers );
            _subscribers.Clear();

            try
            {
                for ( var i = 0; i < subscriberCount; ++i )
                {
                    var subscriber = subscribers[i];
                    if ( subscriber.IsDisposed )
                        continue;

                    subscriber.MarkAsDisposed();
                    subscriber.Listener.OnDispose( DisposalSource.EventSource );
                }
            }
            finally
            {
                ArrayPool<EventSubscriber<TEvent>>.Shared.Return( subscribers, clearArray: true );
            }

            OnDispose();
        }

        public IEventSubscriber Listen(IEventListener<TEvent> listener)
        {
            var subscriber = new EventSubscriber<TEvent>( RemoveSubscriber, listener );
            return ListenInternal( subscriber );
        }

        [Pure]
        public IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator)
        {
            return new DecoratedEventSource<TEvent, TNextEvent>( this, decorator );
        }

        protected virtual void OnSubscriberAdded(IEventSubscriber subscriber, IEventListener<TEvent> listener) { }

        protected virtual IEventListener<TEvent> OverrideListener(IEventSubscriber subscriber, IEventListener<TEvent> listener)
        {
            return listener;
        }

        protected void NotifyListeners(TEvent @event)
        {
            var subscriberCount = _subscribers.Count;
            var subscribers = ArrayPool<EventSubscriber<TEvent>>.Shared.Rent( subscriberCount );
            _subscribers.CopyTo( subscribers );

            try
            {
                for ( var i = 0; i < subscriberCount; ++i )
                {
                    var subscriber = subscribers[i];
                    if ( subscriber.IsDisposed )
                        continue;

                    subscriber.Listener.React( @event );
                }
            }
            finally
            {
                ArrayPool<EventSubscriber<TEvent>>.Shared.Return( subscribers, clearArray: true );
            }
        }

        protected virtual void OnDispose() { }

        internal void RemoveSubscriber(EventSubscriber<TEvent> subscriber)
        {
            _subscribers.Remove( subscriber );
        }

        [Pure]
        internal EventSubscriber<TEvent> CreateSubscriber()
        {
            return new EventSubscriber<TEvent>( RemoveSubscriber, EventListener<TEvent>.Empty );
        }

        internal IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TEvent> subscriber)
        {
            subscriber.Listener = listener;
            return ListenInternal( subscriber );
        }

        internal IEventSubscriber ListenInternal(EventSubscriber<TEvent> subscriber)
        {
            subscriber.Listener = OverrideListener( subscriber, subscriber.Listener );

            if ( subscriber.IsDisposed )
            {
                subscriber.Listener.OnDispose( DisposalSource.Subscriber );
                return subscriber;
            }

            if ( IsDisposed )
            {
                subscriber.MarkAsDisposed();
                subscriber.Listener.OnDispose( DisposalSource.EventSource );
                return subscriber;
            }

            _subscribers.Add( subscriber );
            OnSubscriberAdded( subscriber, subscriber.Listener );
            return subscriber;
        }

        private static IEventSource<TEvent> CreateDisposed()
        {
            var result = new EventPublisher<TEvent>();
            result.Dispose();
            return result;
        }

        IEventSubscriber IEventStream.Listen(IEventListener listener)
        {
            return Listen( Argument.CastTo<IEventListener<TEvent>>( listener, nameof( listener ) ) );
        }
    }
}
