using System;
using System.Collections;
using LfrlAnvil.Reactive.State.Internal;

namespace LfrlAnvil.Reactive.State.Events;

public class CollectionVariableRootValidationEvent<TKey, TElement, TValidationResult>
    : ICollectionVariableRootValidationEvent<TValidationResult>
    where TKey : notnull
    where TElement : VariableNode
{
    public CollectionVariableRootValidationEvent(
        IReadOnlyCollectionVariableRoot<TKey, TElement, TValidationResult> variable,
        VariableState previousState,
        Chain<TValidationResult> previousErrors,
        Chain<TValidationResult> previousWarnings,
        CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult>? associatedChange,
        IVariableNodeEvent? sourceEvent)
    {
        Variable = variable;
        PreviousState = previousState;
        NewState = Variable.State;
        PreviousErrors = previousErrors;
        NewErrors = Variable.Errors;
        PreviousWarnings = previousWarnings;
        NewWarnings = Variable.Warnings;
        AssociatedChange = associatedChange;
        SourceEvent = sourceEvent;
    }

    public CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult>? AssociatedChange { get; }
    public IReadOnlyCollectionVariableRoot<TKey, TElement, TValidationResult> Variable { get; }
    public VariableState PreviousState { get; }
    public VariableState NewState { get; }
    public Chain<TValidationResult> PreviousErrors { get; }
    public Chain<TValidationResult> NewErrors { get; }
    public Chain<TValidationResult> PreviousWarnings { get; }
    public Chain<TValidationResult> NewWarnings { get; }
    public IVariableNodeEvent? SourceEvent { get; }

    Type ICollectionVariableRootValidationEvent.KeyType => typeof( TKey );
    Type ICollectionVariableRootValidationEvent.ElementType => typeof( TElement );
    Type ICollectionVariableRootValidationEvent.ValidationResultType => typeof( TValidationResult );
    IReadOnlyCollectionVariableRoot ICollectionVariableRootValidationEvent.Variable => Variable;
    ICollectionVariableRootChangeEvent? ICollectionVariableRootValidationEvent.AssociatedChange => AssociatedChange;
    IEnumerable ICollectionVariableRootValidationEvent.PreviousErrors => PreviousErrors;
    IEnumerable ICollectionVariableRootValidationEvent.NewErrors => NewErrors;
    IEnumerable ICollectionVariableRootValidationEvent.PreviousWarnings => PreviousWarnings;
    IEnumerable ICollectionVariableRootValidationEvent.NewWarnings => NewWarnings;

    IVariableNode IVariableNodeEvent.Variable => Variable;
}
