using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Internal;

internal abstract class ConcurrentDecoratedEventSourceBase<TRootEvent, TEvent, TRootSource> : IEventStream<TEvent>
    where TRootSource : EventSource<TRootEvent>
{
    protected ConcurrentDecoratedEventSourceBase(ConcurrentEventSource<TRootEvent, TRootSource> root)
    {
        Root = root;
    }

    public bool IsDisposed => Root.IsDisposed;
    protected ConcurrentEventSource<TRootEvent, TRootSource> Root { get; }

    public IEventSubscriber Listen(IEventListener<TEvent> listener)
    {
        lock ( Root.Sync )
        {
            return Listen( listener, Root.CreateSubscriber() );
        }
    }

    [Pure]
    public abstract IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator);

    internal abstract IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TRootEvent> subscriber);

    IEventSubscriber IEventStream.Listen(IEventListener listener)
    {
        return Listen( Argument.CastTo<IEventListener<TEvent>>( listener, nameof( listener ) ) );
    }
}