using System;
using System.Collections;

namespace LfrlAnvil.Reactive.State.Events;

public class CollectionVariableElementSnapshot<TElement, TValidationResult>
    : ICollectionVariableElementSnapshot<TElement>, ICollectionVariableElementValidationSnapshot<TValidationResult>
    where TElement : notnull
{
    public CollectionVariableElementSnapshot(
        TElement element,
        CollectionVariableElementState previousState,
        CollectionVariableElementState newState,
        Chain<TValidationResult> previousErrors,
        Chain<TValidationResult> newErrors,
        Chain<TValidationResult> previousWarnings,
        Chain<TValidationResult> newWarnings)
    {
        Element = element;
        PreviousState = previousState;
        NewState = newState;
        PreviousErrors = previousErrors;
        NewErrors = newErrors;
        PreviousWarnings = previousWarnings;
        NewWarnings = newWarnings;
    }

    public TElement Element { get; }
    public CollectionVariableElementState PreviousState { get; }
    public CollectionVariableElementState NewState { get; }
    public Chain<TValidationResult> PreviousErrors { get; }
    public Chain<TValidationResult> NewErrors { get; }
    public Chain<TValidationResult> PreviousWarnings { get; }
    public Chain<TValidationResult> NewWarnings { get; }

    Type ICollectionVariableElementSnapshot.ElementType => typeof( TElement );
    Type ICollectionVariableElementSnapshot.ValidationResultType => typeof( TValidationResult );
    object ICollectionVariableElementSnapshot.Element => Element;
    IEnumerable ICollectionVariableElementSnapshot.PreviousErrors => PreviousErrors;
    IEnumerable ICollectionVariableElementSnapshot.NewErrors => NewErrors;
    IEnumerable ICollectionVariableElementSnapshot.PreviousWarnings => PreviousWarnings;
    IEnumerable ICollectionVariableElementSnapshot.NewWarnings => NewWarnings;
}
