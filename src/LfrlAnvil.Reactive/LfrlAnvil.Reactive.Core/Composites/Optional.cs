using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Composites;

internal readonly struct Optional<TEvent>
{
    internal static readonly Optional<TEvent> Empty = new Optional<TEvent>();

    internal Optional(TEvent @event)
    {
        Event = @event;
        HasValue = true;
    }

    internal readonly TEvent? Event;
    internal readonly bool HasValue;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void TryForward(IEventListener<TEvent> listener)
    {
        if ( HasValue )
            listener.React( Event! );
    }
}