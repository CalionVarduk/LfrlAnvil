using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil;

/// <summary>
/// Contains boxed instances of value types.
/// </summary>
public static class Boxed
{
    /// <summary>
    /// Represents boxed <see cref="Boolean"/> equal to <b>true</b>.
    /// </summary>
    public static readonly object True = true;

    /// <summary>
    /// Represents boxed <see cref="Boolean"/> equal to <b>false</b>.
    /// </summary>
    public static readonly object False = false;

    /// <summary>
    /// Gets a stored boxed representation of the provided <see cref="Boolean"/> <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to get.</param>
    /// <returns>Boxed <paramref name="value"/> representation.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static object GetBool(bool value)
    {
        return value ? True : False;
    }
}
