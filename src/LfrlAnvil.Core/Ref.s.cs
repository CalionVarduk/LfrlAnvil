using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Creates instances of <see cref="Ref{T}"/> type.
/// </summary>
public static class Ref
{
    /// <summary>
    /// Creates a new <see cref="Ref{T}"/> instance.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    /// <returns>New <see cref="Ref{T}"/> instance.</returns>
    [Pure]
    public static Ref<T> Create<T>(T value)
    {
        return new Ref<T>( value );
    }

    /// <summary>
    /// Attempts to extract the underlying type from the provided <see cref="Ref{T}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Ref{T}"/> type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Ref{T}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Ref<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
