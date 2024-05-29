using System;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a generic variable change event emitted by an <see cref="IReadOnlyCollectionVariable{TKey,TElement,TValidationResult}"/>.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
/// <typeparam name="TValidationResult">Validation result type.</typeparam>
public class CollectionVariableChangeEvent<TKey, TElement, TValidationResult> : ICollectionVariableChangeEvent<TKey, TElement>
    where TKey : notnull
    where TElement : notnull
{
    /// <summary>
    /// Creates a new <see cref="CollectionVariableChangeEvent{TKey,TElement,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="variable">Variable node that emitted this event.</param>
    /// <param name="previousState">Previous state of the <see cref="Variable"/>.</param>
    /// <param name="addedElements">Collection of elements added due to this event.</param>
    /// <param name="removedElements">Collection of elements removed due to this event.</param>
    /// <param name="refreshedElements">Collection of elements refreshed due to this event.</param>
    /// <param name="replacedElements">Collection of elements replaced due to this event.</param>
    /// <param name="source">Specifies the source of this value change.</param>
    public CollectionVariableChangeEvent(
        IReadOnlyCollectionVariable<TKey, TElement, TValidationResult> variable,
        VariableState previousState,
        IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> addedElements,
        IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> removedElements,
        IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> refreshedElements,
        IReadOnlyList<CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>> replacedElements,
        VariableChangeSource source)
    {
        Variable = variable;
        PreviousState = previousState;
        AddedElements = addedElements;
        RemovedElements = removedElements;
        RefreshedElements = refreshedElements;
        ReplacedElements = replacedElements;
        Source = source;
        NewState = Variable.State;
    }

    /// <inheritdoc cref="ICollectionVariableChangeEvent{TKey,TElement}.Variable" />
    public IReadOnlyCollectionVariable<TKey, TElement, TValidationResult> Variable { get; }

    /// <inheritdoc />
    public VariableState PreviousState { get; }

    /// <inheritdoc />
    public VariableState NewState { get; }

    /// <inheritdoc cref="ICollectionVariableChangeEvent{TKey,TElement}.AddedElements" />
    public IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> AddedElements { get; }

    /// <inheritdoc cref="ICollectionVariableChangeEvent{TKey,TElement}.RemovedElements" />
    public IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> RemovedElements { get; }

    /// <inheritdoc cref="ICollectionVariableChangeEvent{TKey,TElement}.RefreshedElements" />
    public IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> RefreshedElements { get; }

    /// <inheritdoc cref="ICollectionVariableChangeEvent{TKey,TElement}.ReplacedElements" />
    public IReadOnlyList<CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>> ReplacedElements { get; }

    /// <inheritdoc />
    public VariableChangeSource Source { get; }

    IReadOnlyCollectionVariable<TKey, TElement> ICollectionVariableChangeEvent<TKey, TElement>.Variable => Variable;

    IReadOnlyList<ICollectionVariableElementSnapshot<TElement>> ICollectionVariableChangeEvent<TKey, TElement>.AddedElements =>
        AddedElements;

    IReadOnlyList<ICollectionVariableElementSnapshot<TElement>> ICollectionVariableChangeEvent<TKey, TElement>.RemovedElements =>
        RemovedElements;

    IReadOnlyList<ICollectionVariableElementSnapshot<TElement>> ICollectionVariableChangeEvent<TKey, TElement>.RefreshedElements =>
        RefreshedElements;

    IReadOnlyList<ICollectionVariableElementSnapshot<TElement>> ICollectionVariableChangeEvent<TKey, TElement>.ReplacedElements =>
        ReplacedElements;

    Type ICollectionVariableChangeEvent.KeyType => typeof( TKey );
    Type ICollectionVariableChangeEvent.ElementType => typeof( TElement );
    Type ICollectionVariableChangeEvent.ValidationResultType => typeof( TValidationResult );
    IReadOnlyCollectionVariable ICollectionVariableChangeEvent.Variable => Variable;
    IReadOnlyList<ICollectionVariableElementSnapshot> ICollectionVariableChangeEvent.AddedElements => AddedElements;
    IReadOnlyList<ICollectionVariableElementSnapshot> ICollectionVariableChangeEvent.RemovedElements => RemovedElements;
    IReadOnlyList<ICollectionVariableElementSnapshot> ICollectionVariableChangeEvent.RefreshedElements => RefreshedElements;
    IReadOnlyList<ICollectionVariableElementSnapshot> ICollectionVariableChangeEvent.ReplacedElements => ReplacedElements;
    IVariableNode IVariableNodeEvent.Variable => Variable;
}
