// Copyright 2024-2026 Łukasz Furlepa
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
using System.Runtime.CompilerServices;
using LfrlAnvil.Async;
using LfrlAnvil.Memory;
using LfrlAnvil.Reactive.Internal;

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
    public IEventListener<IEventStream<TEvent>> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _maxConcurrency );
    }

    private sealed class EventListener : DecoratedEventListener<IEventStream<TEvent>, TEvent>
    {
        private readonly object _sync = new object();
        private readonly int _maxConcurrency;
        private InnerSubscribersCollection _innerSubscribers;
        private QueueSlim<IEventStream<TEvent>> _pendingStreams;
        private bool _isDisposed;

        internal EventListener(IEventListener<TEvent> next, int maxConcurrency)
            : base( next )
        {
            _pendingStreams = QueueSlim<IEventStream<TEvent>>.Create();
            _innerSubscribers = new InnerSubscribersCollection( 0 );
            _maxConcurrency = maxConcurrency;
        }

        public override void React(IEventStream<TEvent> @event)
        {
            int nodeId;
            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return;

                if ( _innerSubscribers.Count == _maxConcurrency )
                {
                    _pendingStreams.Enqueue( @event );
                    return;
                }

                nodeId = _innerSubscribers.Reserve();
            }

            StartListeningToNextInnerStream( nodeId, @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            ArrayPoolToken<IEventSubscriber?> subscribersToken = default;
            try
            {
                ReadOnlySpan<IEventSubscriber?> subscribers;
                using ( AcquireLock() )
                {
                    if ( _isDisposed )
                        return;

                    _isDisposed = true;
                    _pendingStreams = QueueSlim<IEventStream<TEvent>>.Create();
                    subscribers = _innerSubscribers.Clear( out subscribersToken );
                }

                foreach ( var s in subscribers )
                    s?.Dispose();
            }
            finally
            {
                subscribersToken.Dispose();
            }

            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnInnerEvent(TEvent @event)
        {
            Next.React( @event );
        }

        internal void OnInnerDisposed(int nodeId)
        {
            int nextNodeId;
            IEventStream<TEvent>? nextStream;
            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return;

                _innerSubscribers.Remove( nodeId );
                if ( ! _pendingStreams.TryDequeue( out nextStream ) )
                    return;

                nextNodeId = _innerSubscribers.Reserve();
            }

            StartListeningToNextInnerStream( nextNodeId, nextStream );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void StartListeningToNextInnerStream(int nodeId, IEventStream<TEvent> stream)
        {
            var innerListener = new InnerEventListener( this, nodeId );
            var innerSubscriber = stream.Listen( innerListener );

            bool dispose;
            using ( AcquireLock() )
            {
                dispose = _isDisposed;
                if ( ! dispose )
                    _innerSubscribers.Set( nodeId, innerSubscriber );
            }

            if ( dispose )
                innerSubscriber.Dispose();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ExclusiveLock AcquireLock()
        {
            return ExclusiveLock.Enter( _sync );
        }
    }

    private sealed class InnerEventListener : EventListener<TEvent>
    {
        private readonly EventListener _outerListener;
        private readonly int _nodeId;

        internal InnerEventListener(EventListener outerListener, int nodeId)
        {
            _outerListener = outerListener;
            _nodeId = nodeId;
        }

        public override void React(TEvent @event)
        {
            _outerListener.OnInnerEvent( @event );
        }

        public override void OnDispose(DisposalSource _)
        {
            _outerListener.OnInnerDisposed( _nodeId );
        }
    }
}
