﻿using System.Collections.Generic;

namespace LfrlSoft.NET.Core.Collections
{
    public interface IReadOnlyFixedCache<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        int Capacity { get; }
        IEqualityComparer<TKey> Comparer { get; }
        KeyValuePair<TKey, TValue>? Newest { get; }
        KeyValuePair<TKey, TValue>? Oldest { get; }
    }
}