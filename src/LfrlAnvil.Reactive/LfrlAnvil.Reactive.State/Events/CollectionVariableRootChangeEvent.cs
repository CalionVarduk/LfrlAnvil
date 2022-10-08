using System;
using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Internal;

namespace LfrlAnvil.Reactive.State.Events;

public class CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult> : ICollectionVariableRootChangeEvent<TKey, TElement>
    where TKey : notnull
    where TElement : VariableNode
{
    public CollectionVariableRootChangeEvent(
        IReadOnlyCollectionVariableRoot<TKey, TElement, TValidationResult> variable,
        VariableState previousState,
        IReadOnlyList<TElement> addedElements,
        IReadOnlyList<TElement> removedElements,
        IReadOnlyList<TElement> restoredElements,
        VariableChangeSource source)
    {
        Variable = variable;
        PreviousState = previousState;
        NewState = Variable.State;
        AddedElements = addedElements;
        RemovedElements = removedElements;
        RestoredElements = restoredElements;
        Source = source;
        SourceEvent = null;
    }

    public CollectionVariableRootChangeEvent(
        IReadOnlyCollectionVariableRoot<TKey, TElement, TValidationResult> variable,
        VariableState previousState,
        IReadOnlyList<TElement> addedElements,
        IReadOnlyList<TElement> removedElements,
        IReadOnlyList<TElement> restoredElements,
        IVariableNodeEvent sourceEvent)
    {
        Variable = variable;
        PreviousState = previousState;
        NewState = Variable.State;
        AddedElements = addedElements;
        RemovedElements = removedElements;
        RestoredElements = restoredElements;
        Source = VariableChangeSource.ChildNode;
        SourceEvent = sourceEvent;
    }

    public IReadOnlyCollectionVariableRoot<TKey, TElement, TValidationResult> Variable { get; }
    public VariableState PreviousState { get; }
    public VariableState NewState { get; }
    public IReadOnlyList<TElement> AddedElements { get; }
    public IReadOnlyList<TElement> RemovedElements { get; }
    public IReadOnlyList<TElement> RestoredElements { get; }
    public VariableChangeSource Source { get; }
    public IVariableNodeEvent? SourceEvent { get; }

    IReadOnlyCollectionVariableRoot<TKey, TElement> ICollectionVariableRootChangeEvent<TKey, TElement>.Variable => Variable;

    Type ICollectionVariableRootChangeEvent.KeyType => typeof( TKey );
    Type ICollectionVariableRootChangeEvent.ElementType => typeof( TElement );
    Type ICollectionVariableRootChangeEvent.ValidationResultType => typeof( TValidationResult );
    IReadOnlyCollectionVariableRoot ICollectionVariableRootChangeEvent.Variable => Variable;
    IReadOnlyList<IVariableNode> ICollectionVariableRootChangeEvent.AddedElements => AddedElements;
    IReadOnlyList<IVariableNode> ICollectionVariableRootChangeEvent.RemovedElements => RemovedElements;
    IReadOnlyList<IVariableNode> ICollectionVariableRootChangeEvent.RestoredElements => RestoredElements;

    IVariableNode IVariableNodeEvent.Variable => Variable;
}
