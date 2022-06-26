using System.Collections.Generic;

namespace LfrlAnvil.Collections;

public interface ITreeDictionaryNode<out TKey, out TValue> : ITreeNode<TValue>
{
    TKey Key { get; }
    new ITreeDictionaryNode<TKey, TValue>? Parent { get; }
    new IReadOnlyList<ITreeDictionaryNode<TKey, TValue>> Children { get; }
}
