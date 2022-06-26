using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Decorators;

public sealed class EventListenerSkipUntilDecorator<TEvent, TTargetEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly IEventStream<TTargetEvent> _target;

    public EventListenerSkipUntilDecorator(IEventStream<TTargetEvent> target)
    {
        _target = target;
    }

    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _target );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly LazyDisposable<IEventSubscriber> _targetSubscriber;

        internal EventListener(IEventListener<TEvent> next, IEventStream<TTargetEvent> target)
            : base( next )
        {
            _targetSubscriber = new LazyDisposable<IEventSubscriber>();
            var targetListener = new TargetEventListener( this );
            var actualTargetSubscriber = target.Listen( targetListener );
            _targetSubscriber.Assign( actualTargetSubscriber );
        }

        public override void React(TEvent @event)
        {
            if ( _targetSubscriber.IsDisposed )
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
            _targetSubscriber.Dispose();
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
            _sourceListener = null;
        }
    }
}