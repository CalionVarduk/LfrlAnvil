using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic read-only tree data structure with the ability to identify nodes by keys.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public interface IReadOnlyTreeDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Represents this tree's root node. Equal to null when the tree is empty.
    /// </summary>
    ITreeDictionaryNode<TKey, TValue>? Root { get; }

    /// <summary>
    /// Returns a new <see cref="IEnumerable{T}"/> instance that contains all nodes of this tree.
    /// </summary>
    IEnumerable<ITreeDictionaryNode<TKey, TValue>> Nodes { get; }

    /// <summary>
    /// Key equality comparer.
    /// </summary>
    IEqualityComparer<TKey> Comparer { get; }

    /// <summary>
    /// Attempts to return an <see cref="ITreeDictionaryNode{TKey,TValue}"/> associated with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>
    /// An <see cref="ITreeDictionaryNode{TKey,TValue}"/> instance associated with the provided <paramref name="key"/>
    /// or null if key does not exist.
    /// </returns>
    [Pure]
    ITreeDictionaryNode<TKey, TValue>? GetNode(TKey key);

    /// <summary>
    /// Creates a new <see cref="TreeDictionary{TKey,TValue}"/> instance equivalent to the sub-tree
    /// with a node associated with the provided <paramref name="key"/> as its root node.
    /// </summary>
    /// <param name="key">Root node's key.</param>
    /// <returns>
    /// New <see cref="TreeDictionary{TKey,TValue}"/> instance. Result will be empty if the provided <paramref name="key"/> does not exist.
    /// </returns>
    [Pure]
    ITreeDictionary<TKey, TValue> CreateSubtree(TKey key);
}
