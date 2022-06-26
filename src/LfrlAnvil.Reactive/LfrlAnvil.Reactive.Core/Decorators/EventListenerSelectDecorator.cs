using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators;

public sealed class EventListenerSelectDecorator<TSourceEvent, TNextEvent> : IEventListenerDecorator<TSourceEvent, TNextEvent>
{
    private readonly Func<TSourceEvent, TNextEvent> _selector;

    public EventListenerSelectDecorator(Func<TSourceEvent, TNextEvent> selector)
    {
        _selector = selector;
    }

    [Pure]
    public IEventListener<TSourceEvent> Decorate(IEventListener<TNextEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _selector );
    }

    private sealed class EventListener : DecoratedEventListener<TSourceEvent, TNextEvent>
    {
        private readonly Func<TSourceEvent, TNextEvent> _selector;

        internal EventListener(IEventListener<TNextEvent> next, Func<TSourceEvent, TNextEvent> selector)
            : base( next )
        {
            _selector = selector;
        }

        public override void React(TSourceEvent @event)
        {
            Next.React( _selector( @event ) );
        }
    }
}
