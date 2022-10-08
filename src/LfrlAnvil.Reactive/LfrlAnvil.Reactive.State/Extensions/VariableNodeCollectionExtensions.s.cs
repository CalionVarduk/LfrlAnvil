using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Reactive.State.Extensions;

public static class VariableNodeCollectionExtensions
{
    [Pure]
    public static IEnumerable<IVariableNode> FindAllInvalid<TKey>(this IVariableNodeCollection<TKey> nodes)
        where TKey : notnull
    {
        return nodes.InvalidNodeKeys.Select( k => nodes[k] );
    }

    [Pure]
    public static IEnumerable<IVariableNode> FindAllWarning<TKey>(this IVariableNodeCollection<TKey> nodes)
        where TKey : notnull
    {
        return nodes.WarningNodeKeys.Select( k => nodes[k] );
    }

    [Pure]
    public static IEnumerable<IVariableNode> FindAllChanged<TKey>(this IVariableNodeCollection<TKey> nodes)
        where TKey : notnull
    {
        return nodes.ChangedNodeKeys.Select( k => nodes[k] );
    }

    [Pure]
    public static IEnumerable<IVariableNode> FindAllReadOnly<TKey>(this IVariableNodeCollection<TKey> nodes)
        where TKey : notnull
    {
        return nodes.ReadOnlyNodeKeys.Select( k => nodes[k] );
    }

    [Pure]
    public static IEnumerable<IVariableNode> FindAllDirty<TKey>(this IVariableNodeCollection<TKey> nodes)
        where TKey : notnull
    {
        return nodes.DirtyNodeKeys.Select( k => nodes[k] );
    }
}
