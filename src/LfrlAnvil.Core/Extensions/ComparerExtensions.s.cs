using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="IComparer{T}"/> extension methods.
/// </summary>
public static class ComparerExtensions
{
    /// <summary>
    /// Creates a new <see cref="IComparer{T}"/> instance that inverts comparison result returned by the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source comparer.</param>
    /// <typeparam name="T">Comparer value type.</typeparam>
    /// <returns>New <see cref="IComparer{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IComparer<T> Invert<T>(this IComparer<T> source)
    {
        return new InvertedComparer<T>( source );
    }
}
