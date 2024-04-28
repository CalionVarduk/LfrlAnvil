using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="Memory{T}"/> extension methods.
/// </summary>
public static class MemoryExtensions
{
    /// <summary>
    /// Returns an enumerator instance created from the given memory's <see cref="ReadOnlyMemory{T}.Span"/>.
    /// </summary>
    /// <param name="source">Read-only source memory.</param>
    /// <typeparam name="T">Memory element type.</typeparam>
    /// <returns>Underlying memory's span's enumerator.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReadOnlySpan<T>.Enumerator GetEnumerator<T>(this ReadOnlyMemory<T> source)
    {
        return source.Span.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator instance created from the given memory's <see cref="Memory{T}.Span"/>.
    /// </summary>
    /// <param name="source">Source memory.</param>
    /// <typeparam name="T">Memory element type.</typeparam>
    /// <returns>Underlying memory's span's enumerator.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Span<T>.Enumerator GetEnumerator<T>(this Memory<T> source)
    {
        return source.Span.GetEnumerator();
    }
}
