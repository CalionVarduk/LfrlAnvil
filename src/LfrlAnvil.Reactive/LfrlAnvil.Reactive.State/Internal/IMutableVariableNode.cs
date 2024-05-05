namespace LfrlAnvil.Reactive.State.Internal;

/// <summary>
/// Represents a type-erased mutable variable state node.
/// </summary>
public interface IMutableVariableNode : IVariableNode
{
    /// <summary>
    /// Refreshes this variable.
    /// </summary>
    void Refresh();

    /// <summary>
    /// Refreshes this variable's validation.
    /// </summary>
    void RefreshValidation();

    /// <summary>
    /// Removes all errors and warnings from this variable.
    /// </summary>
    void ClearValidation();

    /// <summary>
    /// Sets this variable's read-only state.
    /// </summary>
    /// <param name="enabled">New read-only state.</param>
    void SetReadOnly(bool enabled);
}
