using System;
using System.Collections;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a type-erased validation event emitted by an <see cref="IReadOnlyCollectionVariable"/>.
/// </summary>
public interface ICollectionVariableValidationEvent : IVariableNodeEvent
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
    /// <see cref="ICollectionVariableChangeEvent"/> instance associated with this validation event.
    /// </summary>
    ICollectionVariableChangeEvent? AssociatedChange { get; }

    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    new IReadOnlyCollectionVariable Variable { get; }

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
    /// Collection of elements associated with this event.
    /// </summary>
    IReadOnlyList<ICollectionVariableElementSnapshot> Elements { get; }
}

/// <summary>
/// Represents a generic validation event emitted by an <see cref="IReadOnlyCollectionVariable"/>.
/// </summary>
/// <typeparam name="TValidationResult">Variable's validation result type.</typeparam>
public interface ICollectionVariableValidationEvent<TValidationResult> : ICollectionVariableValidationEvent
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

    /// <summary>
    /// Collection of elements associated with this event.
    /// </summary>
    new IReadOnlyList<ICollectionVariableElementValidationSnapshot<TValidationResult>> Elements { get; }
}
