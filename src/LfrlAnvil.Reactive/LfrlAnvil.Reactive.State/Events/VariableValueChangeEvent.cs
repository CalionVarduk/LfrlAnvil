using System;

namespace LfrlAnvil.Reactive.State.Events;

public class VariableValueChangeEvent<TValue, TValidationResult> : IVariableValueChangeEvent<TValue>
{
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

    public IReadOnlyVariable<TValue, TValidationResult> Variable { get; }
    public VariableState PreviousState { get; }
    public VariableState NewState { get; }
    public TValue PreviousValue { get; }
    public TValue NewValue { get; }
    public VariableChangeSource Source { get; }

    IReadOnlyVariable<TValue> IVariableValueChangeEvent<TValue>.Variable => Variable;
    Type IVariableValueChangeEvent.ValueType => typeof( TValue );
    Type IVariableValueChangeEvent.ValidationResultType => typeof( TValidationResult );
    IReadOnlyVariable IVariableValueChangeEvent.Variable => Variable;
    object? IVariableValueChangeEvent.PreviousValue => PreviousValue;
    object? IVariableValueChangeEvent.NewValue => NewValue;
    IVariableNode IVariableNodeEvent.Variable => Variable;
}
