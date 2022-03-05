using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections
{
    public interface IReadOnlyHeap<T> : IReadOnlyList<T>
    {
        IComparer<T> Comparer { get; }

        [Pure]
        T Peek();

        bool TryPeek([MaybeNullWhen( false )] out T result);
    }
}
