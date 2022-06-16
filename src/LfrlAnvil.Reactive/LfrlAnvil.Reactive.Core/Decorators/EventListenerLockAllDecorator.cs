using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Decorators
{
    public sealed class EventListenerLockAllDecorator<TEvent> : IEventListenerDecorator<IEventStream<TEvent>, IEventStream<TEvent>>
    {
        private readonly object? _sync;

        public EventListenerLockAllDecorator(object? sync)
        {
            _sync = sync;
        }

        [Pure]
        public IEventListener<IEventStream<TEvent>> Decorate(IEventListener<IEventStream<TEvent>> listener, IEventSubscriber _)
        {
            return new EventListener( listener, _sync );
        }

        private sealed class EventListener : DecoratedEventListener<IEventStream<TEvent>, IEventStream<TEvent>>
        {
            private readonly object _sync;

            public EventListener(IEventListener<IEventStream<TEvent>> next, object? sync)
                : base( next )
            {
                _sync = sync ?? new object();
            }

            public override void React(IEventStream<TEvent> @event)
            {
                Next.React( @event.Lock( _sync ) );
            }
        }
    }
}
