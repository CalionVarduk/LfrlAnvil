using System;

namespace LfrlAnvil.Reactive.State.Events;

/// <inheritdoc />
public class VariableValueChangeEvent<TValue, TValidationResult> : IVariableValueChangeEvent<TValue>
{
    /// <summary>
    /// Creates a new <see cref="VariableValueChangeEvent{TValue,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="variable">Variable node that emitted this event.</param>
    /// <param name="previousValue">Value before the change.</param>
    /// <param name="previousState">Value after the change.</param>
    /// <param name="source">Specifies the source of this value change.</param>
    public VariableValueChangeEvent(
        IReadOnlyVariable<TValue, TValidationResult> variable,
        TValue previousValue,
        VariableState previousState,
        VariableChangeSource source)
    {
        Variable = variable;
        PreviousState = previousState;
        NewState = variable.State;
        PreviousValue = previousValue;
        NewValue = variable.Value;
        Source = source;
    }

    /// <inheritdoc cref="IVariableValueChangeEvent{TValue}.Variable" />
    public IReadOnlyVariable<TValue, TValidationResult> Variable { get; }

    /// <inheritdoc />
    public VariableState PreviousState { get; }

    /// <inheritdoc />
    public VariableState NewState { get; }

    /// <inheritdoc />
    public TValue PreviousValue { get; }

    /// <inheritdoc />
    public TValue NewValue { get; }

    /// <inheritdoc />
    public VariableChangeSource Source { get; }

    IReadOnlyVariable<TValue> IVariableValueChangeEvent<TValue>.Variable => Variable;
    Type IVariableValueChangeEvent.ValueType => typeof( TValue );
    Type IVariableValueChangeEvent.ValidationResultType => typeof( TValidationResult );
    IReadOnlyVariable IVariableValueChangeEvent.Variable => Variable;
    object? IVariableValueChangeEvent.PreviousValue => PreviousValue;
    object? IVariableValueChangeEvent.NewValue => NewValue;
    IVariableNode IVariableNodeEvent.Variable => Variable;
}
