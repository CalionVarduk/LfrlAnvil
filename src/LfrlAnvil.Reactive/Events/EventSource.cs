using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Events.Internal;
using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Events
{
    public class EventSource<TEvent> : IEventSource<TEvent>
    {
        private readonly List<EventSubscriber<TEvent>> _subscribers;

        public EventSource()
        {
            _subscribers = new List<EventSubscriber<TEvent>>();
        }

        public bool IsDisposed { get; private set; }
        public bool HasSubscribers => _subscribers.Count > 0;
        public IReadOnlyCollection<IEventSubscriber> Subscribers => _subscribers;

        public void Dispose()
        {
            if ( IsDisposed )
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

                    subscriber.Listener.OnDispose();
                    subscriber.MarkAsDisposed();
                }
            }
            finally
            {
                ArrayPool<EventSubscriber<TEvent>>.Shared.Return( subscribers, clearArray: true );
            }

            OnDispose();
            IsDisposed = true;
        }

        public IEventSubscriber Listen(IEventListener<TEvent> listener)
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

        [Pure]
        public IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator)
        {
            return new DecoratedEventSource<TEvent, TNextEvent>( this, decorator );
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

        internal void RemoveSubscriber(EventSubscriber<TEvent> subscriber)
        {
            _subscribers.Remove( subscriber );
        }

        [Pure]
        internal EventSubscriber<TEvent> CreateSubscriber()
        {
            return new EventSubscriber<TEvent>( this, EventListener<TEvent>.Empty );
        }

        internal IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TEvent> subscriber)
        {
            subscriber.Listener = listener;
            return subscriber.IsDisposed ? subscriber : ListenInternal( listener, subscriber );
        }

        private IEventSubscriber ListenInternal(IEventListener<TEvent> listener, EventSubscriber<TEvent> subscriber)
        {
            if ( IsDisposed )
            {
                subscriber.Listener.OnDispose();
                subscriber.MarkAsDisposed();
                return subscriber;
            }

            _subscribers.Add( subscriber );
            OnSubscriberAdded( subscriber, listener );
            return subscriber;
        }

        IEventSubscriber IEventStream.Listen(IEventListener listener)
        {
            return Listen( Argument.CastTo<IEventListener<TEvent>>( listener, nameof( listener ) ) );
        }

        void IEventSource.Publish(object? @event)
        {
            Publish( Argument.CastTo<TEvent>( @event, nameof( @event ) ) );
        }
    }
}
