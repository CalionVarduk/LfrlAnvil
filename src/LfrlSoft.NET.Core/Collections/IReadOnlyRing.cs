using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlSoft.NET.Core.Collections
{
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
}
