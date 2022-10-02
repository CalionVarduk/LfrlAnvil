using System;
using System.Collections;

namespace LfrlAnvil.Reactive.State.Events;

public interface IVariableValidationEvent : IVariableNodeEvent
{
    Type ValueType { get; }
    Type ValidationResultType { get; }
    IVariableValueChangeEvent? AssociatedChange { get; }
    new IReadOnlyVariable Variable { get; }
    IEnumerable PreviousErrors { get; }
    IEnumerable NewErrors { get; }
    IEnumerable PreviousWarnings { get; }
    IEnumerable NewWarnings { get; }
}

public interface IVariableValidationEvent<TValidationResult> : IVariableValidationEvent
{
    new Chain<TValidationResult> PreviousErrors { get; }
    new Chain<TValidationResult> NewErrors { get; }
    new Chain<TValidationResult> PreviousWarnings { get; }
    new Chain<TValidationResult> NewWarnings { get; }
}
