using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive;

public class EventPublisher<TEvent> : EventSource<TEvent>, IEventPublisher<TEvent>
{
    public void Publish(TEvent @event)
    {
        EnsureNotDisposed();
        OnPublish( @event );
    }

    protected virtual void OnPublish(TEvent @event)
    {
        NotifyListeners( @event );
    }

    void IEventPublisher.Publish(object? @event)
    {
        Publish( Argument.CastTo<TEvent>( @event ) );
    }
}
