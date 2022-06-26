using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Collections;

public interface ITwoWayDictionary<T1, T2> : IReadOnlyTwoWayDictionary<T1, T2>, ICollection<Pair<T1, T2>>
    where T1 : notnull
    where T2 : notnull
{
    new int Count { get; }
    bool TryAdd(T1 first, T2 second);
    void Add(T1 first, T2 second);
    bool TryUpdateForward(T1 first, T2 second);
    void UpdateForward(T1 first, T2 second);
    bool TryUpdateReverse(T2 second, T1 first);
    void UpdateReverse(T2 second, T1 first);
    bool RemoveForward(T1 value);
    bool RemoveReverse(T2 value);
    bool RemoveForward(T1 value, [MaybeNullWhen( false )] out T2 second);
    bool RemoveReverse(T2 value, [MaybeNullWhen( false )] out T1 first);
}
