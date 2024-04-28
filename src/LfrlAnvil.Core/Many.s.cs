using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil;

/// <summary>
/// Creates collections.
/// </summary>
public static class Many
{
    /// <summary>
    /// Returns provided range of <paramref name="values"/> as an <see cref="Array"/>.
    /// </summary>
    /// <param name="values">Range of values.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns><paramref name="values"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T[] Create<T>(params T[] values)
    {
        return values;
    }
}
