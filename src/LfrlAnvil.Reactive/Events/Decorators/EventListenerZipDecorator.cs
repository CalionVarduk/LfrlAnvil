using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Events.Decorators
{
    public sealed class EventListenerZipDecorator<TEvent, TTargetEvent, TNextEvent> : IEventListenerDecorator<TEvent, TNextEvent>
    {
        private readonly IEventStream<TTargetEvent> _target;
        private readonly Func<TEvent, TTargetEvent, TNextEvent> _selector;

        public EventListenerZipDecorator(IEventStream<TTargetEvent> target, Func<TEvent, TTargetEvent, TNextEvent> selector)
        {
            _target = target;
            _selector = selector;
        }

        [Pure]
        public IEventListener<TEvent> Decorate(IEventListener<TNextEvent> listener, IEventSubscriber subscriber)
        {
            return new EventListener( listener, subscriber, _target, _selector );
        }

        private sealed class EventListener : DecoratedEventListener<TEvent, TNextEvent>
        {
            private readonly Queue<TEvent> _sourceEvents;
            private readonly Queue<TTargetEvent> _targetEvents;
            private readonly IEventSubscriber _subscriber;
            private readonly IEventSubscriber _targetSubscriber;
            private readonly Func<TEvent, TTargetEvent, TNextEvent> _selector;

            internal EventListener(
                IEventListener<TNextEvent> next,
                IEventSubscriber subscriber,
                IEventStream<TTargetEvent> target,
                Func<TEvent, TTargetEvent, TNextEvent> selector)
                : base( next )
            {
                _sourceEvents = new Queue<TEvent>();
                _targetEvents = new Queue<TTargetEvent>();
                _subscriber = subscriber;
                _selector = selector;

                _targetSubscriber = target.Listen(
                    Events.EventListener.Create<TTargetEvent>(
                        e =>
                        {
                            if ( _sourceEvents.TryDequeue( out var sourceEvent ) )
                            {
                                Next.React( _selector( sourceEvent, e ) );
                                return;
                            }

                            _targetEvents.Enqueue( e );
                        },
                        _ => _subscriber.Dispose() ) );
            }

            public override void React(TEvent @event)
            {
                if ( _targetEvents.TryDequeue( out var targetEvent ) )
                {
                    Next.React( _selector( @event, targetEvent ) );
                    return;
                }

                _sourceEvents.Enqueue( @event );
            }

            public override void OnDispose(DisposalSource source)
            {
                _targetSubscriber.Dispose();
                _sourceEvents.Clear();
                _sourceEvents.TrimExcess();
                _targetEvents.Clear();
                _targetEvents.TrimExcess();

                base.OnDispose( source );
            }
        }
    }
}
