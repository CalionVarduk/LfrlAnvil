namespace LfrlAnvil.Reactive;

public abstract class DecoratedEventListener<TSourceEvent, TNextEvent> : EventListener<TSourceEvent>
{
    protected DecoratedEventListener(IEventListener<TNextEvent> next)
    {
        Next = next;
    }

    protected IEventListener<TNextEvent> Next { get; }

    public override void OnDispose(DisposalSource source)
    {
        Next.OnDispose( source );
    }
}
