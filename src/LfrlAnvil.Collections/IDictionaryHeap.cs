using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Collections
{
    public interface IDictionaryHeap<TKey, TValue> : IReadOnlyDictionaryHeap<TKey, TValue>
    {
        TValue Extract();
        bool TryExtract([MaybeNullWhen( false )] out TValue result);
        void Add(TKey key, TValue value);
        bool TryAdd(TKey key, TValue value);
        TValue Remove(TKey key);
        bool TryRemove(TKey key, [MaybeNullWhen( false )] out TValue removed);
        void Pop();
        bool TryPop();
        TValue Replace(TKey key, TValue value);
        bool TryReplace(TKey key, TValue value, [MaybeNullWhen( false )] out TValue replaced);
        TValue AddOrReplace(TKey key, TValue value);
        void Clear();
    }
}
