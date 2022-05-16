using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Events.Internal;

namespace LfrlAnvil.Reactive.Events
{
    public abstract class EventSource<TEvent> : IEventSource<TEvent>
    {
        public abstract bool IsDisposed { get; }
        public abstract IReadOnlyCollection<IEventSubscriber> Subscribers { get; }
        public bool HasSubscribers => Subscribers.Count > 0;

        public abstract void Dispose();
        public abstract IEventSubscriber Listen(IEventListener<TEvent> listener);

        [Pure]
        public IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator)
        {
            return new DecoratedEventSource<TEvent, TNextEvent>( this, decorator );
        }

        [Pure]
        internal abstract EventSubscriber<TEvent> CreateSubscriber();

        internal abstract void RemoveSubscriber(IEventSubscriber subscriber);
        internal abstract IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TEvent> subscriber);

        IEventSubscriber IEventStream.Listen(IEventListener listener)
        {
            return Listen( Argument.CastTo<IEventListener<TEvent>>( listener, nameof( listener ) ) );
        }
    }
}
