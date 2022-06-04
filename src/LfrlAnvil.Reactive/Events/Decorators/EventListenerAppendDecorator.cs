using System.Collections.Generic;
using System.Linq;

namespace LfrlAnvil.Reactive.Events.Decorators
{
    public sealed class EventListenerAppendDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
    {
        private readonly TEvent[] _values;

        public EventListenerAppendDecorator(IEnumerable<TEvent> values)
        {
            _values = values.ToArray();
        }

        public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
        {
            return _values.Length == 0 ? listener : new EventListener( listener, _values );
        }

        private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
        {
            private TEvent[]? _values;

            internal EventListener(IEventListener<TEvent> next, TEvent[] values)
                : base( next )
            {
                _values = values;
            }

            public override void React(TEvent @event)
            {
                Next.React( @event );
            }

            public override void OnDispose(DisposalSource source)
            {
                foreach ( var value in _values! )
                    Next.React( value );

                _values = null;

                base.OnDispose( source );
            }
        }
    }
}
