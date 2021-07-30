using System;
using System.Collections.Generic;
using LfrlSoft.NET.Core.Collections.Enums;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue? GetOrAddDefault<TKey, TValue>(this IDictionary<TKey, TValue?> dictionary, TKey key)
            where TKey : notnull
        {
            if ( dictionary.TryGetValue( key, out var value ) )
                return value;

            value = default;
            dictionary[key] = value;
            return value;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueProvider)
            where TKey : notnull
        {
            if ( dictionary.TryGetValue( key, out var value ) )
                return value;

            value = defaultValueProvider();
            dictionary[key] = value;
            return value;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Lazy<TValue> defaultValueProvider)
            where TKey : notnull
        {
            if ( dictionary.TryGetValue( key, out var value ) )
                return value;

            value = defaultValueProvider.Value;
            dictionary[key] = value;
            return value;
        }

        public static AddOrUpdateResult AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if ( dictionary.TryAdd( key, value ) )
                return AddOrUpdateResult.Added;

            dictionary[key] = value;
            return AddOrUpdateResult.Updated;
        }

        public static bool TryUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if ( ! dictionary.ContainsKey( key ) )
                return false;

            dictionary[key] = value;
            return true;
        }
    }
}
