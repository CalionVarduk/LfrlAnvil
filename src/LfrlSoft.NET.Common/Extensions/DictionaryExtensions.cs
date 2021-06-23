using System;
using System.Collections.Generic;

namespace LfrlSoft.NET.Common.Extensions
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
    }
}
