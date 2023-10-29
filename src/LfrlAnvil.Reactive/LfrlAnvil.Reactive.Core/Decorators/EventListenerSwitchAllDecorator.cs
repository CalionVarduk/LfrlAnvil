using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Decorators;

public sealed class EventListenerSwitchAllDecorator<TEvent> : IEventListenerDecorator<IEventStream<TEvent>, TEvent>
{
    public IEventListener<IEventStream<TEvent>> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener );
    }

    private sealed class EventListener : DecoratedEventListener<IEventStream<TEvent>, TEvent>
    {
        private IEventSubscriber? _activeInnerSubscriber;
        private IEventStream<TEvent>? _nextStream;

        internal EventListener(IEventListener<TEvent> next)
            : base( next )
        {
            _activeInnerSubscriber = null;
            _nextStream = null;
        }

        public override void React(IEventStream<TEvent> @event)
        {
            if ( _activeInnerSubscriber is null )
            {
                StartListeningToNextInnerStream( @event );
                return;
            }

            _nextStream = @event;
            _activeInnerSubscriber.Dispose();
        }

        public override void OnDispose(DisposalSource source)
        {
            _nextStream = null;
            _activeInnerSubscriber?.Dispose();

            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnInnerEvent(TEvent @event)
        {
            Next.React( @event );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnInnerDisposed()
        {
            _activeInnerSubscriber = null;

            if ( _nextStream is null )
                return;

            var nextStream = _nextStream;
            _nextStream = null;
            StartListeningToNextInnerStream( nextStream );
        }

        private void StartListeningToNextInnerStream(IEventStream<TEvent> stream)
        {
            var activeInnerListener = new InnerEventListener( this );
            _activeInnerSubscriber = stream.Listen( activeInnerListener );

            if ( activeInnerListener.IsMarkedAsDisposed() )
                _activeInnerSubscriber = null;
        }
    }

    private sealed class InnerEventListener : EventListener<TEvent>
    {
        private EventListener? _outerListener;

        internal InnerEventListener(EventListener outerListener)
        {
            _outerListener = outerListener;
        }

        public override void React(TEvent @event)
        {
            Assume.IsNotNull( _outerListener );
            _outerListener.OnInnerEvent( @event );
        }

        public override void OnDispose(DisposalSource _)
        {
            Assume.IsNotNull( _outerListener );
            _outerListener.OnInnerDisposed();
            _outerListener = null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool IsMarkedAsDisposed()
        {
            return _outerListener is null;
        }
    }
}
