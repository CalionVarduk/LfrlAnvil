using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil;

/// <summary>
/// Contains methods for type casting.
/// </summary>
public static class DynamicCast
{
    /// <summary>
    /// Attempts to cast the provided <paramref name="value"/> to the desired reference type.
    /// </summary>
    /// <param name="value">Value to cast.</param>
    /// <typeparam name="T">Desired type.</typeparam>
    /// <returns>Provided <paramref name="value"/> as the desired reference type or null when it is not of that type.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T? TryTo<T>(object? value)
        where T : class
    {
        return value as T;
    }

    /// <summary>
    /// Casts the provided <paramref name="value"/> to the desired reference type.
    /// </summary>
    /// <param name="value">Value to cast.</param>
    /// <typeparam name="T">Desired type.</typeparam>
    /// <returns>Provided <paramref name="value"/> as the desired reference type.</returns>
    /// <exception cref="InvalidCastException">When the provided <paramref name="value"/> is not of desired type.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    [return: NotNullIfNotNull( "value" )]
    public static T? To<T>(object? value)
        where T : class
    {
        return ( T? )value;
    }

    /// <summary>
    /// Attempts to cast the provided boxed <paramref name="value"/> to the desired value type.
    /// </summary>
    /// <param name="value">Value to cast.</param>
    /// <typeparam name="T">Desired type.</typeparam>
    /// <returns>Provided <paramref name="value"/> unboxed as the desired value type or null when it is not of that type.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T? TryUnbox<T>(object? value)
        where T : struct
    {
        return value is T t ? t : null;
    }

    /// <summary>
    /// Casts the provided boxed <paramref name="value"/> to the desired value type.
    /// </summary>
    /// <param name="value">Value to cast.</param>
    /// <typeparam name="T">Desired type.</typeparam>
    /// <returns>Provided <paramref name="value"/> unboxed as the desired value type.</returns>
    /// <exception cref="InvalidCastException">When the provided <paramref name="value"/> is not of desired type.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T Unbox<T>(object? value)
        where T : struct
    {
        return ( T )value!;
    }
}
