using System;

namespace LfrlAnvil.Reactive.State.Events;

public interface IVariableValueChangeEvent : IVariableNodeEvent
{
    Type ValueType { get; }
    Type ValidationResultType { get; }
    new IReadOnlyVariable Variable { get; }
    object? PreviousValue { get; }
    object? NewValue { get; }
    VariableChangeSource Source { get; }
}

public interface IVariableValueChangeEvent<TValue> : IVariableValueChangeEvent
{
    new IReadOnlyVariable<TValue> Variable { get; }
    new TValue PreviousValue { get; }
    new TValue NewValue { get; }
}
