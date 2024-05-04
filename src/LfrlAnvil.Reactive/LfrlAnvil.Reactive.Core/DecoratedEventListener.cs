namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents a generic decorated event listener.
/// </summary>
/// <typeparam name="TSourceEvent">Source event type.</typeparam>
/// <typeparam name="TNextEvent">Next event type.</typeparam>
public abstract class DecoratedEventListener<TSourceEvent, TNextEvent> : EventListener<TSourceEvent>
{
    /// <summary>
    /// Creates a new <see cref="DecoratedEventListener{TSourceEvent,TNextEvent}"/> instance.
    /// </summary>
    /// <param name="next">Decorating event listener.</param>
    protected DecoratedEventListener(IEventListener<TNextEvent> next)
    {
        Next = next;
    }

    /// <summary>
    /// Decorating event listener.
    /// </summary>
    protected IEventListener<TNextEvent> Next { get; }

    /// <inheritdoc />
    public override void OnDispose(DisposalSource source)
    {
        Next.OnDispose( source );
    }
}
