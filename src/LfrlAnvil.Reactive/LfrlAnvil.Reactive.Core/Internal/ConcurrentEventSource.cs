using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Async;
using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Internal;

public class ConcurrentEventSource<TEvent, TSource> : IEventSource<TEvent>
    where TSource : EventSource<TEvent>
{
    protected internal ConcurrentEventSource(TSource @base)
    {
        Base = @base;
        Sync = new object();
    }

    protected TSource Base { get; }
    protected internal object Sync { get; }

    public bool IsDisposed => Base.IsDisposed;

    public IReadOnlyCollection<IEventSubscriber> Subscribers =>
        new ConcurrentReadOnlyCollection<IEventSubscriber>( Base.Subscribers, Sync );

    public bool HasSubscribers => Subscribers.Count > 0;

    public virtual void Dispose()
    {
        lock ( Sync )
        {
            Base.Dispose();
        }
    }

    [Pure]
    public IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator)
    {
        return new ConcurrentDecoratedEventSource<TEvent, TNextEvent, TSource>( this, decorator );
    }

    public IEventSubscriber Listen(IEventListener<TEvent> listener)
    {
        lock ( Sync )
        {
            var subscriber = new EventSubscriber<TEvent>( RemoveSubscriber, listener );
            return Base.ListenInternal( subscriber );
        }
    }

    protected void EnsureNotDisposed()
    {
        if ( IsDisposed )
            throw new ObjectDisposedException( null, Resources.DisposedEventSource );
    }

    [Pure]
    internal EventSubscriber<TEvent> CreateSubscriber()
    {
        return new EventSubscriber<TEvent>( RemoveSubscriber, EventListener<TEvent>.Empty );
    }

    internal IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TEvent> subscriber)
    {
        subscriber.Listener = listener;
        return Base.ListenInternal( subscriber );
    }

    private void RemoveSubscriber(EventSubscriber<TEvent> subscriber)
    {
        lock ( Sync )
        {
            Base.RemoveSubscriber( subscriber );
        }
    }

    IEventSubscriber IEventStream.Listen(IEventListener listener)
    {
        return Listen( Argument.CastTo<IEventListener<TEvent>>( listener ) );
    }
}
