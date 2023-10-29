namespace LfrlAnvil.Reactive.Internal;

public class ConcurrentEventPublisher<TEvent, TPublisher> : ConcurrentEventSource<TEvent, TPublisher>, IEventPublisher<TEvent>
    where TPublisher : EventPublisher<TEvent>
{
    protected internal ConcurrentEventPublisher(TPublisher @base)
        : base( @base ) { }

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
