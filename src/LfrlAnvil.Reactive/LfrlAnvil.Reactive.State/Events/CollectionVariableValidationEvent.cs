using System;
using System.Collections;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a generic validation event emitted by an <see cref="IReadOnlyCollectionVariable{TKey,TElement,TValidationResult}"/>.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
/// <typeparam name="TValidationResult">Variable's validation result type.</typeparam>
public class CollectionVariableValidationEvent<TKey, TElement, TValidationResult> : ICollectionVariableValidationEvent<TValidationResult>
    where TKey : notnull
    where TElement : notnull
{
    /// <summary>
    /// Creates a new <see cref="CollectionVariableValidationEvent{TKey,TElement,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="variable">Variable node that emitted this event.</param>
    /// <param name="previousErrors">Collection of validation errors before the change.</param>
    /// <param name="previousWarnings">Collection of validation warnings before the change.</param>
    /// <param name="previousState">Previous state of the <see cref="Variable"/>.</param>
    /// <param name="elements">Collection of elements associated with this event.</param>
    /// <param name="associatedChange"><see cref="ICollectionVariableChangeEvent"/> instance associated with this validation event.</param>
    public CollectionVariableValidationEvent(
        IReadOnlyCollectionVariable<TKey, TElement, TValidationResult> variable,
        Chain<TValidationResult> previousErrors,
        Chain<TValidationResult> previousWarnings,
        VariableState previousState,
        IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> elements,
        CollectionVariableChangeEvent<TKey, TElement, TValidationResult>? associatedChange)
    {
        Variable = variable;
        PreviousErrors = previousErrors;
        NewErrors = Variable.Errors;
        PreviousWarnings = previousWarnings;
        NewWarnings = Variable.Warnings;
        PreviousState = previousState;
        NewState = Variable.State;
        Elements = elements;
        AssociatedChange = associatedChange;
    }

    /// <inheritdoc cref="ICollectionVariableValidationEvent.AssociatedChange" />
    public CollectionVariableChangeEvent<TKey, TElement, TValidationResult>? AssociatedChange { get; }

    /// <inheritdoc cref="ICollectionVariableValidationEvent.Variable" />
    public IReadOnlyCollectionVariable<TKey, TElement, TValidationResult> Variable { get; }

    /// <inheritdoc />
    public VariableState PreviousState { get; }

    /// <inheritdoc />
    public VariableState NewState { get; }

    /// <inheritdoc />
    public Chain<TValidationResult> PreviousErrors { get; }

    /// <inheritdoc />
    public Chain<TValidationResult> NewErrors { get; }

    /// <inheritdoc />
    public Chain<TValidationResult> PreviousWarnings { get; }

    /// <inheritdoc />
    public Chain<TValidationResult> NewWarnings { get; }

    /// <inheritdoc cref="ICollectionVariableValidationEvent{TValidationResult}.Elements" />
    public IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> Elements { get; }

    IReadOnlyList<ICollectionVariableElementValidationSnapshot<TValidationResult>> ICollectionVariableValidationEvent<TValidationResult>.
        Elements =>
        Elements;

    Type ICollectionVariableValidationEvent.KeyType => typeof( TKey );
    Type ICollectionVariableValidationEvent.ElementType => typeof( TElement );
    Type ICollectionVariableValidationEvent.ValidationResultType => typeof( TValidationResult );
    ICollectionVariableChangeEvent? ICollectionVariableValidationEvent.AssociatedChange => AssociatedChange;
    IReadOnlyCollectionVariable ICollectionVariableValidationEvent.Variable => Variable;
    IEnumerable ICollectionVariableValidationEvent.PreviousErrors => PreviousErrors;
    IEnumerable ICollectionVariableValidationEvent.NewErrors => NewErrors;
    IEnumerable ICollectionVariableValidationEvent.PreviousWarnings => PreviousWarnings;
    IEnumerable ICollectionVariableValidationEvent.NewWarnings => NewWarnings;
    IReadOnlyList<ICollectionVariableElementSnapshot> ICollectionVariableValidationEvent.Elements => Elements;
    IVariableNode IVariableNodeEvent.Variable => Variable;
}
