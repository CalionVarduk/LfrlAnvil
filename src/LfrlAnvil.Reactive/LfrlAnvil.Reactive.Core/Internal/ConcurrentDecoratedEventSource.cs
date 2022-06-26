using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Internal;

internal sealed class ConcurrentDecoratedEventSource<TRootEvent, TEvent, TRootSource>
    : ConcurrentDecoratedEventSourceBase<TRootEvent, TEvent, TRootSource>
    where TRootSource : EventSource<TRootEvent>
{
    private readonly IEventListenerDecorator<TRootEvent, TEvent> _decorator;

    internal ConcurrentDecoratedEventSource(
        ConcurrentEventSource<TRootEvent, TRootSource> root,
        IEventListenerDecorator<TRootEvent, TEvent> decorator)
        : base( root )
    {
        _decorator = decorator;
    }

    [Pure]
    public override IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator)
    {
        return new ConcurrentDecoratedEventSource<TRootEvent, TEvent, TNextEvent, TRootSource>( Root, this, decorator );
    }

    internal override IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TRootEvent> subscriber)
    {
        var sourceListener = _decorator.Decorate( listener, subscriber );
        return Root.Listen( sourceListener, subscriber );
    }
}

internal sealed class ConcurrentDecoratedEventSource<TRootEvent, TSourceEvent, TEvent, TRootSource>
    : ConcurrentDecoratedEventSourceBase<TRootEvent, TEvent, TRootSource>
    where TRootSource : EventSource<TRootEvent>
{
    private readonly ConcurrentDecoratedEventSourceBase<TRootEvent, TSourceEvent, TRootSource> _base;
    private readonly IEventListenerDecorator<TSourceEvent, TEvent> _decorator;

    internal ConcurrentDecoratedEventSource(
        ConcurrentEventSource<TRootEvent, TRootSource> root,
        ConcurrentDecoratedEventSourceBase<TRootEvent, TSourceEvent, TRootSource> @base,
        IEventListenerDecorator<TSourceEvent, TEvent> decorator)
        : base( root )
    {
        _base = @base;
        _decorator = decorator;
    }

    [Pure]
    public override IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator)
    {
        return new ConcurrentDecoratedEventSource<TRootEvent, TEvent, TNextEvent, TRootSource>( Root, this, decorator );
    }

    internal override IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TRootEvent> subscriber)
    {
        var sourceListener = _decorator.Decorate( listener, subscriber );
        return _base.Listen( sourceListener, subscriber );
    }
}