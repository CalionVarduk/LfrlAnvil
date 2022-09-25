using System;

namespace LfrlAnvil.Reactive.State.Events;

public class VariableValueChangedEvent<TValue, TValidationResult> : IVariableValueChangedEvent<TValue>
{
    public VariableValueChangedEvent(
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

    public IReadOnlyVariable<TValue, TValidationResult> Variable { get; }
    public VariableState PreviousState { get; }
    public VariableState NewState { get; }
    public TValue PreviousValue { get; }
    public TValue NewValue { get; }
    public VariableChangeSource Source { get; }

    IReadOnlyVariable<TValue> IVariableValueChangedEvent<TValue>.Variable => Variable;
    Type IVariableValueChangedEvent.ValueType => typeof( TValue );
    Type IVariableValueChangedEvent.ValidationResultType => typeof( TValidationResult );
    IReadOnlyVariable IVariableValueChangedEvent.Variable => Variable;
    object? IVariableValueChangedEvent.PreviousValue => PreviousValue;
    object? IVariableValueChangedEvent.NewValue => NewValue;
}
