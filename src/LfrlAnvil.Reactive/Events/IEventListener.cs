namespace LfrlAnvil.Reactive.Events
{
    public interface IEventListener
    {
        void React(object? @event);
        void OnDispose();
    }

    public interface IEventListener<in TEvent> : IEventListener
    {
        void React(TEvent @event);
    }
}
