using System.Collections.Generic;

namespace LfrlAnvil.Collections
{
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
    }
}
