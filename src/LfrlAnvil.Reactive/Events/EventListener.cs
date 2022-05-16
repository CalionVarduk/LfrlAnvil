using LfrlAnvil.Reactive.Events.Internal;

namespace LfrlAnvil.Reactive.Events
{
    public abstract class EventListener<TEvent> : IEventListener<TEvent>
    {
        public static readonly IEventListener<TEvent> Empty = EventListener.Create<TEvent>( _ => { } );

        public abstract void React(TEvent @event);
        public abstract void OnDispose(DisposalSource source);

        void IEventListener.React(object? @event)
        {
            React( Argument.CastTo<TEvent>( @event, nameof( @event ) ) );
        }
    }
}
