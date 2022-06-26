using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

public interface IMultiSet<T> : IReadOnlyMultiSet<T>, ISet<Pair<T, int>>
    where T : notnull
{
    new int Count { get; }
    int Add(T item);
    int AddMany(T item, int count);
    int Remove(T item);
    int RemoveMany(T item, int count);
    int RemoveAll(T item);
    int SetMultiplicity(T item, int value);

    [Pure]
    new bool Contains(Pair<T, int> item);

    [Pure]
    new bool IsProperSubsetOf(IEnumerable<Pair<T, int>> other);

    [Pure]
    new bool IsProperSupersetOf(IEnumerable<Pair<T, int>> other);

    [Pure]
    new bool IsSubsetOf(IEnumerable<Pair<T, int>> other);

    [Pure]
    new bool IsSupersetOf(IEnumerable<Pair<T, int>> other);

    [Pure]
    new bool Overlaps(IEnumerable<Pair<T, int>> other);

    [Pure]
    new bool SetEquals(IEnumerable<Pair<T, int>> other);
}
