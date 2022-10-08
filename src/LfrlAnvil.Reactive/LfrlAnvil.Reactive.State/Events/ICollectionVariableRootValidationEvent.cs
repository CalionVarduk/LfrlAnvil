using System;
using System.Collections;

namespace LfrlAnvil.Reactive.State.Events;

public interface ICollectionVariableRootValidationEvent : IVariableNodeEvent
{
    Type KeyType { get; }
    Type ElementType { get; }
    Type ValidationResultType { get; }
    ICollectionVariableRootChangeEvent? AssociatedChange { get; }
    new IReadOnlyCollectionVariableRoot Variable { get; }
    IEnumerable PreviousErrors { get; }
    IEnumerable NewErrors { get; }
    IEnumerable PreviousWarnings { get; }
    IEnumerable NewWarnings { get; }
    IVariableNodeEvent? SourceEvent { get; }
}

public interface ICollectionVariableRootValidationEvent<TValidationResult> : ICollectionVariableRootValidationEvent
{
    new Chain<TValidationResult> PreviousErrors { get; }
    new Chain<TValidationResult> NewErrors { get; }
    new Chain<TValidationResult> PreviousWarnings { get; }
    new Chain<TValidationResult> NewWarnings { get; }
}
