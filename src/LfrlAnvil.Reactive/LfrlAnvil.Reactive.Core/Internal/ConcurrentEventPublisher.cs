namespace LfrlAnvil.Reactive.Internal;

/// <summary>
/// Represents a concurrent version of an <see cref="EventPublisher{TEvent}"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TPublisher">Underlying event publisher type.</typeparam>
public class ConcurrentEventPublisher<TEvent, TPublisher> : ConcurrentEventSource<TEvent, TPublisher>, IEventPublisher<TEvent>
    where TPublisher : EventPublisher<TEvent>
{
    /// <summary>
    /// Creates a new <see cref="ConcurrentEventPublisher{TEvent,TPublisher}"/> instance.
    /// </summary>
    /// <param name="base">Underlying event publisher.</param>
    protected internal ConcurrentEventPublisher(TPublisher @base)
        : base( @base ) { }

    /// <inheritdoc />
    public void Publish(TEvent @event)
    {
        lock ( Sync )
        {
            Base.Publish( @event );
        }
    }

    void IEventPublisher.Publish(object? @event)
    {
        Publish( Argument.CastTo<TEvent>( @event ) );
    }
}
