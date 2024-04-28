using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Creates instances of <see cref="Equality{T}"/> type.
/// </summary>
public static class Equality
{
    /// <summary>
    /// Creates a new <see cref="Equality{T}"/> instance.
    /// </summary>
    /// <param name="first">First value to compare.</param>
    /// <param name="second">Second value to compare.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="Equality{T}"/> instance.</returns>
    [Pure]
    public static Equality<T> Create<T>(T? first, T? second)
    {
        return new Equality<T>( first, second );
    }

    /// <summary>
    /// Attempts to extract the underlying type from the provided <see cref="Equality{T}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Equality{T}"/> type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Equality{T}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Equality<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
