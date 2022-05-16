using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Events.Internal;
using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Events
{
    public class EventPublisher<TEvent> : EventSource<TEvent>, IEventPublisher<TEvent>
    {
        private bool _isDisposed;
        private readonly List<EventSubscriber<TEvent>> _subscribers;

        public EventPublisher()
        {
            _isDisposed = false;
            _subscribers = new List<EventSubscriber<TEvent>>( capacity: 1 );
        }

        public sealed override bool IsDisposed => _isDisposed;
        public sealed override IReadOnlyCollection<IEventSubscriber> Subscribers => _subscribers;

        public sealed override void Dispose()
        {
            if ( _isDisposed )
                return;

            _isDisposed = true;
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

        public sealed override IEventSubscriber Listen(IEventListener<TEvent> listener)
        {
            var subscriber = new EventSubscriber<TEvent>( this, listener );
            return ListenInternal( listener, subscriber );
        }

        public void Publish(TEvent @event)
        {
            if ( IsDisposed )
                throw new ObjectDisposedException( Resources.DisposedEventSource );

            OnPublish( @event );
        }

        protected virtual void OnSubscriberAdded(IEventSubscriber subscriber, IEventListener<TEvent> listener) { }

        protected virtual void OnPublish(TEvent @event)
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

        internal sealed override void RemoveSubscriber(IEventSubscriber subscriber)
        {
            var index = _subscribers.FindIndex( s => ReferenceEquals( s, subscriber ) );
            if ( index < 0 )
                return;

            _subscribers.RemoveAt( index );
        }

        [Pure]
        internal sealed override EventSubscriber<TEvent> CreateSubscriber()
        {
            return new EventSubscriber<TEvent>( this, EventListener<TEvent>.Empty );
        }

        internal sealed override IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TEvent> subscriber)
        {
            subscriber.Listener = listener;

            if ( ! subscriber.IsDisposed )
                return ListenInternal( listener, subscriber );

            subscriber.Listener.OnDispose( DisposalSource.Subscriber );
            return subscriber;
        }

        private IEventSubscriber ListenInternal(IEventListener<TEvent> listener, EventSubscriber<TEvent> subscriber)
        {
            if ( IsDisposed )
            {
                subscriber.MarkAsDisposed();
                subscriber.Listener.OnDispose( DisposalSource.EventSource );
                return subscriber;
            }

            _subscribers.Add( subscriber );
            OnSubscriberAdded( subscriber, listener );
            return subscriber;
        }

        void IEventPublisher.Publish(object? @event)
        {
            Publish( Argument.CastTo<TEvent>( @event, nameof( @event ) ) );
        }
    }
}
