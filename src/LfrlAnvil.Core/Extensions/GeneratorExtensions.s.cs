using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="IGenerator"/> extension methods.
/// </summary>
public static class GeneratorExtensions
{
    /// <summary>
    /// Creates a new <see cref="IEnumerable"/> instance (potentially infinite) from the provided generator.
    /// </summary>
    /// <param name="source">Source generator.</param>
    /// <returns>New <see cref="IEnumerable"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable ToEnumerable(this IGenerator source)
    {
        while ( source.TryGenerate( out var result ) )
            yield return result;
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance (potentially infinite) from the provided generator.
    /// </summary>
    /// <param name="source">Source generator.</param>
    /// <typeparam name="T">Generator value type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> ToEnumerable<T>(this IGenerator<T> source)
    {
        while ( source.TryGenerate( out var result ) )
            yield return result;
    }
}
