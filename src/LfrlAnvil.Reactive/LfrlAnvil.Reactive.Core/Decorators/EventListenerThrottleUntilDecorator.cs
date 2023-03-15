using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Decorators;

public sealed class EventListenerThrottleUntilDecorator<TEvent, TTargetEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly IEventStream<TTargetEvent> _target;

    public EventListenerThrottleUntilDecorator(IEventStream<TTargetEvent> target)
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
        private LazyDisposable<IEventSubscriber>? _targetSubscriber;
        private IEventStream<TTargetEvent>? _target;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, IEventStream<TTargetEvent> target)
            : base( next )
        {
            _subscriber = subscriber;
            _target = target;
            _targetSubscriber = null;

            if ( target.IsDisposed )
                DisposeSubscriber();
        }

        public override void React(TEvent @event)
        {
            if ( _targetSubscriber is not null )
                return;

            Assume.IsNotNull( _target, nameof( _target ) );
            _targetSubscriber = new LazyDisposable<IEventSubscriber>();
            var targetListener = new TargetEventListener( this, @event );
            var actualTargetSubscriber = _target.Listen( targetListener );
            _targetSubscriber?.Assign( actualTargetSubscriber );
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
            Assume.IsNotNull( _targetSubscriber, nameof( _targetSubscriber ) );
            Next.React( @event );
            _targetSubscriber.Dispose();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ClearTargetReferences()
        {
            _targetSubscriber = null;
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
            Assume.IsNotNull( _sourceListener, nameof( _sourceListener ) );
            _sourceListener.OnTargetEvent( _sourceEvent! );
        }

        public override void OnDispose(DisposalSource source)
        {
            Assume.IsNotNull( _sourceListener, nameof( _sourceListener ) );
            _sourceEvent = default;
            _sourceListener.ClearTargetReferences();

            if ( source == DisposalSource.EventSource )
                _sourceListener.DisposeSubscriber();

            _sourceListener = null;
        }
    }
}
