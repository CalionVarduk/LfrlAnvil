using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="Enum"/> extension methods.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance out of the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Source value.</param>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> ToBitmask<T>(this T value)
        where T : struct, Enum
    {
        return new Bitmask<T>( value );
    }
}
