using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators
{
    public sealed class EventListenerSelectManyDecorator<TSourceEvent, TNextEvent> : IEventListenerDecorator<TSourceEvent, TNextEvent>
    {
        private readonly Func<TSourceEvent, IEnumerable<TNextEvent>> _selector;

        public EventListenerSelectManyDecorator(Func<TSourceEvent, IEnumerable<TNextEvent>> selector)
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
            private readonly Func<TSourceEvent, IEnumerable<TNextEvent>> _selector;

            internal EventListener(IEventListener<TNextEvent> next, Func<TSourceEvent, IEnumerable<TNextEvent>> selector)
                : base( next )
            {
                _selector = selector;
            }

            public override void React(TSourceEvent @event)
            {
                foreach ( var nextEvent in _selector( @event ) )
                    Next.React( nextEvent );
            }
        }
    }
}
