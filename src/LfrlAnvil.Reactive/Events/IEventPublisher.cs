namespace LfrlAnvil.Reactive.Events
{
    public interface IEventPublisher : IEventSource
    {
        void Publish(object? @event);
    }

    public interface IEventPublisher<TEvent> : IEventSource<TEvent>, IEventPublisher
    {
        void Publish(TEvent @event);
    }
}
