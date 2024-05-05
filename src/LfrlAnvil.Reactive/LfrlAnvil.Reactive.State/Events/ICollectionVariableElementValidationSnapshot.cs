namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a generic validation snapshot of an <see cref="IReadOnlyCollectionVariable"/> element.
/// </summary>
/// <typeparam name="TValidationResult">Validation result type.</typeparam>
public interface ICollectionVariableElementValidationSnapshot<TValidationResult> : ICollectionVariableElementSnapshot
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
