﻿// Copyright 2024 Łukasz Furlepa
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
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.Composites;
using LfrlAnvil.Reactive.Decorators;

namespace LfrlAnvil.Reactive.Internal;

/// <summary>
/// Represents a generic disposable event source that can be listened to,
/// that notifies its listeners once any inner event source is disposed, with the last event published by that disposed inner event source,
/// and then disposes the listener.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class WhenAnyEventSource<TEvent> : EventSource<WithIndex<TEvent>>
{
    private readonly IEventStream<TEvent>[] _streams;

    internal WhenAnyEventSource(IEnumerable<IEventStream<TEvent>> streams)
    {
        var firstDecorator = new EventListenerFirstDecorator<TEvent>();
        _streams = streams.Select( s => s.Decorate( firstDecorator ) ).ToArray();
    }

    /// <inheritdoc />
    protected override void OnDispose()
    {
        base.OnDispose();
        Array.Clear( _streams, 0, _streams.Length );
    }

    /// <inheritdoc />
    protected override IEventListener<WithIndex<TEvent>> OverrideListener(
        IEventSubscriber subscriber,
        IEventListener<WithIndex<TEvent>> listener)
    {
        return IsDisposed ? listener : new EventListener( listener, subscriber, _streams );
    }

    private sealed class EventListener : DecoratedEventListener<WithIndex<TEvent>, WithIndex<TEvent>>
    {
        private readonly IEventSubscriber _subscriber;
        private readonly IEventSubscriber?[] _innerSubscribers;
        private readonly int _streamCount;
        private int _disposedCount;

        internal EventListener(
            IEventListener<WithIndex<TEvent>> next,
            IEventSubscriber subscriber,
            IReadOnlyList<IEventStream<TEvent>> streams)
            : base( next )
        {
            _subscriber = subscriber;
            _disposedCount = 0;
            _streamCount = streams.Count;

            if ( _streamCount == 0 )
            {
                _innerSubscribers = Array.Empty<IEventSubscriber?>();
                _subscriber.Dispose();
                return;
            }

            _innerSubscribers = new IEventSubscriber?[_streamCount];

            for ( var i = 0; i < _streamCount; ++i )
            {
                if ( _subscriber.IsDisposed )
                    break;

                var innerListener = new InnerEventListener( this, i );
                _innerSubscribers[i] = streams[i].Listen( innerListener );
            }
        }

        public override void React(WithIndex<TEvent> @event)
        {
            Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            foreach ( var subscriber in _innerSubscribers )
                subscriber?.Dispose();

            Array.Clear( _innerSubscribers, 0, _innerSubscribers.Length );
            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnInnerEvent(int index, TEvent @event)
        {
            var nextEvent = new WithIndex<TEvent>( @event, index );
            React( nextEvent );
            _subscriber.Dispose();
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
            Assume.IsNotNull( _outerListener );
            _outerListener.OnInnerEvent( _index, @event );
        }

        public override void OnDispose(DisposalSource _)
        {
            Assume.IsNotNull( _outerListener );
            _outerListener.OnInnerDisposed();
            _outerListener = null;
        }
    }
}
