namespace LfrlAnvil.Reactive.State.Events;

public class CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>
    : CollectionVariableElementSnapshot<TElement, TValidationResult>
    where TElement : notnull
{
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

    public TElement PreviousElement { get; }
}
