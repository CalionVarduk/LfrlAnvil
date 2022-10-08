using System;
using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Internal;

namespace LfrlAnvil.Reactive.State.Events;

public interface ICollectionVariableRootChangeEvent : IVariableNodeEvent
{
    Type KeyType { get; }
    Type ElementType { get; }
    Type ValidationResultType { get; }
    new IReadOnlyCollectionVariableRoot Variable { get; }
    IReadOnlyList<IVariableNode> AddedElements { get; }
    IReadOnlyList<IVariableNode> RemovedElements { get; }
    IReadOnlyList<IVariableNode> RestoredElements { get; }
    VariableChangeSource Source { get; }
    IVariableNodeEvent? SourceEvent { get; }
}

public interface ICollectionVariableRootChangeEvent<TKey, TElement> : ICollectionVariableRootChangeEvent
    where TKey : notnull
    where TElement : VariableNode
{
    new IReadOnlyCollectionVariableRoot<TKey, TElement> Variable { get; }
    new IReadOnlyList<TElement> AddedElements { get; }
    new IReadOnlyList<TElement> RemovedElements { get; }
    new IReadOnlyList<TElement> RestoredElements { get; }
}
