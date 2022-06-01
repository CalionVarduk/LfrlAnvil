﻿using LfrlAnvil.Reactive.Events.Composites;

namespace LfrlAnvil.Reactive.Events.Decorators
{
    public class EventListenerSingleDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
    {
        public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
        {
            return new EventListener( listener, subscriber );
        }

        private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
        {
            private readonly IEventSubscriber _subscriber;
            private Optional<TEvent> _value;

            internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber)
                : base( next )
            {
                _subscriber = subscriber;
                _value = Optional<TEvent>.Empty;
            }

            public override void React(TEvent @event)
            {
                if ( _value.HasValue )
                {
                    _value.Clear();
                    _subscriber.Dispose();
                    return;
                }

                _value = new Optional<TEvent>( @event );
            }

            public override void OnDispose(DisposalSource source)
            {
                _value.TryForward( Next );
                _value.Clear();

                base.OnDispose( source );
            }
        }
    }
}
