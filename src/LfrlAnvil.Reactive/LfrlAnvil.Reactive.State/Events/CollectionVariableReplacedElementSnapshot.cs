namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a generic snapshot of a replaced <see cref="IReadOnlyCollectionVariable"/> element.
/// </summary>
/// <typeparam name="TElement">Element type.</typeparam>
/// <typeparam name="TValidationResult">Validation result type.</typeparam>
public class CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>
    : CollectionVariableElementSnapshot<TElement, TValidationResult>
    where TElement : notnull
{
    /// <summary>
    /// Creates a new <see cref="CollectionVariableReplacedElementSnapshot{TElement,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="previousElement">Replaced element.</param>
    /// <param name="element">Underlying element.</param>
    /// <param name="previousState">
    /// Previous state of the <see cref="CollectionVariableElementSnapshot{TElement,TValidationResult}.Element"/>.
    /// </param>
    /// <param name="newState">
    /// Current state of the <see cref="CollectionVariableElementSnapshot{TElement,TValidationResult}.Element"/>.
    /// </param>
    /// <param name="previousErrors">Collection of validation errors before the change.</param>
    /// <param name="newErrors">Collection of validation errors after the change.</param>
    /// <param name="previousWarnings">Collection of validation warnings before the change.</param>
    /// <param name="newWarnings">Collection of validation warnings after the change.</param>
    public CollectionVariableReplacedElementSnapshot(
        TElement previousElement,
        TElement element,
        CollectionVariableElementState previousState,
        CollectionVariableElementState newState,
        Chain<TValidationResult> previousErrors,
        Chain<TValidationResult> newErrors,
        Chain<TValidationResult> previousWarnings,
        Chain<TValidationResult> newWarnings)
        : base( element, previousState, newState, previousErrors, newErrors, previousWarnings, newWarnings )
    {
        PreviousElement = previousElement;
    }

    /// <summary>
    /// Replaced element.
    /// </summary>
    public TElement PreviousElement { get; }
}
