using System;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.State.Events;

public class VariableValidationEvent<TValue, TValidationResult> : IVariableValidationEvent<TValidationResult>
{
    public VariableValidationEvent(
        IReadOnlyVariable<TValue, TValidationResult> variable,
        Chain<TValidationResult> previousErrors,
        Chain<TValidationResult> previousWarnings,
        VariableState previousState,
        VariableValueChangedEvent<TValue, TValidationResult>? associatedChange)
    {
        AssociatedChange = associatedChange;
        Variable = variable;
        PreviousState = previousState;
        NewState = variable.State;
        PreviousErrors = previousErrors;
        NewErrors = Variable.Errors;
        PreviousWarnings = previousWarnings;
        NewWarnings = Variable.Warnings;
    }

    public VariableValueChangedEvent<TValue, TValidationResult>? AssociatedChange { get; }
    public IReadOnlyVariable<TValue, TValidationResult> Variable { get; }
    public VariableState PreviousState { get; }
    public VariableState NewState { get; }
    public Chain<TValidationResult> PreviousErrors { get; }
    public Chain<TValidationResult> NewErrors { get; }
    public Chain<TValidationResult> PreviousWarnings { get; }
    public Chain<TValidationResult> NewWarnings { get; }

    Type IVariableValidationEvent.ValueType => typeof( TValue );
    Type IVariableValidationEvent.ValidationResultType => typeof( TValidationResult );
    IVariableValueChangedEvent? IVariableValidationEvent.AssociatedChange => AssociatedChange;
    IReadOnlyVariable IVariableValidationEvent.Variable => Variable;

    IReadOnlyCollection<object?> IVariableValidationEvent.PreviousErrors =>
        (IReadOnlyCollection<object?>)(IReadOnlyCollection<TValidationResult>)PreviousErrors;

    IReadOnlyCollection<object?> IVariableValidationEvent.NewErrors =>
        (IReadOnlyCollection<object?>)(IReadOnlyCollection<TValidationResult>)NewErrors;

    IReadOnlyCollection<object?> IVariableValidationEvent.PreviousWarnings =>
        (IReadOnlyCollection<object?>)(IReadOnlyCollection<TValidationResult>)PreviousWarnings;

    IReadOnlyCollection<object?> IVariableValidationEvent.NewWarnings =>
        (IReadOnlyCollection<object?>)(IReadOnlyCollection<TValidationResult>)NewWarnings;
}
