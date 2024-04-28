using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Creates instances of <see cref="BoundsRange{T}"/> type.
/// </summary>
public static class BoundsRange
{
    /// <summary>
    /// Creates a new <see cref="BoundsRange{T}"/> instance from a single <see cref="Bounds{T}"/> instance.
    /// </summary>
    /// <param name="value">Single range.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static BoundsRange<T> Create<T>(Bounds<T> value)
        where T : IComparable<T>
    {
        return new BoundsRange<T>( value );
    }

    /// <summary>
    /// Creates a new <see cref="BoundsRange{T}"/> instance from a collection of <see cref="Bounds{T}"/> instances.
    /// </summary>
    /// <param name="range">Collection of ranges.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    /// <exception cref="ArgumentException">When <paramref name="range"/> is not ordered.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static BoundsRange<T> Create<T>(IEnumerable<Bounds<T>> range)
        where T : IComparable<T>
    {
        return new BoundsRange<T>( range );
    }

    /// <summary>
    /// Attempts to extract the underlying type from the provided <see cref="BoundsRange{T}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="BoundsRange{T}"/> type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="BoundsRange{T}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( BoundsRange<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
