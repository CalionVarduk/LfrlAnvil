using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Internal;

public sealed class CombineEventSource<TEvent> : EventSource<ReadOnlyMemory<TEvent>>
{
    private readonly IEventStream<TEvent>[] _streams;

    internal CombineEventSource(IEnumerable<IEventStream<TEvent>> streams)
    {
        _streams = streams.ToArray();
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        Array.Clear( _streams, 0, _streams.Length );
    }

    protected override IEventListener<ReadOnlyMemory<TEvent>> OverrideListener(
        IEventSubscriber subscriber,
        IEventListener<ReadOnlyMemory<TEvent>> listener)
    {
        return IsDisposed ? listener : new EventListener( listener, subscriber, _streams );
    }

    private sealed class EventListener : DecoratedEventListener<ReadOnlyMemory<TEvent>, ReadOnlyMemory<TEvent>>
    {
        private readonly TEvent?[] _buffer;
        private readonly IEventSubscriber _subscriber;
        private readonly IEventSubscriber?[] _innerSubscribers;
        private readonly int _streamCount;
        private int _withValueCount;
        private int _disposedCount;

        internal EventListener(
            IEventListener<ReadOnlyMemory<TEvent>> next,
            IEventSubscriber subscriber,
            IReadOnlyList<IEventStream<TEvent>> streams)
            : base( next )
        {
            _subscriber = subscriber;
            _withValueCount = 0;
            _disposedCount = 0;
            _streamCount = streams.Count;

            if ( _streamCount == 0 )
            {
                _innerSubscribers = Array.Empty<IEventSubscriber?>();
                _buffer = Array.Empty<TEvent?>();
                _subscriber.Dispose();
                return;
            }

            _innerSubscribers = new IEventSubscriber?[_streamCount];
            _buffer = new TEvent?[_streamCount];

            for ( var i = 0; i < _streamCount; ++i )
            {
                if ( _subscriber.IsDisposed )
                    break;

                var innerListener = new InnerEventListener( this, i );
                _innerSubscribers[i] = streams[i].Listen( innerListener );
            }
        }

        public override void React(ReadOnlyMemory<TEvent> @event)
        {
            Next.React( _buffer.AsMemory()! );
        }

        public override void OnDispose(DisposalSource source)
        {
            foreach ( var subscriber in _innerSubscribers )
                subscriber?.Dispose();

            Array.Clear( _innerSubscribers, 0, _innerSubscribers.Length );
            Array.Clear( _buffer, 0, _buffer.Length );

            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnInnerEvent(int index, TEvent @event, bool firstEvent)
        {
            _buffer[index] = @event;
            if ( _withValueCount < _streamCount )
            {
                if ( ! firstEvent || ++_withValueCount < _streamCount )
                    return;
            }

            React( _buffer.AsMemory()! );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnInnerDisposed()
        {
            if ( ++_disposedCount == _streamCount || _withValueCount < _streamCount )
                _subscriber.Dispose();
        }
    }

    private sealed class InnerEventListener : EventListener<TEvent>
    {
        private readonly int _index;
        private EventListener? _outerListener;
        private bool _hasValue;

        internal InnerEventListener(EventListener outerListener, int index)
        {
            _outerListener = outerListener;
            _index = index;
            _hasValue = false;
        }

        public override void React(TEvent @event)
        {
            Assume.IsNotNull( _outerListener );
            var firstEvent = ! _hasValue;
            _hasValue = true;
            _outerListener.OnInnerEvent( _index, @event, firstEvent );
        }

        public override void OnDispose(DisposalSource source)
        {
            Assume.IsNotNull( _outerListener );
            _outerListener.OnInnerDisposed();
            _outerListener = null;
        }
    }
}
