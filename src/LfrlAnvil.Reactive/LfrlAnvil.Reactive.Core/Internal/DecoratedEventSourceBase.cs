using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Internal;

internal abstract class DecoratedEventSourceBase<TRootEvent, TEvent> : IEventStream<TEvent>
{
    protected DecoratedEventSourceBase(EventSource<TRootEvent> root)
    {
        Root = root;
    }

    public bool IsDisposed => Root.IsDisposed;
    protected EventSource<TRootEvent> Root { get; }

    public IEventSubscriber Listen(IEventListener<TEvent> listener)
    {
        return Listen( listener, Root.CreateSubscriber() );
    }

    [Pure]
    public abstract IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator);

    internal abstract IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TRootEvent> subscriber);

    IEventSubscriber IEventStream.Listen(IEventListener listener)
    {
        return Listen( Argument.CastTo<IEventListener<TEvent>>( listener, nameof( listener ) ) );
    }
}