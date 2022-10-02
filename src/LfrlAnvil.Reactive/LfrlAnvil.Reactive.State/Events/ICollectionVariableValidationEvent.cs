using System;
using System.Collections;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.State.Events;

public interface ICollectionVariableValidationEvent : IVariableNodeEvent
{
    Type KeyType { get; }
    Type ElementType { get; }
    Type ValidationResultType { get; }
    ICollectionVariableChangeEvent? AssociatedChange { get; }
    new IReadOnlyCollectionVariable Variable { get; }
    IEnumerable PreviousErrors { get; }
    IEnumerable NewErrors { get; }
    IEnumerable PreviousWarnings { get; }
    IEnumerable NewWarnings { get; }
    IReadOnlyList<ICollectionVariableElementSnapshot> Elements { get; }
}

public interface ICollectionVariableValidationEvent<TValidationResult> : ICollectionVariableValidationEvent
{
    new Chain<TValidationResult> PreviousErrors { get; }
    new Chain<TValidationResult> NewErrors { get; }
    new Chain<TValidationResult> PreviousWarnings { get; }
    new Chain<TValidationResult> NewWarnings { get; }
    new IReadOnlyList<ICollectionVariableElementValidationSnapshot<TValidationResult>> Elements { get; }
}
