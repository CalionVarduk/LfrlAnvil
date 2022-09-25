namespace LfrlAnvil.Reactive.State.Events;

public interface IVariableRootEvent : IVariableNodeEvent
{
    new IVariableRoot Variable { get; }
    object NodeKey { get; }
    IVariableNodeEvent SourceEvent { get; }
}

public interface IVariableRootEvent<TKey> : IVariableRootEvent
    where TKey : notnull
{
    new IVariableRoot<TKey> Variable { get; }
    new TKey NodeKey { get; }
}
