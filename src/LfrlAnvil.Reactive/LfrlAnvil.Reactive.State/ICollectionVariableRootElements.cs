using System.Collections;
using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Internal;

namespace LfrlAnvil.Reactive.State;

public interface ICollectionVariableRootElements : IEnumerable
{
    int Count { get; }
    IEnumerable Keys { get; }
    IReadOnlyCollection<IVariableNode> Values { get; }
    IEnumerable InvalidElementKeys { get; }
    IEnumerable WarningElementKeys { get; }
    IEnumerable AddedElementKeys { get; }
    IEnumerable RemovedElementKeys { get; }
    IEnumerable ChangedElementKeys { get; }
}

public interface ICollectionVariableRootElements<TKey, TElement> : ICollectionVariableRootElements, IReadOnlyDictionary<TKey, TElement>
    where TKey : notnull
    where TElement : VariableNode
{
    new int Count { get; }
    new IReadOnlyCollection<TKey> Keys { get; }
    new IReadOnlyCollection<TElement> Values { get; }
    new IReadOnlySet<TKey> InvalidElementKeys { get; }
    new IReadOnlySet<TKey> WarningElementKeys { get; }
    new IReadOnlySet<TKey> AddedElementKeys { get; }
    new IReadOnlySet<TKey> RemovedElementKeys { get; }
    new IReadOnlySet<TKey> ChangedElementKeys { get; }
    IEqualityComparer<TKey> KeyComparer { get; }
}
