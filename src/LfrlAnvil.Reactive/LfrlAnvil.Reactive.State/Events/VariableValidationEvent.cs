using System;
using System.Collections;

namespace LfrlAnvil.Reactive.State.Events;

/// <inheritdoc />
public class VariableValidationEvent<TValue, TValidationResult> : IVariableValidationEvent<TValidationResult>
{
    /// <summary>
    /// Creates a new <see cref="VariableValidationEvent{TValue,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="variable">Variable node that emitted this event.</param>
    /// <param name="previousErrors">Collection of validation errors before the change.</param>
    /// <param name="previousWarnings">Collection of validation warnings before the change.</param>
    /// <param name="previousState">Previous state of the <see cref="Variable"/>.</param>
    /// <param name="associatedChange">
    /// <see cref="VariableValueChangeEvent{TEvent,TValidationResult}"/> instance associated with this validation event.
    /// </param>
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

    /// <inheritdoc cref="IVariableValidationEvent.AssociatedChange" />
    public VariableValueChangeEvent<TValue, TValidationResult>? AssociatedChange { get; }

    /// <inheritdoc cref="IVariableValidationEvent.Variable" />
    public IReadOnlyVariable<TValue, TValidationResult> Variable { get; }

    /// <inheritdoc />
    public VariableState PreviousState { get; }

    /// <inheritdoc />
    public VariableState NewState { get; }

    /// <inheritdoc />
    public Chain<TValidationResult> PreviousErrors { get; }

    /// <inheritdoc />
    public Chain<TValidationResult> NewErrors { get; }

    /// <inheritdoc />
    public Chain<TValidationResult> PreviousWarnings { get; }

    /// <inheritdoc />
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
