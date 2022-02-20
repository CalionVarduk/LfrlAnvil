using System.Collections.Generic;

namespace LfrlAnvil.Collections
{
    public interface IMultiSet<T> : IReadOnlyMultiSet<T>, ICollection<Pair<T, int>>
        where T : notnull
    {
        int Add(T item);
        int AddMany(T item, int count);
        int Remove(T item);
        int RemoveMany(T item, int count);
        int RemoveAll(T item);
        int SetMultiplicity(T item, int value);
    }
}
