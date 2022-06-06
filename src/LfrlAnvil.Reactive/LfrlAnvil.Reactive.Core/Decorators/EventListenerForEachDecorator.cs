using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators
{
    public sealed class EventListenerForEachDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
    {
        private readonly Action<TEvent> _action;

        public EventListenerForEachDecorator(Action<TEvent> action)
        {
            _action = action;
        }

        [Pure]
        public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
        {
            return new EventListener( listener, _action );
        }

        private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
        {
            private readonly Action<TEvent> _action;

            internal EventListener(IEventListener<TEvent> next, Action<TEvent> action)
                : base( next )
            {
                _action = action;
            }

            public override void React(TEvent @event)
            {
                _action( @event );
                Next.React( @event );
            }
        }
    }
}
