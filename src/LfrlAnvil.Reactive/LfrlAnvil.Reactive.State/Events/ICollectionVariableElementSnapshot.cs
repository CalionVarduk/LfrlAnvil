using System;
using System.Collections;

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a type-erased snapshot of an <see cref="IReadOnlyCollectionVariable"/> element.
/// </summary>
public interface ICollectionVariableElementSnapshot
{
    /// <summary>
    /// Element type.
    /// </summary>
    Type ElementType { get; }

    /// <summary>
    /// Validation result type.
    /// </summary>
    Type ValidationResultType { get; }

    /// <summary>
    /// Underlying element.
    /// </summary>
    object Element { get; }

    /// <summary>
    /// Previous state of the <see cref="Element"/>.
    /// </summary>
    CollectionVariableElementState PreviousState { get; }

    /// <summary>
    /// Current state of the <see cref="Element"/>.
    /// </summary>
    CollectionVariableElementState NewState { get; }

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
/// Represents a generic snapshot of an <see cref="IReadOnlyCollectionVariable"/> element.
/// </summary>
/// <typeparam name="TElement">Element type.</typeparam>
public interface ICollectionVariableElementSnapshot<out TElement> : ICollectionVariableElementSnapshot
{
    /// <summary>
    /// Underlying element.
    /// </summary>
    new TElement Element { get; }
}
