using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators
{
    public class EventListenerSkipDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
    {
        private readonly int _count;

        public EventListenerSkipDecorator(int count)
        {
            _count = count;
        }

        [Pure]
        public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
        {
            return _count <= 0 ? listener : new EventListener( listener, _count );
        }

        private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
        {
            private readonly int _count;
            private int _skipped;

            internal EventListener(IEventListener<TEvent> next, int count)
                : base( next )
            {
                _count = count;
                _skipped = 0;
            }

            public override void React(TEvent @event)
            {
                if ( _skipped == _count )
                {
                    Next.React( @event );
                    return;
                }

                ++_skipped;
            }
        }
    }
}
