using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.Events.Composites;

namespace LfrlAnvil.Reactive.Events.Decorators
{
    public sealed class EventListenerSampleWhenDecorator<TEvent, TTargetEvent> : IEventListenerDecorator<TEvent, TEvent>
    {
        private readonly IEventStream<TTargetEvent> _target;

        public EventListenerSampleWhenDecorator(IEventStream<TTargetEvent> target)
        {
            _target = target;
        }

        [Pure]
        public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
        {
            return new EventListener( listener, subscriber, _target );
        }

        private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
        {
            private readonly IEventSubscriber _subscriber;
            private readonly IEventSubscriber _targetSubscriber;
            private readonly TargetEventListener _targetListener;

            internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, IEventStream<TTargetEvent> target)
                : base( next )
            {
                _subscriber = subscriber;
                _targetListener = new TargetEventListener( this );
                _targetSubscriber = target.Listen( _targetListener );
            }

            public override void React(TEvent @event)
            {
                _targetListener.UpdateSample( @event );
            }

            public override void OnDispose(DisposalSource source)
            {
                _targetSubscriber.Dispose();
                base.OnDispose( source );
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal void OnTargetEvent(Optional<TEvent> sample)
            {
                sample.TryForward( Next );
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal void DisposeSubscriber()
            {
                _subscriber.Dispose();
            }
        }

        private sealed class TargetEventListener : EventListener<TTargetEvent>
        {
            private Optional<TEvent> _sample;
            private EventListener? _sourceListener;

            internal TargetEventListener(EventListener sourceListener)
            {
                _sample = Optional<TEvent>.Empty;
                _sourceListener = sourceListener;
            }

            public override void React(TTargetEvent _)
            {
                _sourceListener!.OnTargetEvent( _sample );
                _sample = Optional<TEvent>.Empty;
            }

            public override void OnDispose(DisposalSource _)
            {
                _sample = Optional<TEvent>.Empty;
                _sourceListener!.DisposeSubscriber();
                _sourceListener = null;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal void UpdateSample(TEvent @event)
            {
                _sample = new Optional<TEvent>( @event );
            }
        }
    }
}
