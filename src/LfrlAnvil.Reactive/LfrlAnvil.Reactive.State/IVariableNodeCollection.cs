using System.Collections;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.State;

public interface IVariableNodeCollection : IEnumerable
{
    int Count { get; }
    IReadOnlyCollection<object> Keys { get; }
    IReadOnlyCollection<IVariableNode> Values { get; }
    IReadOnlyCollection<object> ChangedNodeKeys { get; }
    IReadOnlyCollection<object> InvalidNodeKeys { get; }
    IReadOnlyCollection<object> WarningNodeKeys { get; }
    IReadOnlyCollection<object> ReadOnlyNodeKeys { get; }
    IReadOnlyCollection<object> DirtyNodeKeys { get; }
}

public interface IVariableNodeCollection<TKey> : IReadOnlyDictionary<TKey, IVariableNode>, IVariableNodeCollection
    where TKey : notnull
{
    new int Count { get; }
    new IReadOnlyCollection<TKey> Keys { get; }
    new IReadOnlyCollection<IVariableNode> Values { get; }
    IEqualityComparer<TKey> Comparer { get; }
    new IReadOnlySet<TKey> ChangedNodeKeys { get; }
    new IReadOnlySet<TKey> InvalidNodeKeys { get; }
    new IReadOnlySet<TKey> WarningNodeKeys { get; }
    new IReadOnlySet<TKey> ReadOnlyNodeKeys { get; }
    new IReadOnlySet<TKey> DirtyNodeKeys { get; }
}
