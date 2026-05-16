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
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;

namespace LfrlAnvil.Reactive.Internal;

/// <summary>
/// Represents a generic disposable event source that can be listened to,
/// that notifies its listeners when any of the inner event streams publishes an event, with a sequence of last events published
/// by inner event streams. First event will be published once all inner event streams publish at least one event.
/// Event listeners will be disposed when all inner event streams get disposed or when the first event has not been published yet
/// and any of the inner streams gets disposed.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class CombineEventSource<TEvent> : EventSource<ReadOnlyMemory<TEvent>>
{
    private IEventStream<TEvent>[] _streams;

    internal CombineEventSource(IEnumerable<IEventStream<TEvent>> streams)
    {
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
    protected override IEventListener<ReadOnlyMemory<TEvent>> OverrideListenerUnsafe(
        IEventSubscriber subscriber,
        IEventListener<ReadOnlyMemory<TEvent>> listener)
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

    private sealed class EventListener : DecoratedEventListener<ReadOnlyMemory<TEvent>, ReadOnlyMemory<TEvent>>
    {
        private readonly Lock _lock = new Lock();
        private readonly int _streamCount;
        private readonly IEventSubscriber _subscriber;
        private InnerSubscribersCollection _innerSubscribers;
        private TEvent?[] _buffer;
        private ArrayPoolToken<TEvent>? _pending;
        private int _withValueCount;
        private int _disposedCount;
        private bool _isEmitting;
        private bool _isDisposed;

        internal EventListener(
            IEventListener<ReadOnlyMemory<TEvent>> next,
            IEventSubscriber subscriber,
            ReadOnlyArray<IEventStream<TEvent>> streams)
            : base( next )
        {
            _subscriber = subscriber;
            _withValueCount = 0;
            _disposedCount = 0;
            _streamCount = streams.Count;

            if ( _streamCount == 0 )
            {
                _isDisposed = true;
                _innerSubscribers = new InnerSubscribersCollection( 0 );
                _buffer = [ ];
                _subscriber.Dispose();
                return;
            }

            _innerSubscribers = new InnerSubscribersCollection( _streamCount );
            _buffer = new TEvent?[_streamCount];

            for ( var i = 0; i < _streamCount; ++i )
            {
                int nodeId;
                using ( AcquireLock() )
                {
                    if ( _isDisposed )
                        break;

                    nodeId = _innerSubscribers.Reserve();
                }

                var innerListener = new InnerEventListener( this, i );
                var innerSubscriber = streams[i].Listen( innerListener );

                bool disposed;
                using ( AcquireLock() )
                {
                    disposed = _isDisposed;
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

        public override void React(ReadOnlyMemory<TEvent> @event)
        {
            try
            {
                Next.React( @event );
            }
            catch
            {
                ArrayPoolToken<TEvent>? poolToken = null;
                try
                {
                    using ( AcquireLock() )
                    {
                        poolToken = _pending;
                        _pending = null;
                        _isEmitting = false;
                    }
                }
                finally
                {
                    poolToken?.Dispose();
                }

                throw;
            }
        }

        public override void OnDispose(DisposalSource source)
        {
            ArrayPoolToken<TEvent>? pendingToken = null;
            ArrayPoolToken<IEventSubscriber?> subscribersToken = default;
            try
            {
                ReadOnlySpan<IEventSubscriber?> subscribers;
                using ( AcquireLock() )
                {
                    if ( _isDisposed && _innerSubscribers.Count == 0 )
                        return;

                    _isDisposed = true;
                    pendingToken = _pending;
                    _pending = null;
                    _buffer = [ ];
                    subscribers = _innerSubscribers.Clear( out subscribersToken );
                }

                foreach ( var s in subscribers )
                    s?.Dispose();
            }
            finally
            {
                pendingToken?.Dispose();
                subscribersToken.Dispose();
            }

            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Lock.Scope AcquireLock()
        {
            return _lock.EnterScope();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool OnInnerEventUnsafe(int index, TEvent @event, bool firstEvent)
        {
            if ( _isDisposed )
                return false;

            _buffer[index] = @event;
            if ( _withValueCount >= _streamCount )
                return true;

            return firstEvent && ++_withValueCount >= _streamCount;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal ReadOnlyMemory<TEvent>? GetEventsToEmitUnsafe(int index, ref ArrayPoolToken<TEvent> token)
        {
            if ( _pending is not null )
            {
                Assume.True( _isEmitting );
                var pendingSpan = _pending.Value.AsSpan();
                pendingSpan[index] = _buffer[index]!;
                return null;
            }

            var poolToken = ArrayPool<TEvent>.Shared.RentToken( _buffer.Length, clearArray: true );
            var events = poolToken.AsMemory();
            _buffer.AsSpan().CopyTo( events.Span! );

            if ( _isEmitting )
            {
                _pending = poolToken;
                return null;
            }

            _isEmitting = true;
            token = poolToken;
            return events;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Emit(ReadOnlyMemory<TEvent> events)
        {
            React( events );

            while ( true )
            {
                ArrayPoolToken<TEvent> poolToken = default;
                try
                {
                    using ( AcquireLock() )
                    {
                        Assume.True( _isEmitting );
                        if ( _pending is null )
                        {
                            _isEmitting = false;
                            return;
                        }

                        events = _pending.Value.AsMemory();
                        poolToken = _pending.Value;
                        _pending = null;
                    }

                    React( events );
                }
                finally
                {
                    poolToken.Dispose();
                }
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool OnInnerDisposedUnsafe(ref ArrayPoolToken<TEvent>? token)
        {
            if ( _isDisposed || (++_disposedCount < _streamCount && _withValueCount >= _streamCount) )
                return false;

            _isDisposed = true;
            token = _pending;
            _pending = null;
            _buffer = [ ];
            return true;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void DisposeSubscriber()
        {
            _subscriber.Dispose();
        }
    }

    private sealed class InnerEventListener : EventListener<TEvent>
    {
        private readonly int _index;
        private readonly EventListener _outerListener;
        private bool _hasValue;
        private bool _isDisposed;

        internal InnerEventListener(EventListener outerListener, int index)
        {
            _outerListener = outerListener;
            _index = index;
        }

        public override void React(TEvent @event)
        {
            ArrayPoolToken<TEvent> poolToken = default;
            try
            {
                ReadOnlyMemory<TEvent>? events;
                using ( _outerListener.AcquireLock() )
                {
                    if ( _isDisposed )
                        return;

                    var firstEvent = ! _hasValue;
                    _hasValue = true;
                    if ( ! _outerListener.OnInnerEventUnsafe( _index, @event, firstEvent ) )
                        return;

                    events = _outerListener.GetEventsToEmitUnsafe( _index, ref poolToken );
                }

                if ( events is not null )
                    _outerListener.Emit( events.Value );
            }
            finally
            {
                poolToken.Dispose();
            }
        }

        public override void OnDispose(DisposalSource source)
        {
            bool dispose;
            ArrayPoolToken<TEvent>? poolToken = null;
            try
            {
                using ( _outerListener.AcquireLock() )
                {
                    if ( _isDisposed )
                        return;

                    _isDisposed = true;
                    dispose = _outerListener.OnInnerDisposedUnsafe( ref poolToken );
                }
            }
            finally
            {
                poolToken?.Dispose();
            }

            if ( dispose )
                _outerListener.DisposeSubscriber();
        }
    }
}
