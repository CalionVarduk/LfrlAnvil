using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Collections;

public interface IHeap<T> : IReadOnlyHeap<T>
{
    T Extract();
    bool TryExtract([MaybeNullWhen( false )] out T result);
    void Add(T item);
    void Pop();
    bool TryPop();
    T Replace(T item);
    bool TryReplace(T item, [MaybeNullWhen( false )] out T replaced);
    void Clear();
}
