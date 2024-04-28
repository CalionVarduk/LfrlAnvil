using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Creates instances of <see cref="Chain{T}"/> type.
/// </summary>
public static class Chain
{
    /// <summary>
    /// Creates a new <see cref="Chain{T}"/> instance from a single value.
    /// </summary>
    /// <param name="value">Single value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="Chain{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Chain<T> Create<T>(T value)
    {
        return new Chain<T>( value );
    }

    /// <summary>
    /// Creates a new <see cref="Chain{T}"/> instance from a collection of values.
    /// </summary>
    /// <param name="values">Collection of values.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="Chain{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Chain<T> Create<T>(IEnumerable<T> values)
    {
        return new Chain<T>( values );
    }

    /// <summary>
    /// Creates a new <see cref="Chain{T}"/> instance from another <see cref="Chain{T}"/> instance.
    /// </summary>
    /// <param name="other"><see cref="Chain{T}"/> instance to copy.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="Chain{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Chain<T> Create<T>(Chain<T> other)
    {
        return new Chain<T>( other );
    }

    /// <summary>
    /// Attempts to extract the underlying type from the provided <see cref="Chain{T}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Chain{T}"/> type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Chain{T}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Chain<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
