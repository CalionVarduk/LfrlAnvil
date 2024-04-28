using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Creates instances of <see cref="Bounds{T}"/> type.
/// </summary>
public static class Bounds
{
    /// <summary>
    /// Creates a new <see cref="Bounds{T}"/> instance.
    /// </summary>
    /// <param name="min">Minimum value.</param>
    /// <param name="max">Maximum value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="Bounds{T}"/> instance.</returns>
    /// <exception cref="ArgumentException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    public static Bounds<T> Create<T>(T min, T max)
        where T : IComparable<T>
    {
        return new Bounds<T>( min, max );
    }

    /// <summary>
    /// Attempts to extract the underlying type from the provided <see cref="Bounds{T}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Bounds{T}"/> type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Bounds{T}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Bounds<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
