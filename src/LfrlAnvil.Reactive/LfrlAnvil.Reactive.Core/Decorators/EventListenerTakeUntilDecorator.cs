using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Decorators
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
                var targetListener = new TargetEventListener( this );
                _targetSubscriber = target.Listen( targetListener );
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

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal void OnTargetEvent()
            {
                _subscriber.Dispose();
            }
        }

        private sealed class TargetEventListener : EventListener<TTargetEvent>
        {
            private EventListener? _sourceListener;

            internal TargetEventListener(EventListener sourceListener)
            {
                _sourceListener = sourceListener;
            }

            public override void React(TTargetEvent _)
            {
                _sourceListener!.OnTargetEvent();
            }

            public override void OnDispose(DisposalSource _)
            {
                _sourceListener!.OnTargetEvent();
                _sourceListener = null;
            }
        }
    }
}
