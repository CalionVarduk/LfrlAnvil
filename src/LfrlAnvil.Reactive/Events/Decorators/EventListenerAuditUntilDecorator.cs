using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Events.Decorators
{
    public sealed class EventListenerAuditUntilDecorator<TEvent, TTargetEvent> : IEventListenerDecorator<TEvent, TEvent>
    {
        private readonly IEventStream<TTargetEvent> _target;

        public EventListenerAuditUntilDecorator(IEventStream<TTargetEvent> target)
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
            private LazyEventSubscriber? _targetSubscriber;
            private TargetEventListener? _targetListener;
            private IEventStream<TTargetEvent>? _target;

            internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, IEventStream<TTargetEvent> target)
                : base( next )
            {
                _subscriber = subscriber;
                _target = target;
                _targetSubscriber = null;
                _targetListener = null;

                if ( target.IsDisposed )
                    DisposeSubscriber();
            }

            public override void React(TEvent @event)
            {
                if ( _targetListener is not null )
                {
                    _targetListener.UpdateEvent( @event );
                    return;
                }

                _targetSubscriber = new LazyEventSubscriber();
                _targetListener = new TargetEventListener( this, @event );
                var actualTargetSubscriber = _target!.Listen( _targetListener );
                _targetSubscriber?.Initialize( actualTargetSubscriber );
            }

            public override void OnDispose(DisposalSource source)
            {
                _target = null;
                _targetSubscriber?.Dispose();

                base.OnDispose( source );
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal void OnTargetEvent(TEvent @event)
            {
                Next.React( @event );
                _targetSubscriber!.Dispose();
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal void ClearTargetReferences()
            {
                _targetSubscriber = null;
                _targetListener = null;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal void DisposeSubscriber()
            {
                _subscriber.Dispose();
            }
        }

        private sealed class TargetEventListener : EventListener<TTargetEvent>
        {
            private EventListener? _sourceListener;
            private TEvent? _sourceEvent;

            internal TargetEventListener(EventListener sourceListener, TEvent sourceEvent)
            {
                _sourceListener = sourceListener;
                _sourceEvent = sourceEvent;
            }

            public override void React(TTargetEvent _)
            {
                _sourceListener!.OnTargetEvent( _sourceEvent! );
            }

            public override void OnDispose(DisposalSource source)
            {
                _sourceEvent = default;
                _sourceListener!.ClearTargetReferences();

                if ( source == DisposalSource.EventSource )
                    _sourceListener!.DisposeSubscriber();

                _sourceListener = null;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal void UpdateEvent(TEvent @event)
            {
                _sourceEvent = @event;
            }
        }
    }
}
