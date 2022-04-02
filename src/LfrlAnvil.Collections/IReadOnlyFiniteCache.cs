﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Collections
{
    public interface IReadOnlyFiniteCache<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        int Capacity { get; }
        IEqualityComparer<TKey> Comparer { get; }
        KeyValuePair<TKey, TValue>? Oldest { get; }
        KeyValuePair<TKey, TValue>? Newest { get; }
        new bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue result);
    }
}