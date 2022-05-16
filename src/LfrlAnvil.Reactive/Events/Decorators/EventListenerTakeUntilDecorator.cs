namespace LfrlAnvil.Reactive.Events.Decorators
{
    public sealed class EventListenerTakeUntilDecorator<TEvent, TTargetEvent> : IEventListenerDecorator<TEvent, TEvent>
    {
        private readonly IEventStream<TTargetEvent> _target;

        public EventListenerTakeUntilDecorator(IEventStream<TTargetEvent> target)
        {
            _target = target;
        }

        public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
        {
            return new EventListener( listener, subscriber, _target );
        }

        private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
        {
            private readonly IEventSubscriber _subscriber;
            private readonly IEventSubscriber _targetSubscriber;

            internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, IEventStream<TTargetEvent> target)
                : base( next )
            {
                _subscriber = subscriber;
                _targetSubscriber = target.Listen(
                    Events.EventListener.Create<TTargetEvent>(
                        _ => _subscriber.Dispose(),
                        _ => _subscriber.Dispose() ) );
            }

            public override void React(TEvent @event)
            {
                Next.React( @event );
            }

            public override void OnDispose(DisposalSource source)
            {
                _targetSubscriber.Dispose();
                base.OnDispose( source );
            }
        }
    }
}
