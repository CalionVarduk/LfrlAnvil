using System.Collections.Generic;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a <see cref="ITreeDictionary{TKey,TValue}"/> node.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public interface ITreeDictionaryNode<out TKey, out TValue> : ITreeNode<TValue>
{
    /// <summary>
    /// Underlying key.
    /// </summary>
    TKey Key { get; }

    /// <inheritdoc cref="ITreeNode{T}.Parent" />
    new ITreeDictionaryNode<TKey, TValue>? Parent { get; }

    /// <inheritdoc cref="ITreeNode{T}.Children" />
    new IReadOnlyList<ITreeDictionaryNode<TKey, TValue>> Children { get; }
}
