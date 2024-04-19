﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Async;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive;

public abstract class EventSource<TEvent> : IEventSource<TEvent>
{
    private readonly List<EventSubscriber<TEvent>> _subscribers;
    private SubscriberPool _subscriberPool;
    private InterlockedBoolean _isDisposed;

    protected EventSource()
    {
        _isDisposed = new InterlockedBoolean( false );
        _subscribers = new List<EventSubscriber<TEvent>>();
        _subscriberPool = SubscriberPool.Create();
    }

    public bool IsDisposed => _isDisposed.Value;
    public IReadOnlyCollection<IEventSubscriber> Subscribers => _subscribers;
    public bool HasSubscribers => _subscribers.Count > 0;

    public void Dispose()
    {
        if ( ! _isDisposed.WriteTrue() )
            return;

        var (subscribers, count) = _subscriberPool.Rent( _subscribers );
        _subscribers.Clear();

        try
        {
            for ( var i = 0; i < count; ++i )
            {
                var subscriber = subscribers[i];
                if ( subscriber.MarkAsDisposed() )
                    subscriber.Listener.OnDispose( DisposalSource.EventSource );
            }
        }
        finally
        {
            _subscriberPool.Return( count );
        }

        OnDispose();
    }

    public IEventSubscriber Listen(IEventListener<TEvent> listener)
    {
        var subscriber = new EventSubscriber<TEvent>( RemoveSubscriber, listener );
        return ListenInternal( subscriber );
    }

    [Pure]
    public IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator)
    {
        return new DecoratedEventSource<TEvent, TNextEvent>( this, decorator );
    }

    protected virtual void OnSubscriberAdded(IEventSubscriber subscriber, IEventListener<TEvent> listener) { }

    protected virtual IEventListener<TEvent> OverrideListener(IEventSubscriber subscriber, IEventListener<TEvent> listener)
    {
        return listener;
    }

    protected void NotifyListeners(TEvent @event)
    {
        var (subscribers, count) = _subscriberPool.Rent( _subscribers );

        try
        {
            for ( var i = 0; i < count; ++i )
            {
                var subscriber = subscribers[i];
                if ( subscriber.IsDisposed )
                    continue;

                subscriber.Listener.React( @event );
            }
        }
        finally
        {
            _subscriberPool.Return( count );
        }
    }

    protected virtual void OnDispose() { }

    protected void EnsureNotDisposed()
    {
        if ( IsDisposed )
            throw new ObjectDisposedException( null, Resources.DisposedEventSource );
    }

    internal void RemoveSubscriber(EventSubscriber<TEvent> subscriber)
    {
        _subscribers.Remove( subscriber );
    }

    [Pure]
    internal EventSubscriber<TEvent> CreateSubscriber()
    {
        return new EventSubscriber<TEvent>( RemoveSubscriber, EventListener<TEvent>.Empty );
    }

    internal IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TEvent> subscriber)
    {
        subscriber.Listener = listener;
        return ListenInternal( subscriber );
    }

    internal IEventSubscriber ListenInternal(EventSubscriber<TEvent> subscriber)
    {
        subscriber.Listener = OverrideListener( subscriber, subscriber.Listener );

        if ( subscriber.IsDisposed )
        {
            subscriber.Listener.OnDispose( DisposalSource.Subscriber );
            return subscriber;
        }

        _subscribers.Add( subscriber );
        if ( IsDisposed )
        {
            if ( subscriber.MarkAsDisposed() )
            {
                RemoveSubscriber( subscriber );
                subscriber.Listener.OnDispose( DisposalSource.EventSource );
            }

            return subscriber;
        }

        OnSubscriberAdded( subscriber, subscriber.Listener );
        return subscriber;
    }

    private struct SubscriberPool
    {
        private EventSubscriber<TEvent>[][] _buffer;
        private int _level;

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static SubscriberPool Create()
        {
            return new SubscriberPool
            {
                _buffer = Array.Empty<EventSubscriber<TEvent>[]>(),
                _level = -1
            };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal (EventSubscriber<TEvent>[] Buffer, int Count) Rent(List<EventSubscriber<TEvent>> subscribers)
        {
            if ( ++_level >= _buffer.Length )
            {
                var newBuffer = new EventSubscriber<TEvent>[_level + 1][];
                for ( var i = 0; i < _buffer.Length; ++i )
                    newBuffer[i] = _buffer[i];

                newBuffer[^1] = Array.Empty<EventSubscriber<TEvent>>();
                _buffer = newBuffer;
            }

            var count = subscribers.Count;
            var result = _buffer[_level];
            if ( count > result.Length )
            {
                result = new EventSubscriber<TEvent>[count];
                _buffer[_level] = result;
            }

            subscribers.CopyTo( result );
            return (result, count);
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Return(int count)
        {
            var buffer = _buffer[_level--];
            Array.Clear( buffer, 0, count );
        }
    }

    IEventSubscriber IEventStream.Listen(IEventListener listener)
    {
        return Listen( Argument.CastTo<IEventListener<TEvent>>( listener ) );
    }
}
