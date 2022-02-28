using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Functional.Extensions
{
    public static class DictionaryExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Maybe<TValue> TryGetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : notnull
        {
            return dictionary.TryGetValue( key, out var result ) ? new Maybe<TValue>( result ) : Maybe<TValue>.None;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Maybe<TValue> TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
            where TKey : notnull
            where TValue : notnull
        {
            return dictionary.Remove( key, out var removed ) ? new Maybe<TValue>( removed ) : Maybe<TValue>.None;
        }
    }
}
