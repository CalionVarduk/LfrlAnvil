using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Creates instances of <see cref="IEqualityComparer{T}"/> type.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public static class EqualityComparerFactory<T>
{
    /// <summary>
    /// Creates a new <see cref="IEqualityComparer{T}"/> instance from the provided <paramref name="equalityComparer"/> that uses default
    /// <see cref="Object.GetHashCode()"/> implementation.
    /// </summary>
    /// <param name="equalityComparer">Value equality comparer.</param>
    /// <returns>New <see cref="IEqualityComparer{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEqualityComparer<T> Create(Func<T?, T?, bool> equalityComparer)
    {
        return new LambdaEqualityComparer<T>( equalityComparer );
    }

    /// <summary>
    /// Creates a new <see cref="IEqualityComparer{T}"/> instance from the provided <paramref name="equalityComparer"/>
    /// and <paramref name="hashCodeCalculator"/>.
    /// </summary>
    /// <param name="equalityComparer">Value equality comparer.</param>
    /// <param name="hashCodeCalculator">Hash code calculator.</param>
    /// <returns>New <see cref="IEqualityComparer{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEqualityComparer<T> Create(Func<T?, T?, bool> equalityComparer, Func<T, int> hashCodeCalculator)
    {
        return new LambdaEqualityComparer<T>( equalityComparer, hashCodeCalculator );
    }
}
