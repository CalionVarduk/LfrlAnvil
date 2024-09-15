// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Subscribes to all emitted inner event streams, unless the number of maximum concurrently active inner event streams is reached,
/// in which case subsequent inner event streams are moved to a queue. If an inner event stream is disposed,
/// then an enqueued inner stream is dequeued and becomes active. The decorated event listener is notified with all events
/// emitted by all active inner streams.
/// </summary>
/// <typeparam name="TEvent">Inner event type.</typeparam>
public sealed class EventListenerMergeAllDecorator<TEvent> : IEventListenerDecorator<IEventStream<TEvent>, TEvent>
{
    private readonly int _maxConcurrency;

    /// <summary>
    /// Creates a new <see cref="EventListenerMergeAllDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="maxConcurrency">Maximum number of concurrently active inner event streams.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxConcurrency"/> is less than <b>1</b>.</exception>
    public EventListenerMergeAllDecorator(int maxConcurrency)
    {
        Ensure.IsGreaterThan( maxConcurrency, 0 );
        _maxConcurrency = maxConcurrency;
    }

    /// <inheritdoc />
    public IEventListener<IEventStream<TEvent>> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, subscriber, _maxConcurrency );
    }

    private sealed class EventListener : DecoratedEventListener<IEventStream<TEvent>, TEvent>
    {
        private readonly LinkedList<IEventSubscriber?> _innerSubscribers;
        private readonly IEventSubscriber _subscriber;
        private readonly int _maxConcurrency;
        private QueueSlim<IEventStream<TEvent>> _streamQueue;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, int maxConcurrency)
            : base( next )
        {
            _streamQueue = QueueSlim<IEventStream<TEvent>>.Create();
            _innerSubscribers = new LinkedList<IEventSubscriber?>();
            _subscriber = subscriber;
            _maxConcurrency = maxConcurrency;
        }

        public override void React(IEventStream<TEvent> @event)
        {
            if ( _innerSubscribers.Count == _maxConcurrency )
            {
                _streamQueue.Enqueue( @event );
                return;
            }

            StartListeningToNextInnerStream( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            foreach ( var subscriber in _innerSubscribers )
                subscriber?.Dispose();

            _innerSubscribers.Clear();
            _streamQueue.Clear();
            _streamQueue.ResetCapacity();

            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnInnerEvent(TEvent @event)
        {
            Next.React( @event );
        }

        internal void OnInnerDisposed(LinkedListNode<IEventSubscriber?> node)
        {
            if ( _subscriber.IsDisposed )
                return;

            _innerSubscribers.Remove( node );
            if ( _streamQueue.TryDequeue( out var stream ) )
                StartListeningToNextInnerStream( stream );
        }

        private void StartListeningToNextInnerStream(IEventStream<TEvent> stream)
        {
            var innerSubscriberNode = _innerSubscribers.AddLast( ( IEventSubscriber? )null );
            var innerListener = new InnerEventListener( this, innerSubscriberNode );

            var innerSubscriber = stream.Listen( innerListener );
            innerSubscriberNode.Value = innerSubscriber;
        }
    }

    private sealed class InnerEventListener : EventListener<TEvent>
    {
        private readonly LinkedListNode<IEventSubscriber?> _subscriberNode;
        private EventListener? _outerListener;

        internal InnerEventListener(EventListener outerListener, LinkedListNode<IEventSubscriber?> subscriberNode)
        {
            _subscriberNode = subscriberNode;
            _outerListener = outerListener;
        }

        public override void React(TEvent @event)
        {
            Assume.IsNotNull( _outerListener );
            _outerListener.OnInnerEvent( @event );
        }

        public override void OnDispose(DisposalSource _)
        {
            Assume.IsNotNull( _outerListener );
            _outerListener.OnInnerDisposed( _subscriberNode );
            _outerListener = null;
        }
    }
}
