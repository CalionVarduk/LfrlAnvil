using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Decorators;

public sealed class EventListenerWithIndexDecorator<TEvent> : IEventListenerDecorator<TEvent, WithIndex<TEvent>>
{
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<WithIndex<TEvent>> listener, IEventSubscriber _)
    {
        return new EventListener( listener );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, WithIndex<TEvent>>
    {
        private int _index;

        internal EventListener(IEventListener<WithIndex<TEvent>> next)
            : base( next )
        {
            _index = -1;
        }

        public override void React(TEvent @event)
        {
            Next.React( new WithIndex<TEvent>( @event, ++_index ) );
        }
    }
}