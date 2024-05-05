using System;

namespace LfrlAnvil.Reactive.State.Events;

/// <inheritdoc />
public class VariableRootChangeEvent<TKey> : IVariableRootEvent<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Creates a new <see cref="VariableRootChangeEvent{TKey}"/> instance.
    /// </summary>
    /// <param name="root">Variable node that emitted this event.</param>
    /// <param name="nodeKey">Key of the child node that caused this event.</param>
    /// <param name="sourceEvent">Source child node event.</param>
    /// <param name="previousState">Previous state of the <see cref="Variable"/>.</param>
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

    /// <inheritdoc />
    public IReadOnlyVariableRoot<TKey> Variable { get; }

    /// <inheritdoc />
    public TKey NodeKey { get; }

    /// <inheritdoc />
    public IVariableNodeEvent SourceEvent { get; }

    /// <inheritdoc />
    public VariableState PreviousState { get; }

    /// <inheritdoc />
    public VariableState NewState { get; }

    Type IVariableRootEvent.KeyType => typeof( TKey );
    IReadOnlyVariableRoot IVariableRootEvent.Variable => Variable;
    object IVariableRootEvent.NodeKey => NodeKey;
    IVariableNode IVariableNodeEvent.Variable => Variable;
}
