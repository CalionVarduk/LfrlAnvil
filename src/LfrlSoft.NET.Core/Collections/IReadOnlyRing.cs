using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlSoft.NET.Core.Collections
{
    public interface IReadOnlyRing<out T> : IReadOnlyList<T?>
    {
        int StartIndex { get; }

        [Pure]
        int GetUnderlyingIndex(int index);
    }
}
