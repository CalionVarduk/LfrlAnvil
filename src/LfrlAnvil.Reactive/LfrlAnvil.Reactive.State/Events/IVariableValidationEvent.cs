using System;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.State.Events;

public interface IVariableValidationEvent : IVariableNodeEvent
{
    Type ValueType { get; }
    Type ValidationResultType { get; }
    IVariableValueChangeEvent? AssociatedChange { get; }
    new IReadOnlyVariable Variable { get; }
    IReadOnlyCollection<object?> PreviousErrors { get; }
    IReadOnlyCollection<object?> NewErrors { get; }
    IReadOnlyCollection<object?> PreviousWarnings { get; }
    IReadOnlyCollection<object?> NewWarnings { get; }
}

public interface IVariableValidationEvent<TValidationResult> : IVariableValidationEvent
{
    new Chain<TValidationResult> PreviousErrors { get; }
    new Chain<TValidationResult> NewErrors { get; }
    new Chain<TValidationResult> PreviousWarnings { get; }
    new Chain<TValidationResult> NewWarnings { get; }
}
