using System;

namespace LfrlAnvil.Reactive.State.Events;

public interface IVariableValueChangedEvent
{
    Type ValueType { get; }
    Type ValidationResultType { get; }
    IReadOnlyVariable Variable { get; }
    VariableState PreviousState { get; }
    VariableState NewState { get; }
    object? PreviousValue { get; }
    object? NewValue { get; }
    VariableChangeSource Source { get; }
}

public interface IVariableValueChangedEvent<TValue> : IVariableValueChangedEvent
{
    new IReadOnlyVariable<TValue> Variable { get; }
    new TValue PreviousValue { get; }
    new TValue NewValue { get; }
}
