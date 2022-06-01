using System;
using LfrlAnvil.Reactive.Events.Internal;
using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Events
{
    public class EventPublisher<TEvent> : EventSource<TEvent>, IEventPublisher<TEvent>
    {
        public void Publish(TEvent @event)
        {
            if ( IsDisposed )
                throw new ObjectDisposedException( Resources.DisposedEventSource );

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
