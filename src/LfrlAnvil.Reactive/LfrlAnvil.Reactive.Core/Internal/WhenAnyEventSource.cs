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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
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
    private IEventStream<TEvent>[] _streams;

    internal WhenAnyEventSource(IEnumerable<IEventStream<TEvent>> streams)
    {
        var firstDecorator = new EventListenerFirstDecorator<TEvent>();
        _streams = streams.Select( s => s.Decorate( firstDecorator ) ).ToArray();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        if ( DisposeCore( out var exceptions ) )
        {
            using ( AcquireLock() )
                _streams = [ ];
        }

        if ( exceptions.Count > 0 )
            exceptions.Consolidate()?.Rethrow();
    }

    /// <inheritdoc />
    protected override IEventListener<WithIndex<TEvent>> OverrideListenerUnsafe(
        IEventSubscriber subscriber,
        IEventListener<WithIndex<TEvent>> listener)
    {
        IEventStream<TEvent>[] streams;
        using ( AcquireLock() )
        {
            if ( IsDisposedUnsafe() )
                return listener;

            streams = _streams;
        }

        return new EventListener( listener, subscriber, streams );
    }

    private sealed class EventListener : DecoratedEventListener<WithIndex<TEvent>, WithIndex<TEvent>>
    {
        private readonly Lock _lock = new Lock();
        private readonly IEventSubscriber _subscriber;
        private readonly int _streamCount;
        private InnerSubscribersCollection _innerSubscribers;
        private int _disposedCount;
        private bool _emitted;
        private bool _isDisposed;

        internal EventListener(
            IEventListener<WithIndex<TEvent>> next,
            IEventSubscriber subscriber,
            ReadOnlyArray<IEventStream<TEvent>> streams)
            : base( next )
        {
            _subscriber = subscriber;
            _disposedCount = 0;
            _streamCount = streams.Count;

            if ( _streamCount == 0 )
            {
                _isDisposed = true;
                _innerSubscribers = new InnerSubscribersCollection( 0 );
                _subscriber.Dispose();
                return;
            }

            _innerSubscribers = new InnerSubscribersCollection( _streamCount );
            for ( var i = 0; i < _streamCount; ++i )
            {
                int nodeId;
                using ( AcquireLock() )
                {
                    if ( _isDisposed || _emitted )
                        break;

                    nodeId = _innerSubscribers.Reserve();
                }

                var innerListener = new InnerEventListener( this, i );
                var innerSubscriber = streams[i].Listen( innerListener );

                bool disposed;
                using ( AcquireLock() )
                {
                    disposed = _isDisposed || _emitted;
                    if ( ! disposed )
                        _innerSubscribers.Set( nodeId, innerSubscriber );
                }

                if ( disposed )
                {
                    innerSubscriber.Dispose();
                    break;
                }
            }
        }

        public override void React(WithIndex<TEvent> @event)
        {
            Next.React( @event );
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
        internal void OnInnerEvent(int index, TEvent @event)
        {
            using ( AcquireLock() )
            {
                if ( _isDisposed || _emitted )
                    return;

                _emitted = true;
            }

            var nextEvent = new WithIndex<TEvent>( @event, index );
            try
            {
                React( nextEvent );
            }
            finally
            {
                _subscriber.Dispose();
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnInnerDisposed()
        {
            bool dispose;
            using ( AcquireLock() )
                dispose = ! _isDisposed && ++_disposedCount == _streamCount;

            if ( dispose )
                _subscriber.Dispose();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private Lock.Scope AcquireLock()
        {
            return _lock.EnterScope();
        }
    }

    private sealed class InnerEventListener : EventListener<TEvent>
    {
        private readonly int _index;
        private readonly EventListener _outerListener;

        internal InnerEventListener(EventListener outerListener, int index)
        {
            _outerListener = outerListener;
            _index = index;
        }

        public override void React(TEvent @event)
        {
            _outerListener.OnInnerEvent( _index, @event );
        }

        public override void OnDispose(DisposalSource _)
        {
            _outerListener.OnInnerDisposed();
        }
    }
}
