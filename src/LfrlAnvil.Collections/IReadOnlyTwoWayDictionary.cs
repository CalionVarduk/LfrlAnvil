using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

public interface IReadOnlyTwoWayDictionary<T1, T2> : IReadOnlyCollection<Pair<T1, T2>>
    where T1 : notnull
    where T2 : notnull
{
    IReadOnlyDictionary<T1, T2> Forward { get; }
    IReadOnlyDictionary<T2, T1> Reverse { get; }
    IEqualityComparer<T1> ForwardComparer { get; }
    IEqualityComparer<T2> ReverseComparer { get; }

    [Pure]
    bool Contains(T1 first, T2 second);
}
