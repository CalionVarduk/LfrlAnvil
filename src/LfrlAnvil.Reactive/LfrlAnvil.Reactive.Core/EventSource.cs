using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Async;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive;

/// <inheritdoc />
public abstract class EventSource<TEvent> : IEventSource<TEvent>
{
    private readonly List<EventSubscriber<TEvent>> _subscribers;
    private SubscriberPool _subscriberPool;
    private InterlockedBoolean _isDisposed;

    /// <summary>
    /// Creates a new <see cref="EventSource{TEvent}"/> instance.
    /// </summary>
    protected EventSource()
    {
        _isDisposed = new InterlockedBoolean( false );
        _subscribers = new List<EventSubscriber<TEvent>>();
        _subscriberPool = SubscriberPool.Create();
    }

    /// <inheritdoc />
    public bool IsDisposed => _isDisposed.Value;

    /// <inheritdoc />
    public IReadOnlyCollection<IEventSubscriber> Subscribers => _subscribers;

    /// <inheritdoc />
    public bool HasSubscribers => _subscribers.Count > 0;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IEventSubscriber Listen(IEventListener<TEvent> listener)
    {
        var subscriber = new EventSubscriber<TEvent>( RemoveSubscriber, listener );
        return ListenInternal( subscriber );
    }

    /// <inheritdoc />
    [Pure]
    public IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator)
    {
        return new DecoratedEventSource<TEvent, TNextEvent>( this, decorator );
    }

    /// <summary>
    /// Allows to react to attachment of a new event subscriber.
    /// </summary>
    /// <param name="subscriber">Attached event subscriber.</param>
    /// <param name="listener">Event listener attached to the event subscriber.</param>
    protected virtual void OnSubscriberAdded(IEventSubscriber subscriber, IEventListener<TEvent> listener) { }

    /// <summary>
    /// Allows to override the event listener.
    /// </summary>
    /// <param name="subscriber">Event subscriber.</param>
    /// <param name="listener">Event listener to override.</param>
    /// <returns><see cref="IEventListener{TEvent}"/> instance.</returns>
    protected virtual IEventListener<TEvent> OverrideListener(IEventSubscriber subscriber, IEventListener<TEvent> listener)
    {
        return listener;
    }

    /// <summary>
    /// Allows to notify all current event listeners that an event has occurred.
    /// </summary>
    /// <param name="event">Event to notify with.</param>
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

    /// <summary>
    /// Allows to provide custom disposal implementation.
    /// </summary>
    protected virtual void OnDispose() { }

    /// <summary>
    /// Throws an exception when this event source has been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">When this event source has been disposed.</exception>
    protected void EnsureNotDisposed()
    {
        if ( IsDisposed )
            ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.DisposedEventSource ) );
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
