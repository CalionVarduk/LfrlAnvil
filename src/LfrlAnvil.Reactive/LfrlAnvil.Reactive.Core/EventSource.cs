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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive;

/// <inheritdoc cref="IEventSource{TEvent}" />
public abstract class EventSource<TEvent> : IEventSource<TEvent>
{
    private readonly Lock _lock = new Lock();
    private ListSlim<EventSubscriber<TEvent>> _subscribers;
    private bool _isDisposed;

    /// <summary>
    /// Creates a new <see cref="EventSource{TEvent}"/> instance.
    /// </summary>
    protected EventSource()
    {
        _subscribers = ListSlim<EventSubscriber<TEvent>>.Create();
        _isDisposed = false;
    }

    /// <inheritdoc />
    public bool IsDisposed
    {
        get
        {
            using ( AcquireLock() )
                return _isDisposed;
        }
    }

    /// <inheritdoc cref="IEventSource.Subscribers" />
    public IEventSubscriber[] Subscribers
    {
        get
        {
            using ( AcquireLock() )
            {
                if ( _subscribers.IsEmpty )
                    return Array.Empty<IEventSubscriber>();

                var i = 0;
                var result = new IEventSubscriber[_subscribers.Count];
                foreach ( var s in _subscribers )
                    result[i++] = s;

                return result;
            }
        }
    }

    /// <inheritdoc />
    public bool HasSubscribers
    {
        get
        {
            using ( AcquireLock() )
                return ! _subscribers.IsEmpty;
        }
    }

    IReadOnlyCollection<IEventSubscriber> IEventSource.Subscribers => Subscribers;

    /// <inheritdoc />
    public virtual void Dispose()
    {
        if ( DisposeCore( out var exceptions ) && exceptions.Count > 0 )
            exceptions.Consolidate()?.Rethrow();
    }

    /// <inheritdoc />
    public IEventSubscriber Listen(IEventListener<TEvent> listener)
    {
        var subscriber = new EventSubscriber<TEvent>( this, listener );
        return ListenCore( subscriber );
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
    /// <remarks>This method call is not thread-safe by default.</remarks>
    protected virtual void OnSubscriberAddedUnsafe(IEventSubscriber subscriber, IEventListener<TEvent> listener) { }

    /// <summary>
    /// Allows to override an event listener.
    /// </summary>
    /// <param name="subscriber">Event subscriber.</param>
    /// <param name="listener">Event listener to override.</param>
    /// <returns><see cref="IEventListener{TEvent}"/> instance.</returns>
    /// <remarks>This method call is not thread-safe by default.</remarks>
    protected virtual IEventListener<TEvent> OverrideListenerUnsafe(IEventSubscriber subscriber, IEventListener<TEvent> listener)
    {
        return listener;
    }

    /// <summary>
    /// Allows to override an event listener after subscriber has been registered.
    /// </summary>
    /// <param name="subscriber">Event subscriber.</param>
    /// <param name="listener">Event listener to override.</param>
    /// <returns><see cref="IEventListener{TEvent}"/> instance.</returns>
    protected virtual IEventListener<TEvent> OnSubscriberRegistered(IEventSubscriber subscriber, IEventListener<TEvent> listener)
    {
        return listener;
    }

    /// <summary>
    /// Performs safe disposal and cleans up all subscribers.
    /// </summary>
    /// <param name="exceptions">
    /// <b>out</b> parameter which returns all caught exceptions. Will always be empty when this method returns <b>false</b>.
    /// </param>
    /// <returns><b>false</b> when this event source was already disposed, otherwise <b>true</b>.</returns>
    protected bool DisposeCore(out Chain<Exception> exceptions)
    {
        ArrayPoolToken<IEventSubscriber> poolToken = default;
        exceptions = Chain<Exception>.Empty;
        try
        {
            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return false;

                poolToken = GetCurrentSubscribersUnsafe();
                _subscribers.Clear();
                _isDisposed = true;
            }

            var subscribers = poolToken.AsSpan();
            foreach ( var s in subscribers )
            {
                try
                {
                    var subscriber = ReinterpretCast.To<EventSubscriber<TEvent>>( s );
                    if ( subscriber.MarkAsDisposed() )
                        subscriber.Listener.OnDispose( DisposalSource.EventSource );
                }
                catch ( Exception exc )
                {
                    exceptions = exceptions.Extend( exc );
                }
            }
        }
        finally
        {
            poolToken.Dispose();
        }

        return true;
    }

    /// <summary>
    /// Gets all currently active subscribers and stores them into an array pool token.
    /// </summary>
    /// <returns>Array pool token containing all currently active subscribers.</returns>
    /// <remarks>This method call is not thread-safe by default.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected ArrayPoolToken<IEventSubscriber> GetCurrentSubscribersUnsafe()
    {
        if ( _subscribers.IsEmpty )
            return default;

        var token = ArrayPool<IEventSubscriber>.Shared.RentToken( _subscribers.Count, clearArray: true );
        var subscribers = token.AsSpan();

        var i = 0;
        foreach ( var s in _subscribers )
            subscribers[i++] = s;

        return token;
    }

    /// <summary>
    /// Allows to notify all provided listeners via array pool <paramref name="token"/> that an event has occurred.
    /// </summary>
    /// <param name="token">
    /// Array pool token containing subscribers to notify.
    /// This should be a token returned by <see cref="GetCurrentSubscribersUnsafe"/> method.
    /// </param>
    /// <param name="event">Event to notify with.</param>
    /// <returns>Collection of all caught exceptions.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected Chain<Exception> NotifyListeners(ArrayPoolToken<IEventSubscriber> token, TEvent @event)
    {
        var subscribers = token.AsSpan();
        var exceptions = Chain<Exception>.Empty;
        foreach ( var s in subscribers )
        {
            try
            {
                var subscriber = ReinterpretCast.To<EventSubscriber<TEvent>>( s );
                if ( ! subscriber.IsDisposed )
                    subscriber.Listener.React( @event );
            }
            catch ( Exception exc )
            {
                exceptions = exceptions.Extend( exc );
            }
        }

        return exceptions;
    }

    /// <summary>
    /// Allows to notify all current event listeners that an event has occurred.
    /// </summary>
    /// <param name="event">Event to notify with.</param>
    /// <returns><b>false</b> when this event source was disposed and listeners were not notified, otherwise <b>true</b>.</returns>
    protected bool TryNotifyListeners(TEvent @event)
    {
        ArrayPoolToken<IEventSubscriber> poolToken = default;
        try
        {
            using ( AcquireLock() )
            {
                if ( _isDisposed )
                    return false;

                poolToken = GetCurrentSubscribersUnsafe();
            }

            var exceptions = NotifyListeners( poolToken, @event );
            if ( exceptions.Count > 0 )
                exceptions.Consolidate()?.Rethrow();
        }
        finally
        {
            poolToken.Dispose();
        }

        return true;
    }

    /// <summary>
    /// Throws an exception when this event source has been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">When this event source has been disposed.</exception>
    /// <remarks>This method call is not thread-safe by default.</remarks>
    [DoesNotReturn]
    protected void ThrowDisposedException()
    {
        ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.DisposedEventSource ) );
    }

    /// <summary>
    /// Acquires this event source's exclusive lock.
    /// </summary>
    /// <returns>Entered <see cref="Lock.Scope"/> of this event source.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected Lock.Scope AcquireLock()
    {
        return _lock.EnterScope();
    }

    /// <summary>
    /// Specifies whether this event stream has been disposed.
    /// </summary>
    /// <returns><b>true</b> when this event stream has been disposed, otherwise <b>false</b>.</returns>
    /// <remarks>This method call is not thread-safe by default.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected bool IsDisposedUnsafe()
    {
        return _isDisposed;
    }

    [Pure]
    internal EventSubscriber<TEvent> CreateSubscriber()
    {
        return new EventSubscriber<TEvent>( this, EventListener<TEvent>.Empty );
    }

    internal void RemoveSubscriber(EventSubscriber<TEvent> subscriber)
    {
        using ( AcquireLock() )
        {
            for ( var i = 0; i < _subscribers.Count; ++i )
            {
                if ( ReferenceEquals( _subscribers[i], subscriber ) )
                {
                    _subscribers.RemoveAt( i );
                    break;
                }
            }
        }
    }

    internal IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TEvent> subscriber)
    {
        subscriber.Listener = listener;
        return ListenCore( subscriber );
    }

    private EventSubscriber<TEvent> ListenCore(EventSubscriber<TEvent> subscriber)
    {
        subscriber.Listener = OverrideListenerUnsafe( subscriber, subscriber.Listener );

        DisposalSource? disposalSource = null;
        using ( AcquireLock() )
        using ( subscriber.AcquireLock() )
        {
            if ( ! subscriber.HasOwnerUnsafe() )
                disposalSource = DisposalSource.Subscriber;
            else if ( _isDisposed )
            {
                disposalSource = DisposalSource.EventSource;
                subscriber.RemoveOwnerUnsafe();
            }
            else
            {
                _subscribers.Add( subscriber );
                subscriber.Listener = OnSubscriberRegistered( subscriber, subscriber.Listener );
            }
        }

        if ( disposalSource is null )
            OnSubscriberAddedUnsafe( subscriber, subscriber.Listener );
        else
            subscriber.Listener.OnDispose( disposalSource.Value );

        return subscriber;
    }

    IEventSubscriber IEventStream.Listen(IEventListener listener)
    {
        return Listen( Argument.CastTo<IEventListener<TEvent>>( listener ) );
    }
}
