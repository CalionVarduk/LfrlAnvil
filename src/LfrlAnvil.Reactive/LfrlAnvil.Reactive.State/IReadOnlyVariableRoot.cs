using LfrlAnvil.Reactive.State.Events;

namespace LfrlAnvil.Reactive.State;

public interface IReadOnlyVariableRoot : IVariableNode
{
    IVariableNodeCollection Nodes { get; }
    new IEventStream<IVariableRootEvent> OnChange { get; }
    new IEventStream<IVariableRootEvent> OnValidate { get; }
}

public interface IReadOnlyVariableRoot<TKey> : IReadOnlyVariableRoot
    where TKey : notnull
{
    new IVariableNodeCollection<TKey> Nodes { get; }
    new IEventStream<IVariableRootEvent<TKey>> OnChange { get; }
    new IEventStream<IVariableRootEvent<TKey>> OnValidate { get; }
}
