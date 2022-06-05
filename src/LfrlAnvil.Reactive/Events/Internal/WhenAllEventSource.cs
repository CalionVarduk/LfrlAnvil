﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.Events.Decorators;

namespace LfrlAnvil.Reactive.Events.Internal
{
    internal sealed class WhenAllEventSource<TEvent> : EventSource<ReadOnlyMemory<TEvent?>>
    {
        private readonly IEventStream<TEvent>[] _streams;

        internal WhenAllEventSource(IEnumerable<IEventStream<TEvent>> streams)
        {
            var lastDecorator = new EventListenerLastDecorator<TEvent>();
            _streams = streams.Select( s => s.Decorate( lastDecorator ) ).ToArray();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            Array.Clear( _streams, 0, _streams.Length );
        }

        protected override IEventListener<ReadOnlyMemory<TEvent?>> OverrideListener(
            IEventSubscriber subscriber,
            IEventListener<ReadOnlyMemory<TEvent?>> listener)
        {
            return IsDisposed ? listener : new EventListener( listener, subscriber, _streams );
        }

        private sealed class EventListener : DecoratedEventListener<ReadOnlyMemory<TEvent?>, ReadOnlyMemory<TEvent?>>
        {
            private readonly IEventSubscriber[] _innerSubscribers;
            private readonly IEventSubscriber _subscriber;
            private readonly TEvent?[] _result;
            private readonly int _streamCount;
            private int _disposedCount;

            internal EventListener(
                IEventListener<ReadOnlyMemory<TEvent?>> next,
                IEventSubscriber subscriber,
                IReadOnlyList<IEventStream<TEvent>> streams)
                : base( next )
            {
                _disposedCount = 0;
                _streamCount = streams.Count;
                _subscriber = subscriber;

                if ( _streamCount == 0 )
                {
                    _innerSubscribers = Array.Empty<IEventSubscriber>();
                    _result = Array.Empty<TEvent?>();
                    Next.React( _result.AsMemory() );
                    _subscriber.Dispose();
                    return;
                }

                _innerSubscribers = new IEventSubscriber[_streamCount];
                _result = new TEvent?[_streamCount];

                for ( var i = 0; i < _streamCount; ++i )
                {
                    var innerListener = new InnerEventListener( this, i );
                    _innerSubscribers[i] = streams[i].Listen( innerListener );
                }
            }

            public override void React(ReadOnlyMemory<TEvent?> _) { }

            public override void OnDispose(DisposalSource source)
            {
                foreach ( var subscriber in _innerSubscribers )
                    subscriber.Dispose();

                Array.Clear( _innerSubscribers, 0, _innerSubscribers.Length );

                Next.React( _result.AsMemory() );
                Array.Clear( _result, 0, _result.Length );

                base.OnDispose( source );
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal void OnInnerEvent(int index, TEvent @event)
            {
                _result[index] = @event;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal void OnInnerDisposed()
            {
                if ( ++_disposedCount == _streamCount )
                    _subscriber.Dispose();
            }
        }

        private sealed class InnerEventListener : EventListener<TEvent>
        {
            private readonly int _index;
            private EventListener? _outerListener;

            internal InnerEventListener(EventListener outerListener, int index)
            {
                _outerListener = outerListener;
                _index = index;
            }

            public override void React(TEvent @event)
            {
                _outerListener!.OnInnerEvent( _index, @event );
            }

            public override void OnDispose(DisposalSource _)
            {
                _outerListener!.OnInnerDisposed();
                _outerListener = null;
            }
        }
    }
}