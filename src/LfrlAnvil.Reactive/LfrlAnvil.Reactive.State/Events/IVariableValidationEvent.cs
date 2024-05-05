using System;
using System.Collections;

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a type-erased validation event emitted by an <see cref="IReadOnlyVariable"/>.
/// </summary>
public interface IVariableValidationEvent : IVariableNodeEvent
{
    /// <summary>
    /// Variable's value type.
    /// </summary>
    Type ValueType { get; }

    /// <summary>
    /// Variable's validation result type.
    /// </summary>
    Type ValidationResultType { get; }

    /// <summary>
    /// <see cref="IVariableValueChangeEvent"/> instance associated with this validation event.
    /// </summary>
    IVariableValueChangeEvent? AssociatedChange { get; }

    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    new IReadOnlyVariable Variable { get; }

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
}

/// <summary>
/// Represents a generic validation event emitted by an <see cref="IReadOnlyVariable{TValue,TValidationResult}"/>.
/// </summary>
/// <typeparam name="TValidationResult">Variable's validation result type.</typeparam>
public interface IVariableValidationEvent<TValidationResult> : IVariableValidationEvent
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
