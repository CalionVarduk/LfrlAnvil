using System;

namespace LfrlAnvil.Reactive.State.Events;

public class VariableRootChangeEvent<TKey> : IVariableRootEvent<TKey>
    where TKey : notnull
{
    public VariableRootChangeEvent(
        IReadOnlyVariableRoot<TKey> root,
        TKey nodeKey,
        IVariableNodeEvent sourceEvent,
        VariableState previousState)
    {
        Variable = root;
        NodeKey = nodeKey;
        SourceEvent = sourceEvent;
        PreviousState = previousState;
        NewState = Variable.State;
    }

    public IReadOnlyVariableRoot<TKey> Variable { get; }
    public TKey NodeKey { get; }
    public IVariableNodeEvent SourceEvent { get; }
    public VariableState PreviousState { get; }
    public VariableState NewState { get; }

    Type IVariableRootEvent.KeyType => typeof( TKey );
    IReadOnlyVariableRoot IVariableRootEvent.Variable => Variable;
    object IVariableRootEvent.NodeKey => NodeKey;
    IVariableNode IVariableNodeEvent.Variable => Variable;
}
