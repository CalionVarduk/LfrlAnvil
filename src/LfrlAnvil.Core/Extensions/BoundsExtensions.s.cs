using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="Bounds{T}"/> extension methods.
/// </summary>
public static class BoundsExtensions
{
    /// <summary>
    /// Returns a new <see cref="IEnumerable{T}"/> instance created from the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Conversion source.</param>
    /// <typeparam name="T">Bounds value type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance with two elements: <see cref="Bounds{T}.Min"/> followed by <see cref="Bounds{T}.Max"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> AsEnumerable<T>(this Bounds<T> source)
        where T : IComparable<T>
    {
        yield return source.Min;
        yield return source.Max;
    }
}
