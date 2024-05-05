namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents the result of a variable change attempt.
/// </summary>
public enum VariableChangeResult : byte
{
    /// <summary>
    /// Specifies that the variable has changed.
    /// </summary>
    Changed = 0,

    /// <summary>
    /// Specifies that the variable has not changed due to e.g. new value being equal to the current value.
    /// </summary>
    NotChanged = 1,

    /// <summary>
    /// Specifies that the variable has not changed due to being read-only.
    /// </summary>
    ReadOnly = 2
}
