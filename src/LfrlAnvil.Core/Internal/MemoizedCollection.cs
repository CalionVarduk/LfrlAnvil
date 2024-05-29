using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Internal;

/// <inheritdoc cref="IMemoizedCollection{T}" />
public sealed class MemoizedCollection<T> : IMemoizedCollection<T>
{
    /// <summary>
    /// Creates a new <see cref="MemoizedCollection{T}"/> instance.
    /// </summary>
    /// <param name="source">Source collection.</param>
    public MemoizedCollection(IEnumerable<T> source)
    {
        Source = new Lazy<IReadOnlyCollection<T>>( source.Materialize );
    }

    /// <inheritdoc />
    public Lazy<IReadOnlyCollection<T>> Source { get; }

    /// <inheritdoc />
    public int Count => Source.Value.Count;

    /// <inheritdoc />
    public bool IsMaterialized => Source.IsValueCreated;

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public IEnumerator<T> GetEnumerator()
    {
        return Source.Value.GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
