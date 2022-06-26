using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Internal;

public sealed class MergeEventSource<TEvent> : EventSource<TEvent>
{
    private readonly IEventStream<TEvent>[] _streams;
    private readonly int _maxConcurrency;

    internal MergeEventSource(IEnumerable<IEventStream<TEvent>> streams, int maxConcurrency)
    {
        Ensure.IsGreaterThan( maxConcurrency, 0, nameof( maxConcurrency ) );
        _maxConcurrency = maxConcurrency;
        _streams = streams.ToArray();
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        Array.Clear( _streams, 0, _streams.Length );
    }

    protected override IEventListener<TEvent> OverrideListener(IEventSubscriber subscriber, IEventListener<TEvent> listener)
    {
        return IsDisposed ? listener : new EventListener( listener, subscriber, _streams, _maxConcurrency );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly LinkedList<IEventSubscriber?> _innerSubscribers;
        private readonly IReadOnlyList<IEventStream<TEvent>> _streams;
        private readonly IEventSubscriber _subscriber;
        private int _nextStreamIndex;

        internal EventListener(
            IEventListener<TEvent> next,
            IEventSubscriber subscriber,
            IReadOnlyList<IEventStream<TEvent>> streams,
            int maxConcurrency)
            : base( next )
        {
            _innerSubscribers = new LinkedList<IEventSubscriber?>();
            _streams = streams;
            _subscriber = subscriber;
            _nextStreamIndex = 0;

            if ( _streams.Count == 0 )
            {
                _subscriber.Dispose();
                return;
            }

            while ( _nextStreamIndex < _streams.Count && _innerSubscribers.Count < maxConcurrency )
                StartListeningToNextInnerStream();
        }

        public override void React(TEvent @event)
        {
            Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _nextStreamIndex = _streams.Count;

            foreach ( var subscriber in _innerSubscribers )
                subscriber?.Dispose();

            _innerSubscribers.Clear();

            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnInnerEvent(TEvent @event)
        {
            React( @event );
        }

        internal void OnInnerDisposed(LinkedListNode<IEventSubscriber?> node)
        {
            if ( _subscriber.IsDisposed )
                return;

            _innerSubscribers.Remove( node );

            if ( _nextStreamIndex == _streams.Count )
            {
                if ( _innerSubscribers.Count == 0 )
                    _subscriber.Dispose();

                return;
            }

            StartListeningToNextInnerStream();
        }

        private void StartListeningToNextInnerStream()
        {
            var stream = _streams[_nextStreamIndex++];

            var innerSubscriberNode = _innerSubscribers.AddLast( (IEventSubscriber?)null );
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
            _outerListener!.OnInnerEvent( @event );
        }

        public override void OnDispose(DisposalSource _)
        {
            _outerListener!.OnInnerDisposed( _subscriberNode );
            _outerListener = null;
        }
    }
}
