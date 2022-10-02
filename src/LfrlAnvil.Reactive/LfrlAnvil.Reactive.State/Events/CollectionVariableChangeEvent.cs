using System;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.State.Events;

public class CollectionVariableChangeEvent<TKey, TElement, TValidationResult> : ICollectionVariableChangeEvent<TKey, TElement>
    where TKey : notnull
    where TElement : notnull
{
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

    public IReadOnlyCollectionVariable<TKey, TElement, TValidationResult> Variable { get; }
    public VariableState PreviousState { get; }
    public VariableState NewState { get; }
    public IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> AddedElements { get; }
    public IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> RemovedElements { get; }
    public IReadOnlyList<CollectionVariableElementSnapshot<TElement, TValidationResult>> RefreshedElements { get; }
    public IReadOnlyList<CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>> ReplacedElements { get; }
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
