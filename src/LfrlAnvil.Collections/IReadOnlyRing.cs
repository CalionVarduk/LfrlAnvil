using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

public interface IReadOnlyRing<out T> : IReadOnlyList<T?>
{
    int WriteIndex { get; }

    [Pure]
    int GetWrappedIndex(int index);

    [Pure]
    int GetWriteIndex(int offset);

    [Pure]
    IEnumerable<T?> Read(int readIndex);
}