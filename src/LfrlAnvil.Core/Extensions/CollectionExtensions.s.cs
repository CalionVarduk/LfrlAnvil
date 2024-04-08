using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Caching;
using LfrlAnvil.Memory;

namespace LfrlAnvil.Extensions;

public static class CollectionExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IReadOnlyCollection<T> EmptyIfNull<T>(this IReadOnlyCollection<T>? source)
    {
        return source ?? Array.Empty<T>();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsNullOrEmpty<T>([NotNullWhen( false )] this IReadOnlyCollection<T>? source)
    {
        return source is null || source.IsEmpty();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsEmpty<T>(this IReadOnlyCollection<T> source)
    {
        return source.Count == 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsAtLeast<T>(this IReadOnlyCollection<T> source, int count)
    {
        return source.Count >= count;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsAtMost<T>(this IReadOnlyCollection<T> source, int count)
    {
        return source.Count <= count;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsBetween<T>(this IReadOnlyCollection<T> source, int minCount, int maxCount)
    {
        return source.Count >= minCount && source.Count <= maxCount;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsExactly<T>(this IReadOnlyCollection<T> source, int count)
    {
        return source.Count == count;
    }

    public static void CopyTo<T>(this IReadOnlyCollection<T> source, RentedMemorySequenceSpan<T> span)
    {
        Ensure.ContainsAtMost( source, span.Length );

        var index = 0;
        foreach ( var e in source )
            span[index++] = e;
    }

    public static TValue GetOrAdd<TKey, TValue>(this ICache<TKey, TValue> cache, TKey key, Func<TKey, TValue> provider)
        where TKey : notnull
    {
        if ( cache.TryGetValue( key, out var value ) )
            return value;

        value = provider( key );
        cache[key] = value;
        return value;
    }

    public static async ValueTask<TValue> GetOrAddAsync<TKey, TValue>(
        this ICache<TKey, TValue> cache,
        TKey key,
        Func<TKey, CancellationToken, ValueTask<TValue>> provider,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        if ( cache.TryGetValue( key, out var value ) )
            return value;

        value = await provider( key, cancellationToken );
        cache[key] = value;
        return value;
    }

    public static async ValueTask<TValue> GetOrAddAsync<TKey, TValue>(
        this ICache<TKey, TValue> cache,
        TKey key,
        Func<TKey, CancellationToken, Task<TValue>> provider,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        if ( cache.TryGetValue( key, out var value ) )
            return value;

        value = await provider( key, cancellationToken );
        cache[key] = value;
        return value;
    }
}
