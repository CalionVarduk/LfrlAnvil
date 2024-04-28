using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Creates instances of <see cref="Bitmask{T}"/> type.
/// </summary>
public static class Bitmask
{
    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance.
    /// </summary>
    /// <param name="value">Bitmask value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    public static Bitmask<T> Create<T>(T value)
        where T : struct, IConvertible, IComparable
    {
        return new Bitmask<T>( value );
    }

    /// <summary>
    /// Attempts to extract the underlying type from the provided <see cref="Bitmask{T}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Bitmask{T}"/> type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Bitmask{T}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Bitmask<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
