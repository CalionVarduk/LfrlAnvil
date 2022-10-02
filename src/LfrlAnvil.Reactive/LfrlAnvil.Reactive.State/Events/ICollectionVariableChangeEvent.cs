using System;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.State.Events;

public interface ICollectionVariableChangeEvent : IVariableNodeEvent
{
    Type KeyType { get; }
    Type ElementType { get; }
    Type ValidationResultType { get; }
    new IReadOnlyCollectionVariable Variable { get; }
    IReadOnlyList<ICollectionVariableElementSnapshot> AddedElements { get; }
    IReadOnlyList<ICollectionVariableElementSnapshot> RemovedElements { get; }
    IReadOnlyList<ICollectionVariableElementSnapshot> RefreshedElements { get; }
    IReadOnlyList<ICollectionVariableElementSnapshot> ReplacedElements { get; }
    VariableChangeSource Source { get; }
}

public interface ICollectionVariableChangeEvent<TKey, TElement> : ICollectionVariableChangeEvent
    where TKey : notnull
    where TElement : notnull
{
    new IReadOnlyCollectionVariable<TKey, TElement> Variable { get; }
    new IReadOnlyList<ICollectionVariableElementSnapshot<TElement>> AddedElements { get; }
    new IReadOnlyList<ICollectionVariableElementSnapshot<TElement>> RemovedElements { get; }
    new IReadOnlyList<ICollectionVariableElementSnapshot<TElement>> RefreshedElements { get; }
    new IReadOnlyList<ICollectionVariableElementSnapshot<TElement>> ReplacedElements { get; }
}
