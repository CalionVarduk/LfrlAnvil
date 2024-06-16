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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Async;
using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Exchanges;

/// <inheritdoc cref="IMutableEventExchange" />
public sealed class EventExchange : IMutableEventExchange
{
    private readonly Dictionary<Type, IEventPublisher> _publishers;
    private InterlockedBoolean _isDisposed;

    /// <summary>
    /// Creates a new empty <see cref="EventExchange"/> instance.
    /// </summary>
    public EventExchange()
    {
        _publishers = new Dictionary<Type, IEventPublisher>();
        _isDisposed = new InterlockedBoolean( false );
    }

    /// <inheritdoc />
    public bool IsDisposed => _isDisposed.Value;

    /// <inheritdoc />
    public void Dispose()
    {
        if ( ! _isDisposed.WriteTrue() )
            return;

        foreach ( var (_, publisher) in _publishers )
            publisher.Dispose();

        _publishers.Clear();
        _publishers.TrimExcess();
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public IEnumerable<Type> GetRegisteredEventTypes()
    {
        return _publishers.Keys;
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool IsRegistered(Type eventType)
    {
        return _publishers.ContainsKey( eventType );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public IEventStream GetStream(Type eventType)
    {
        return GetPublisher( eventType );
    }

    /// <inheritdoc />
    public bool TryGetStream(Type eventType, [MaybeNullWhen( false )] out IEventStream result)
    {
        if ( _publishers.TryGetValue( eventType, out var publisher ) )
        {
            result = publisher;
            return true;
        }

        result = null;
        return false;
    }

    /// <inheritdoc />
    [Pure]
    public IEventPublisher GetPublisher(Type eventType)
    {
        if ( ! TryGetPublisher( eventType, out var publisher ) )
            throw new EventPublisherNotFoundException( eventType );

        return publisher;
    }

    /// <inheritdoc />
    public bool TryGetPublisher(Type eventType, [MaybeNullWhen( false )] out IEventPublisher result)
    {
        if ( _publishers.TryGetValue( eventType, out var publisher ) )
        {
            result = publisher;
            return true;
        }

        result = null;
        return false;
    }

    /// <inheritdoc />
    public IEventPublisher<TEvent> RegisterPublisher<TEvent>(IEventPublisher<TEvent> publisher)
    {
        if ( IsDisposed )
            throw new ObjectDisposedException( null, Resources.DisposedEventExchange );

        if ( ! _publishers.TryAdd( typeof( TEvent ), publisher ) )
            throw new EventPublisherAlreadyExistsException( typeof( TEvent ) );

        var listener = new DisposalListener<TEvent>( this );
        publisher.Listen( listener );
        return publisher;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void RemovePublisher(Type eventType)
    {
        _publishers.Remove( eventType );
    }

    private sealed class DisposalListener<TEvent> : EventListener<TEvent>
    {
        private EventExchange? _exchange;

        internal DisposalListener(EventExchange exchange)
        {
            _exchange = exchange;
        }

        public override void React(TEvent _) { }

        public override void OnDispose(DisposalSource source)
        {
            Assume.IsNotNull( _exchange );
            if ( _exchange.IsDisposed )
                return;

            if ( source == DisposalSource.Subscriber )
                throw new InvalidEventPublisherDisposalException();

            _exchange.RemovePublisher( typeof( TEvent ) );
            _exchange = null;
        }
    }
}
