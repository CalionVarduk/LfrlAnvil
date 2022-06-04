using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Events.Decorators
{
    public sealed class EventListenerExhaustAllDecorator<TEvent> : IEventListenerDecorator<IEventStream<TEvent>, TEvent>
    {
        public IEventListener<IEventStream<TEvent>> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
        {
            return new EventListener( listener );
        }

        private sealed class EventListener : DecoratedEventListener<IEventStream<TEvent>, TEvent>
        {
            private IEventSubscriber? _activeInnerSubscriber;

            internal EventListener(IEventListener<TEvent> next)
                : base( next )
            {
                _activeInnerSubscriber = null;
            }

            public override void React(IEventStream<TEvent> @event)
            {
                if ( _activeInnerSubscriber is not null )
                    return;

                var activeInnerListener = new InnerEventListener( this );
                _activeInnerSubscriber = @event.Listen( activeInnerListener );

                if ( activeInnerListener.IsMarkedAsDisposed() )
                    _activeInnerSubscriber = null;
            }

            public override void OnDispose(DisposalSource source)
            {
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
                _outerListener!.OnInnerEvent( @event );
            }

            public override void OnDispose(DisposalSource _)
            {
                _outerListener!.OnInnerDisposed();
                _outerListener = null;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal bool IsMarkedAsDisposed()
            {
                return _outerListener is null;
            }
        }
    }
}
