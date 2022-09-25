using LfrlAnvil.Reactive.State.Events;

namespace LfrlAnvil.Reactive.State;

public interface IVariableRoot : IVariableNode
{
    IVariableNodeCollection Nodes { get; }
    new IEventStream<IVariableRootEvent> OnChange { get; }
    new IEventStream<IVariableRootEvent> OnValidate { get; }
}

public interface IVariableRoot<TKey> : IVariableRoot
    where TKey : notnull
{
    new IVariableNodeCollection<TKey> Nodes { get; }
    new IEventStream<IVariableRootEvent<TKey>> OnChange { get; }
    new IEventStream<IVariableRootEvent<TKey>> OnValidate { get; }
}
