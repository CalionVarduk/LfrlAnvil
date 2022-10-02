using System;
using System.Collections;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.State.Events;

public class CollectionVariableValidationEvent<TKey, TElement, TValidationResult> : ICollectionVariableValidationEvent<TValidationResult>
    where TKey : notnull
    where TElement : notnull
{
    public CollectionVariableValidationEvent(
        IReadOnlyCollectionVariable<TKey, TElement, TValidationResult> variable,
        Chain<TValidationResult> previousErrors,
        Chain<TValidationResult> previousWarnings,
        VariableState previousState,
        IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> elements,
        CollectionVariableChangeEvent<TKey, TElement, TValidationResult>? associatedChange)
    {
        Variable = variable;
        PreviousErrors = previousErrors;
        NewErrors = Variable.Errors;
        PreviousWarnings = previousWarnings;
        NewWarnings = Variable.Warnings;
        PreviousState = previousState;
        NewState = Variable.State;
        Elements = elements;
        AssociatedChange = associatedChange;
    }

    public CollectionVariableChangeEvent<TKey, TElement, TValidationResult>? AssociatedChange { get; }
    public IReadOnlyCollectionVariable<TKey, TElement, TValidationResult> Variable { get; }
    public VariableState PreviousState { get; }
    public VariableState NewState { get; }
    public Chain<TValidationResult> PreviousErrors { get; }
    public Chain<TValidationResult> NewErrors { get; }
    public Chain<TValidationResult> PreviousWarnings { get; }
    public Chain<TValidationResult> NewWarnings { get; }
    public IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> Elements { get; }

    IReadOnlyList<ICollectionVariableElementValidationSnapshot<TValidationResult>> ICollectionVariableValidationEvent<TValidationResult>.
        Elements =>
        Elements;

    Type ICollectionVariableValidationEvent.KeyType => typeof( TKey );
    Type ICollectionVariableValidationEvent.ElementType => typeof( TElement );
    Type ICollectionVariableValidationEvent.ValidationResultType => typeof( TValidationResult );
    ICollectionVariableChangeEvent? ICollectionVariableValidationEvent.AssociatedChange => AssociatedChange;
    IReadOnlyCollectionVariable ICollectionVariableValidationEvent.Variable => Variable;
    IEnumerable ICollectionVariableValidationEvent.PreviousErrors => PreviousErrors;
    IEnumerable ICollectionVariableValidationEvent.NewErrors => NewErrors;
    IEnumerable ICollectionVariableValidationEvent.PreviousWarnings => PreviousWarnings;
    IEnumerable ICollectionVariableValidationEvent.NewWarnings => NewWarnings;
    IReadOnlyList<ICollectionVariableElementSnapshot> ICollectionVariableValidationEvent.Elements => Elements;
    IVariableNode IVariableNodeEvent.Variable => Variable;
}
