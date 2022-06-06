using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators
{
    public sealed class EventListenerDefaultIfEmptyDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
    {
        private readonly TEvent _defaultValue;

        public EventListenerDefaultIfEmptyDecorator(TEvent defaultValue)
        {
            _defaultValue = defaultValue;
        }

        [Pure]
        public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
        {
            return new EventListener( listener, _defaultValue );
        }

        private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
        {
            private bool _shouldEmitDefault;
            private TEvent? _defaultValue;

            internal EventListener(IEventListener<TEvent> next, TEvent defaultValue)
                : base( next )
            {
                _shouldEmitDefault = true;
                _defaultValue = defaultValue;
            }

            public override void React(TEvent @event)
            {
                _shouldEmitDefault = false;
                Next.React( @event );
            }

            public override void OnDispose(DisposalSource source)
            {
                if ( _shouldEmitDefault )
                    Next.React( _defaultValue! );

                _defaultValue = default;
                base.OnDispose( source );
            }
        }
    }
}
