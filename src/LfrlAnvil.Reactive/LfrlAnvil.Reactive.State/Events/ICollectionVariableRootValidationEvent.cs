using System;
using System.Collections;

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a type-erased validation event emitted by an <see cref="IReadOnlyCollectionVariableRoot"/>.
/// </summary>
public interface ICollectionVariableRootValidationEvent : IVariableNodeEvent
{
    /// <summary>
    /// Key type.
    /// </summary>
    Type KeyType { get; }

    /// <summary>
    /// Element type.
    /// </summary>
    Type ElementType { get; }

    /// <summary>
    /// Validation result type.
    /// </summary>
    Type ValidationResultType { get; }

    /// <summary>
    /// <see cref="ICollectionVariableRootChangeEvent"/> instance associated with this validation event.
    /// </summary>
    ICollectionVariableRootChangeEvent? AssociatedChange { get; }

    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    new IReadOnlyCollectionVariableRoot Variable { get; }

    /// <summary>
    /// Collection of validation errors before the change.
    /// </summary>
    IEnumerable PreviousErrors { get; }

    /// <summary>
    /// Collection of validation errors after the change.
    /// </summary>
    IEnumerable NewErrors { get; }

    /// <summary>
    /// Collection of validation warnings before the change.
    /// </summary>
    IEnumerable PreviousWarnings { get; }

    /// <summary>
    /// Collection of validation warnings after the change.
    /// </summary>
    IEnumerable NewWarnings { get; }

    /// <summary>
    /// Source child node event.
    /// </summary>
    IVariableNodeEvent? SourceEvent { get; }
}

/// <summary>
/// Represents a generic validation event emitted by an <see cref="IReadOnlyCollectionVariableRoot"/>.
/// </summary>
/// <typeparam name="TValidationResult">Variable's validation result type.</typeparam>
public interface ICollectionVariableRootValidationEvent<TValidationResult> : ICollectionVariableRootValidationEvent
{
    /// <summary>
    /// Collection of validation errors before the change.
    /// </summary>
    new Chain<TValidationResult> PreviousErrors { get; }

    /// <summary>
    /// Collection of validation errors after the change.
    /// </summary>
    new Chain<TValidationResult> NewErrors { get; }

    /// <summary>
    /// Collection of validation warnings before the change.
    /// </summary>
    new Chain<TValidationResult> PreviousWarnings { get; }

    /// <summary>
    /// Collection of validation warnings after the change.
    /// </summary>
    new Chain<TValidationResult> NewWarnings { get; }
}
