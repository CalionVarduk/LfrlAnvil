namespace LfrlAnvil.Reactive.State.Events;

public class VariableRootValidationEvent<TKey> : IVariableRootEvent<TKey>
    where TKey : notnull
{
    public VariableRootValidationEvent(
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

    IReadOnlyVariableRoot IVariableRootEvent.Variable => Variable;
    object IVariableRootEvent.NodeKey => NodeKey;
    IVariableNode IVariableNodeEvent.Variable => Variable;
}
