using System;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive
{
    public class EventPublisher<TEvent> : EventSource<TEvent>, IEventPublisher<TEvent>
    {
        public void Publish(TEvent @event)
        {
            if ( IsDisposed )
                throw new ObjectDisposedException( ExceptionResources.DisposedEventSource );

            OnPublish( @event );
        }

        protected virtual void OnPublish(TEvent @event)
        {
            NotifyListeners( @event );
        }

        void IEventPublisher.Publish(object? @event)
        {
            Publish( Argument.CastTo<TEvent>( @event, nameof( @event ) ) );
        }
    }
}
