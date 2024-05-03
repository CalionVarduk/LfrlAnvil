using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Creates instances of <see cref="Injected{T}"/> type.
/// </summary>
public static class Injected
{
    /// <summary>
    /// Creates a new <see cref="Injected{T}"/> instance.
    /// </summary>
    /// <param name="instance">Underlying value.</param>
    /// <typeparam name="T">Member type.</typeparam>
    /// <returns>New <see cref="Injected{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Injected<T> Create<T>(T instance)
    {
        return new Injected<T>( instance );
    }

    /// <summary>
    /// Attempts to extract the underlying type from the provided <see cref="Injected{T}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Injected{T}"/> type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Injected{T}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Injected<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
