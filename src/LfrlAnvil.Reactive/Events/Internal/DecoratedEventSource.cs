using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Events.Internal
{
    internal sealed class DecoratedEventSource<TRootEvent, TEvent> : DecoratedEventSourceBase<TRootEvent, TEvent>
    {
        private readonly IEventListenerDecorator<TRootEvent, TEvent> _decorator;

        internal DecoratedEventSource(EventSource<TRootEvent> root, IEventListenerDecorator<TRootEvent, TEvent> decorator)
            : base( root )
        {
            _decorator = decorator;
        }

        [Pure]
        public override IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator)
        {
            return new DecoratedEventSource<TRootEvent, TEvent, TNextEvent>( Root, this, decorator );
        }

        internal override IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TRootEvent> subscriber)
        {
            var sourceListener = _decorator.Decorate( listener, subscriber );
            return Root.Listen( sourceListener, subscriber );
        }
    }

    internal sealed class DecoratedEventSource<TRootEvent, TSourceEvent, TEvent> : DecoratedEventSourceBase<TRootEvent, TEvent>
    {
        private readonly DecoratedEventSourceBase<TRootEvent, TSourceEvent> _base;
        private readonly IEventListenerDecorator<TSourceEvent, TEvent> _decorator;

        internal DecoratedEventSource(
            EventSource<TRootEvent> root,
            DecoratedEventSourceBase<TRootEvent, TSourceEvent> @base,
            IEventListenerDecorator<TSourceEvent, TEvent> decorator)
            : base( root )
        {
            _base = @base;
            _decorator = decorator;
        }

        [Pure]
        public override IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator)
        {
            return new DecoratedEventSource<TRootEvent, TEvent, TNextEvent>( Root, this, decorator );
        }

        internal override IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TRootEvent> subscriber)
        {
            var sourceListener = _decorator.Decorate( listener, subscriber );
            return _base.Listen( sourceListener, subscriber );
        }
    }
}
