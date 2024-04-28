using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Creates instances of <see cref="Pair{T1,T2}"/> type.
/// </summary>
public static class Pair
{
    /// <summary>
    /// Creates a new <see cref="Pair{T1,T2}"/> instance.
    /// </summary>
    /// <param name="first">First item.</param>
    /// <param name="second">Second item.</param>
    /// <typeparam name="T1">First item type.</typeparam>
    /// <typeparam name="T2">Second item type.</typeparam>
    /// <returns>New <see cref="Pair{T1,T2}"/> instance.</returns>
    [Pure]
    public static Pair<T1, T2> Create<T1, T2>(T1 first, T2 second)
    {
        return new Pair<T1, T2>( first, second );
    }

    /// <summary>
    /// Attempts to extract the underlying first item type from the provided <see cref="Pair{T1,T2}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Pair{T1,T2}"/> first item type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Pair{T1,T2}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingFirstType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Pair<,> ) );
        return result.Length == 0 ? null : result[0];
    }

    /// <summary>
    /// Attempts to extract the underlying second item type from the provided <see cref="Pair{T1,T2}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Pair{T1,T2}"/> second item type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Pair{T1,T2}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingSecondType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Pair<,> ) );
        return result.Length == 0 ? null : result[1];
    }

    /// <summary>
    /// Attempts to extract underlying types from the provided <see cref="Pair{T1,T2}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract underlying types from.</param>
    /// <returns>
    /// <see cref="Pair{T1,T2}"/> of underlying types
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Pair{T1,T2}"/> type.
    /// </returns>
    [Pure]
    public static Pair<Type, Type>? GetUnderlyingTypes(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Pair<,> ) );
        return result.Length == 0 ? null : Create( result[0], result[1] );
    }
}
