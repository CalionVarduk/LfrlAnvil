using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Async;

namespace LfrlAnvil.Reactive.Internal;

/// <summary>
/// Represents a concurrent version of a generic disposable event source that can be listened to.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TSource">Underlying event source type.</typeparam>
public class ConcurrentEventSource<TEvent, TSource> : IEventSource<TEvent>
    where TSource : EventSource<TEvent>
{
    /// <summary>
    /// Creates a new <see cref="ConcurrentEventSource{TEvent,TSource}"/> instance.
    /// </summary>
    /// <param name="base">Underlying event source.</param>
    protected internal ConcurrentEventSource(TSource @base)
    {
        Base = @base;
        Sync = new object();
    }

    /// <summary>
    /// Underlying event source.
    /// </summary>
    protected TSource Base { get; }

    /// <summary>
    /// Object used for thread synchronization.
    /// </summary>
    protected internal object Sync { get; }

    /// <inheritdoc />
    public bool IsDisposed => Base.IsDisposed;

    /// <inheritdoc />
    public IReadOnlyCollection<IEventSubscriber> Subscribers =>
        new ConcurrentReadOnlyCollection<IEventSubscriber>( Base.Subscribers, Sync );

    /// <inheritdoc />
    public bool HasSubscribers => Subscribers.Count > 0;

    /// <inheritdoc />
    public virtual void Dispose()
    {
        lock ( Sync )
        {
            Base.Dispose();
        }
    }

    /// <inheritdoc />
    [Pure]
    public IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator)
    {
        return new ConcurrentDecoratedEventSource<TEvent, TNextEvent, TSource>( this, decorator );
    }

    /// <inheritdoc />
    public IEventSubscriber Listen(IEventListener<TEvent> listener)
    {
        lock ( Sync )
        {
            var subscriber = new EventSubscriber<TEvent>( RemoveSubscriber, listener );
            return Base.ListenInternal( subscriber );
        }
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
