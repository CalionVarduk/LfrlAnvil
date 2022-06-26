namespace LfrlAnvil.Reactive;

public interface IEventListener
{
    void React(object? @event);
    void OnDispose(DisposalSource source);
}

public interface IEventListener<in TEvent> : IEventListener
{
    void React(TEvent @event);
}