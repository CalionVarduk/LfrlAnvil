using System.Collections.Generic;

namespace LfrlSoft.NET.Core.Collections
{
    public interface IReadOnlyRing<out T> : IReadOnlyList<T?>
    {
        int StartIndex { get; }
        int GetUnderlyingIndex(int index);
    }
}
