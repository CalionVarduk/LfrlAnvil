using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Async;
using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Exchanges;

public sealed class EventExchange : IMutableEventExchange
{
    private readonly Dictionary<Type, IEventPublisher> _publishers;
    private InterlockedBoolean _isDisposed;

    public EventExchange()
    {
        _publishers = new Dictionary<Type, IEventPublisher>();
        _isDisposed = new InterlockedBoolean( false );
    }

    public bool IsDisposed => _isDisposed.Value;

    public void Dispose()
    {
        if ( ! _isDisposed.WriteTrue() )
            return;

        foreach ( var (_, publisher) in _publishers )
            publisher.Dispose();

        _publishers.Clear();
        _publishers.TrimExcess();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public IEnumerable<Type> GetRegisteredEventTypes()
    {
        return _publishers.Keys;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool IsRegistered<TEvent>()
    {
        return IsRegistered( typeof( TEvent ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool IsRegistered(Type eventType)
    {
        return _publishers.ContainsKey( eventType );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public IEventStream<TEvent> GetStream<TEvent>()
    {
        return GetPublisher<TEvent>();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public IEventStream GetStream(Type eventType)
    {
        return GetPublisher( eventType );
    }

    public bool TryGetStream<TEvent>([MaybeNullWhen( false )] out IEventStream<TEvent> result)
    {
        if ( _publishers.TryGetValue( typeof( TEvent ), out var publisher ) )
        {
            result = ( IEventStream<TEvent> )publisher;
            return true;
        }

        result = null;
        return false;
    }

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

    [Pure]
    public IEventPublisher<TEvent> GetPublisher<TEvent>()
    {
        if ( ! TryGetPublisher<TEvent>( out var publisher ) )
            throw new EventPublisherNotFoundException( typeof( TEvent ) );

        return publisher;
    }

    [Pure]
    public IEventPublisher GetPublisher(Type eventType)
    {
        if ( ! TryGetPublisher( eventType, out var publisher ) )
            throw new EventPublisherNotFoundException( eventType );

        return publisher;
    }

    public bool TryGetPublisher<TEvent>([MaybeNullWhen( false )] out IEventPublisher<TEvent> result)
    {
        if ( _publishers.TryGetValue( typeof( TEvent ), out var publisher ) )
        {
            result = ( IEventPublisher<TEvent> )publisher;
            return true;
        }

        result = null;
        return false;
    }

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

    public IEventSubscriber Listen<TEvent>(IEventListener<TEvent> listener)
    {
        var publisher = GetPublisher<TEvent>();
        return publisher.Listen( listener );
    }

    public IEventSubscriber Listen(Type eventType, IEventListener listener)
    {
        var publisher = GetPublisher( eventType );
        return publisher.Listen( listener );
    }

    public bool TryListen<TEvent>(IEventListener<TEvent> listener, [MaybeNullWhen( false )] out IEventSubscriber subscriber)
    {
        if ( TryGetPublisher<TEvent>( out var publisher ) )
        {
            subscriber = publisher.Listen( listener );
            return true;
        }

        subscriber = null;
        return false;
    }

    public bool TryListen(Type eventType, IEventListener listener, [MaybeNullWhen( false )] out IEventSubscriber subscriber)
    {
        if ( TryGetPublisher( eventType, out var publisher ) )
        {
            subscriber = publisher.Listen( listener );
            return true;
        }

        subscriber = null;
        return false;
    }

    public void Publish<TEvent>(TEvent @event)
    {
        if ( ! TryPublish( @event ) )
            throw new EventPublisherNotFoundException( typeof( TEvent ) );
    }

    public bool TryPublish<TEvent>(TEvent @event)
    {
        if ( ! TryGetPublisher<TEvent>( out var publisher ) )
            return false;

        publisher.Publish( @event );
        return true;
    }

    public void Publish(Type eventType, object? @event)
    {
        if ( ! TryPublish( eventType, @event ) )
            throw new EventPublisherNotFoundException( eventType );
    }

    public bool TryPublish(Type eventType, object? @event)
    {
        if ( ! TryGetPublisher( eventType, out var publisher ) )
            return false;

        publisher.Publish( @event );
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public IEventPublisher<TEvent> RegisterPublisher<TEvent>()
    {
        return RegisterPublisher( new EventPublisher<TEvent>() );
    }

    public IEventPublisher<TEvent> RegisterPublisher<TEvent>(IEventPublisher<TEvent> publisher)
    {
        if ( IsDisposed )
            throw new ObjectDisposedException( Resources.DisposedEventExchange );

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
