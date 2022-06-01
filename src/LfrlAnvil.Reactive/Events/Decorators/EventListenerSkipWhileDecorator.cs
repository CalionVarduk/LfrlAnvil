using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Events.Decorators
{
    public class EventListenerSkipWhileDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
    {
        private readonly Func<TEvent, bool> _predicate;

        public EventListenerSkipWhileDecorator(Func<TEvent, bool> predicate)
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
            private bool _isDone;

            internal EventListener(IEventListener<TEvent> next, Func<TEvent, bool> predicate)
                : base( next )
            {
                _isDone = false;
                _predicate = predicate;
            }

            public override void React(TEvent @event)
            {
                if ( _isDone )
                {
                    Next.React( @event );
                    return;
                }

                if ( _predicate( @event ) )
                    return;

                _isDone = true;
                Next.React( @event );
            }
        }
    }
}
