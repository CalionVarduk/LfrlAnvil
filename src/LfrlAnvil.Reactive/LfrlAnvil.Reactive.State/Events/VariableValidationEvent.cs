﻿using System;
using System.Collections;

namespace LfrlAnvil.Reactive.State.Events;

public class VariableValidationEvent<TValue, TValidationResult> : IVariableValidationEvent<TValidationResult>
{
    public VariableValidationEvent(
        IReadOnlyVariable<TValue, TValidationResult> variable,
        Chain<TValidationResult> previousErrors,
        Chain<TValidationResult> previousWarnings,
        VariableState previousState,
        VariableValueChangeEvent<TValue, TValidationResult>? associatedChange)
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

    public VariableValueChangeEvent<TValue, TValidationResult>? AssociatedChange { get; }
    public IReadOnlyVariable<TValue, TValidationResult> Variable { get; }
    public VariableState PreviousState { get; }
    public VariableState NewState { get; }
    public Chain<TValidationResult> PreviousErrors { get; }
    public Chain<TValidationResult> NewErrors { get; }
    public Chain<TValidationResult> PreviousWarnings { get; }
    public Chain<TValidationResult> NewWarnings { get; }

    Type IVariableValidationEvent.ValueType => typeof( TValue );
    Type IVariableValidationEvent.ValidationResultType => typeof( TValidationResult );
    IVariableValueChangeEvent? IVariableValidationEvent.AssociatedChange => AssociatedChange;
    IReadOnlyVariable IVariableValidationEvent.Variable => Variable;
    IVariableNode IVariableNodeEvent.Variable => Variable;
    IEnumerable IVariableValidationEvent.PreviousErrors => PreviousErrors;
    IEnumerable IVariableValidationEvent.NewErrors => NewErrors;
    IEnumerable IVariableValidationEvent.PreviousWarnings => PreviousWarnings;
    IEnumerable IVariableValidationEvent.NewWarnings => NewWarnings;
}