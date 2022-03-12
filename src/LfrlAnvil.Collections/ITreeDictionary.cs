using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections
{
    public interface ITreeDictionary<TKey, TValue> : IReadOnlyTreeDictionary<TKey, TValue>, IDictionary<TKey, TValue>
        where TKey : notnull
    {
        new int Count { get; }
        new TValue this[TKey key] { get; set; }
        ITreeDictionaryNode<TKey, TValue> SetRoot(TKey key, TValue value);
        void SetRoot(ITreeDictionaryNode<TKey, TValue> node);
        new ITreeDictionaryNode<TKey, TValue> Add(TKey key, TValue value);
        void Add(ITreeDictionaryNode<TKey, TValue> node);
        ITreeDictionaryNode<TKey, TValue> AddTo(TKey parentKey, TKey key, TValue value);
        void AddTo(TKey parentKey, ITreeDictionaryNode<TKey, TValue> node);
        ITreeDictionaryNode<TKey, TValue> AddTo(ITreeDictionaryNode<TKey, TValue> parent, TKey key, TValue value);
        void AddTo(ITreeDictionaryNode<TKey, TValue> parent, ITreeDictionaryNode<TKey, TValue> node);
        ITreeDictionaryNode<TKey, TValue> AddSubtree(ITreeDictionaryNode<TKey, TValue> node);
        ITreeDictionaryNode<TKey, TValue> AddSubtreeTo(TKey parentKey, ITreeDictionaryNode<TKey, TValue> node);

        ITreeDictionaryNode<TKey, TValue> AddSubtreeTo(
            ITreeDictionaryNode<TKey, TValue> parent,
            ITreeDictionaryNode<TKey, TValue> node);

        void Remove(ITreeDictionaryNode<TKey, TValue> node);
        bool Remove(TKey key, [MaybeNullWhen( false )] out TValue removed);
        int RemoveSubtree(TKey key);
        int RemoveSubtree(ITreeDictionaryNode<TKey, TValue> node);
        void Swap(TKey firstKey, TKey secondKey);
        void Swap(ITreeDictionaryNode<TKey, TValue> firstNode, ITreeDictionaryNode<TKey, TValue> secondNode);
        ITreeDictionaryNode<TKey, TValue> MoveTo(TKey parentKey, TKey key);
        void MoveTo(TKey parentKey, ITreeDictionaryNode<TKey, TValue> node);
        ITreeDictionaryNode<TKey, TValue> MoveTo(ITreeDictionaryNode<TKey, TValue> parent, TKey key);
        void MoveTo(ITreeDictionaryNode<TKey, TValue> parent, ITreeDictionaryNode<TKey, TValue> node);
        ITreeDictionaryNode<TKey, TValue> MoveSubtreeTo(TKey parentKey, TKey key);
        void MoveSubtreeTo(TKey parentKey, ITreeDictionaryNode<TKey, TValue> node);
        ITreeDictionaryNode<TKey, TValue> MoveSubtreeTo(ITreeDictionaryNode<TKey, TValue> parent, TKey key);
        void MoveSubtreeTo(ITreeDictionaryNode<TKey, TValue> parent, ITreeDictionaryNode<TKey, TValue> node);
        new bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue result);

        [Pure]
        new bool ContainsKey(TKey key);
    }
}
