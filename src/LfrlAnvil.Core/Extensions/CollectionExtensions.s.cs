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

/// <summary>
/// Contains extension methods for materialized collections.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Returns the provided <paramref name="source"/>, unless it is null, in which case returns an empty array instead.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><paramref name="source"/> if it is not null, otherwise an empty array.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IReadOnlyCollection<T> EmptyIfNull<T>(this IReadOnlyCollection<T>? source)
    {
        return source ?? Array.Empty<T>();
    }

    /// <summary>
    /// Checks if the provided <paramref name="source"/> is null or empty.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is null or empty, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsNullOrEmpty<T>([NotNullWhen( false )] this IReadOnlyCollection<T>? source)
    {
        return source is null || source.IsEmpty();
    }

    /// <summary>
    /// Checks if the provided <paramref name="source"/> is empty.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is empty, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsEmpty<T>(this IReadOnlyCollection<T> source)
    {
        return source.Count == 0;
    }

    /// <summary>
    /// Checks if the provided <paramref name="source"/> contains at least <paramref name="count"/> number of elements.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="count">Expected minimum number of elements.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> contains correct number of elements, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsAtLeast<T>(this IReadOnlyCollection<T> source, int count)
    {
        return source.Count >= count;
    }

    /// <summary>
    /// Checks if the provided <paramref name="source"/> contains at most <paramref name="count"/> number of elements.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="count">Expected maximum number of elements.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> contains correct number of elements, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsAtMost<T>(this IReadOnlyCollection<T> source, int count)
    {
        return source.Count <= count;
    }

    /// <summary>
    /// Checks if the provided <paramref name="source"/> contains between
    /// <paramref name="minCount"/> and <paramref name="maxCount"/> number of elements.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="minCount">Expected minimum number of elements.</param>
    /// <param name="maxCount">Expected maximum number of elements.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> contains correct number of elements, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsInRange<T>(this IReadOnlyCollection<T> source, int minCount, int maxCount)
    {
        return source.Count >= minCount && source.Count <= maxCount;
    }

    /// <summary>
    /// Checks if the provided <paramref name="source"/> contains exactly <paramref name="count"/> number of elements.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="count">Expected exact number of elements.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> contains correct number of elements, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsExactly<T>(this IReadOnlyCollection<T> source, int count)
    {
        return source.Count == count;
    }

    /// <summary>
    /// Copies the provided <paramref name="source"/> into the given <paramref name="span"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="span">Target <see cref="RentedMemorySequenceSpan{T}"/> to copy <paramref name="source"/> elements to.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <exception cref="ArgumentException">
    /// When <paramref name="source"/> contains more elements than the <paramref name="span"/> can hold.
    /// </exception>
    /// <remarks>Copying starts at the beginning of the <paramref name="span"/>.</remarks>
    public static void CopyTo<T>(this IReadOnlyCollection<T> source, RentedMemorySequenceSpan<T> span)
    {
        Ensure.ContainsAtMost( source, span.Length );

        var index = 0;
        foreach ( var e in source )
            span[index++] = e;
    }

    /// <summary>
    /// Reads a value associated with the specified <paramref name="key"/> from the <paramref name="cache"/>, if it exists.
    /// Otherwise adds a new entry to the <paramref name="cache"/> with the value returned by the <paramref name="provider"/>
    /// invocation and returns that value.
    /// </summary>
    /// <param name="cache">Source cache.</param>
    /// <param name="key">Entry's key.</param>
    /// <param name="provider">
    /// Entry's value provider. Gets invoked only when entry with the provided <paramref name="key"/>
    /// does not exist in the <paramref name="cache"/>.
    /// </param>
    /// <typeparam name="TKey">Cache key type.</typeparam>
    /// <typeparam name="TValue">Cache value type.</typeparam>
    /// <returns>
    /// Value associated with the provided <paramref name="key"/>, if it exists, otherwise value returned by <paramref name="provider"/>.
    /// </returns>
    public static TValue GetOrAdd<TKey, TValue>(this ICache<TKey, TValue> cache, TKey key, Func<TKey, TValue> provider)
        where TKey : notnull
    {
        if ( cache.TryGetValue( key, out var value ) )
            return value;

        value = provider( key );
        cache[key] = value;
        return value;
    }

    /// <summary>
    /// Reads a value associated with the specified <paramref name="key"/> from the <paramref name="cache"/>, if it exists.
    /// Otherwise adds a new entry to the <paramref name="cache"/> with the value returned by the asynchronous <paramref name="provider"/>
    /// invocation and returns that value.
    /// </summary>
    /// <param name="cache">Source cache.</param>
    /// <param name="key">Entry's key.</param>
    /// <param name="provider">
    /// Entry's asynchronous value provider. Gets invoked only when entry with the provided <paramref name="key"/>
    /// does not exist in the <paramref name="cache"/>.
    /// </param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> forwarded to the <paramref name="provider"/>.</param>
    /// <typeparam name="TKey">Cache key type.</typeparam>
    /// <typeparam name="TValue">Cache value type.</typeparam>
    /// <returns>
    /// Value associated with the provided <paramref name="key"/>, if it exists, otherwise value returned by <paramref name="provider"/>.
    /// </returns>
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

    /// <summary>
    /// Reads a value associated with the specified <paramref name="key"/> from the <paramref name="cache"/>, if it exists.
    /// Otherwise adds a new entry to the <paramref name="cache"/> with the value returned by the asynchronous <paramref name="provider"/>
    /// invocation and returns that value.
    /// </summary>
    /// <param name="cache">Source cache.</param>
    /// <param name="key">Entry's key.</param>
    /// <param name="provider">
    /// Entry's asynchronous value provider. Gets invoked only when entry with the provided <paramref name="key"/>
    /// does not exist in the <paramref name="cache"/>.
    /// </param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> forwarded to the <paramref name="provider"/>.</param>
    /// <typeparam name="TKey">Cache key type.</typeparam>
    /// <typeparam name="TValue">Cache value type.</typeparam>
    /// <returns>
    /// Value associated with the provided <paramref name="key"/>, if it exists, otherwise value returned by <paramref name="provider"/>.
    /// </returns>
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
