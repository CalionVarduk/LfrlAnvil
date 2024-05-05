using System;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents all possible states that variables nodes can be in.
/// </summary>
[Flags]
public enum VariableState : byte
{
    /// <summary>
    /// Specifies that the variable has not changed.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Specifies that the variable has changed, compared to the initial value.
    /// </summary>
    Changed = 1,

    /// <summary>
    /// Specifies that the variable contains validation errors.
    /// </summary>
    Invalid = 2,

    /// <summary>
    /// Specifies that the variable contains validation warnings.
    /// </summary>
    Warning = 4,

    /// <summary>
    /// Specifies that the variable is read-only.
    /// </summary>
    ReadOnly = 8,

    /// <summary>
    /// Specifies that the variable has been disposed.
    /// </summary>
    Disposed = 16,

    /// <summary>
    /// Specifies that the variable has been modified after its creation.
    /// </summary>
    Dirty = 32
}
