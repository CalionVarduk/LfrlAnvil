namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a generic variable root node that listens to its children's events and propagates them.
/// </summary>
/// <typeparam name="TKey">Child node's key type.</typeparam>
public interface IVariableRoot<TKey> : IReadOnlyVariableRoot<TKey>
    where TKey : notnull
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
}
