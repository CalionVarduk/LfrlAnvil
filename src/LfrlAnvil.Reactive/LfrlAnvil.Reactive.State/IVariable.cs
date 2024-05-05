namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a generic variable.
/// </summary>
/// <typeparam name="TValue">Value type.</typeparam>
/// <typeparam name="TValidationResult">Validation result type.</typeparam>
public interface IVariable<TValue, TValidationResult> : IReadOnlyVariable<TValue, TValidationResult>
{
    /// <summary>
    /// Attempts to change the <see cref="IReadOnlyVariable{TValue}.Value"/> if the value to set is not equal to the current value.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult TryChange(TValue value);

    /// <summary>
    /// Changes the <see cref="IReadOnlyVariable{TValue}.Value"/>, even if the value to set is equal to the current value.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Change(TValue value);

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
