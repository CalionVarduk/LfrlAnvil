using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlSoft.NET.Core.Collections
{
    public interface IReadOnlyMultiSet<T> : IReadOnlyCollection<Pair<T, int>>
        where T : notnull
    {
        long FullCount { get; }
        IEnumerable<T> DistinctItems { get; }
        IEqualityComparer<T> Comparer { get; }

        [Pure]
        bool Contains(T item);

        [Pure]
        int GetMultiplicity(T item);
    }
}
