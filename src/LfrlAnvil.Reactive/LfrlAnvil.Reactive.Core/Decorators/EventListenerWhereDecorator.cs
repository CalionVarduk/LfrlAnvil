using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators
{
    public sealed class EventListenerWhereDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
    {
        private readonly Func<TEvent, bool> _predicate;

        public EventListenerWhereDecorator(Func<TEvent, bool> predicate)
        {
            _predicate = predicate;
        }

        [Pure]
        public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
        {
            return new EventListener( listener, _predicate );
        }

        private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
        {
            private readonly Func<TEvent, bool> _predicate;

            internal EventListener(IEventListener<TEvent> next, Func<TEvent, bool> predicate)
                : base( next )
            {
                _predicate = predicate;
            }

            public override void React(TEvent @event)
            {
                if ( _predicate( @event ) )
                    Next.React( @event );
            }
        }
    }
}
