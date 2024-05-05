namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a source of variable's value change.
/// </summary>
public enum VariableChangeSource : byte
{
    /// <summary>
    /// Specifies manual change invocation.
    /// </summary>
    Change = 0,

    /// <summary>
    /// Specifies manual change attempt invocation.
    /// </summary>
    TryChange = 1,

    /// <summary>
    /// Specifies variable refresh.
    /// </summary>
    Refresh = 2,

    /// <summary>
    /// Specifies variable reset.
    /// </summary>
    Reset = 3,

    /// <summary>
    /// Specifies variable read-only change.
    /// </summary>
    SetReadOnly = 4,

    /// <summary>
    /// Specifies child node change.
    /// </summary>
    ChildNode = 5
}
