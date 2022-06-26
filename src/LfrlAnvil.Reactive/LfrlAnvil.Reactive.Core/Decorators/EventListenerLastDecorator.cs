using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Decorators;

public class EventListenerLastDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private Optional<TEvent> _value;

        internal EventListener(IEventListener<TEvent> next)
            : base( next )
        {
            _value = Optional<TEvent>.Empty;
        }

        public override void React(TEvent @event)
        {
            _value = new Optional<TEvent>( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _value.TryForward( Next );
            _value = Optional<TEvent>.Empty;

            base.OnDispose( source );
        }
    }
}
