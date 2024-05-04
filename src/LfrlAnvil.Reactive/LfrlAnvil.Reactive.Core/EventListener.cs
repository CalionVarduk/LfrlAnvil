using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive;

/// <inheritdoc />
public abstract class EventListener<TEvent> : IEventListener<TEvent>
{
    /// <summary>
    /// Represents an event listener that does nothing.
    /// </summary>
    public static readonly IEventListener<TEvent> Empty = EventListener.Create<TEvent>( static _ => { } );

    /// <inheritdoc />
    public abstract void React(TEvent @event);

    /// <inheritdoc />
    public abstract void OnDispose(DisposalSource source);

    void IEventListener.React(object? @event)
    {
        React( Argument.CastTo<TEvent>( @event ) );
    }
}
