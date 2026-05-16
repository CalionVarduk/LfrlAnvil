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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Async;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;

namespace LfrlAnvil.Reactive.Internal;

/// <summary>
/// Represents a generic disposable event source that can be listened to,
/// that notifies its listeners with events published by any of its inner event streams.
/// Number of maximum active inner event streams can be limited, in which case the inner event streams are activated sequentially.
/// Event listener gets disposed once all inner event streams get disposed.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class MergeEventSource<TEvent> : EventSource<TEvent>
{
    private IEventStream<TEvent>[] _streams;
    private readonly int _maxConcurrency;

    internal MergeEventSource(IEnumerable<IEventStream<TEvent>> streams, int maxConcurrency)
    {
        Ensure.IsGreaterThan( maxConcurrency, 0 );
        _maxConcurrency = maxConcurrency;
        _streams = streams.ToArray();
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
    protected override IEventListener<TEvent> OverrideListenerUnsafe(IEventSubscriber subscriber, IEventListener<TEvent> listener)
    {
        IEventStream<TEvent>[] streams;
        using ( AcquireLock() )
        {
            if ( IsDisposedUnsafe() )
                return listener;

            streams = _streams;
        }

        return new EventListener( listener, subscriber, streams, _maxConcurrency );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly object _sync = new object();
        private readonly ReadOnlyArray<IEventStream<TEvent>> _streams;
        private readonly IEventSubscriber _subscriber;
        private InnerSubscribersCollection _innerSubscribers;
        private readonly int _maxConcurrency;
        private int _nextStreamIndex;
        private bool _isDisposed;

        internal EventListener(
            IEventListener<TEvent> next,
            IEventSubscriber subscriber,
            ReadOnlyArray<IEventStream<TEvent>> streams,
            int maxConcurrency)
            : base( next )
        {
            _innerSubscribers = new InnerSubscribersCollection( Math.Min( maxConcurrency, streams.Count ) );
            _streams = streams;
            _subscriber = subscriber;
            _nextStreamIndex = 0;
            _maxConcurrency = maxConcurrency;

            if ( _streams.Count == 0 )
            {
                _isDisposed = true;
                _subscriber.Dispose();
                return;
            }

            while ( TryReserveNextStream( out var nodeId, out var stream ) )
                StartListeningToNextInnerStream( nodeId, stream );
        }

        public override void React(TEvent @event)
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
                    _nextStreamIndex = _streams.Count;
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
            React( @event );
        }

        internal void OnInnerDisposed(int nodeId)
        {
            var dispose = false;
            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return;

                _innerSubscribers.Remove( nodeId );
                if ( _nextStreamIndex == _streams.Count && _innerSubscribers.Count == 0 )
                {
                    dispose = true;
                    _isDisposed = true;
                    _innerSubscribers.Clear();
                }
            }

            if ( dispose )
            {
                _subscriber.Dispose();
                return;
            }

            if ( TryReserveNextStream( out var nextNodeId, out var nextStream ) )
                StartListeningToNextInnerStream( nextNodeId, nextStream );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ExclusiveLock AcquireLock()
        {
            return ExclusiveLock.Enter( _sync );
        }

        private bool TryReserveNextStream(out int nodeId, [MaybeNullWhen( false )] out IEventStream<TEvent> stream)
        {
            using ( AcquireLock() )
            {
                if ( _isDisposed || _nextStreamIndex >= _streams.Count || _innerSubscribers.Count >= _maxConcurrency )
                {
                    nodeId = 0;
                    stream = null;
                    return false;
                }

                nodeId = _innerSubscribers.Reserve();
                stream = _streams[_nextStreamIndex++];
                return true;
            }
        }

        private void StartListeningToNextInnerStream(int nodeId, IEventStream<TEvent> stream)
        {
            var innerListener = new InnerEventListener( this, nodeId );
            var innerSubscriber = stream.Listen( innerListener );

            bool disposed;
            using ( AcquireLock() )
            {
                disposed = _isDisposed;
                if ( ! disposed )
                    _innerSubscribers.Set( nodeId, innerSubscriber );
            }

            if ( disposed )
                innerSubscriber.Dispose();
        }
    }

    private sealed class InnerEventListener : EventListener<TEvent>
    {
        private readonly int _nodeId;
        private readonly EventListener _outerListener;

        internal InnerEventListener(EventListener outerListener, int nodeId)
        {
            _nodeId = nodeId;
            _outerListener = outerListener;
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
