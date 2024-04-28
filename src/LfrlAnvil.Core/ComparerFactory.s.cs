using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil;

/// <summary>
/// Creates instances of <see cref="IComparer{T}"/> type.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public static class ComparerFactory<T>
{
    /// <summary>
    /// Creates a new <see cref="IComparer{T}"/> instance that compares values
    /// by comparing results of the provided <paramref name="selector"/> using the <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="selector">Comparable value selector.</param>
    /// <typeparam name="TValue">Comparable value type.</typeparam>
    /// <returns>New <see cref="IComparer{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IComparer<T> CreateBy<TValue>(Func<T?, TValue?> selector)
    {
        return CreateBy( selector, Comparer<TValue>.Default );
    }

    /// <summary>
    /// Creates a new <see cref="IComparer{T}"/> instance that compares values
    /// by comparing results of the provided <paramref name="selector"/>.
    /// </summary>
    /// <param name="selector">Comparable value selector.</param>
    /// <param name="valueComparer">Value comparer.</param>
    /// <typeparam name="TValue">Comparable value type.</typeparam>
    /// <returns>New <see cref="IComparer{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IComparer<T> CreateBy<TValue>(Func<T?, TValue?> selector, IComparer<TValue> valueComparer)
    {
        return Comparer<T>.Create( (a, b) => valueComparer.Compare( selector( a ), selector( b ) ) );
    }
}
