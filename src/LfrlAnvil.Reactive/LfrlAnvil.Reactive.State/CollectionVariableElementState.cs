using System;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents the state of an element in its collection.
/// </summary>
[Flags]
public enum CollectionVariableElementState : byte
{
    /// <summary>
    /// Specifies that the element has not changed.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Specifies that the element has changed.
    /// </summary>
    Changed = 1,

    /// <summary>
    /// Specifies that the element contains validation errors.
    /// </summary>
    Invalid = 2,

    /// <summary>
    /// Specifies that the element contains validation warnings.
    /// </summary>
    Warning = 4,

    /// <summary>
    /// Specifies that the element has been added.
    /// </summary>
    Added = 8,

    /// <summary>
    /// Specifies that the element has been removed.
    /// </summary>
    Removed = 16,

    /// <summary>
    /// Specifies that the element does not exist.
    /// </summary>
    NotFound = 32
}
