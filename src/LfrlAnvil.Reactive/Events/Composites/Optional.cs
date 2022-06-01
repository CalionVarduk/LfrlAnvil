using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Events.Composites
{
    internal struct Optional<TEvent>
    {
        internal static readonly Optional<TEvent> Empty = new Optional<TEvent>();

        internal Optional(TEvent @event)
        {
            Event = @event;
            HasValue = true;
        }

        internal TEvent? Event;
        internal bool HasValue;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Clear()
        {
            Event = default;
            HasValue = false;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void TryForward(IEventListener<TEvent> listener)
        {
            if ( HasValue )
                listener.React( Event! );
        }
    }
}
