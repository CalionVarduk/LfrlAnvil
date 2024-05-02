using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a read-only generic heap data structure.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public interface IReadOnlyHeap<T> : IReadOnlyList<T>
{
    /// <summary>
    /// Entry comparer.
    /// </summary>
    IComparer<T> Comparer { get; }

    /// <summary>
    /// Returns an entry currently at the top of this heap.
    /// </summary>
    /// <returns>Entry currently at the top of this heap.</returns>
    /// <exception cref="IndexOutOfRangeException">When this heap is empty.</exception>
    [Pure]
    T Peek();

    /// <summary>
    /// Attempts to return an entry currently at the top of this heap if it is not empty.
    /// </summary>
    /// <param name="result"><b>out</b> parameter that returns an entry currently at the top of the heap.</param>
    /// <returns><b>true</b> when this heap is not empty, otherwise <b>false</b>.</returns>
    bool TryPeek([MaybeNullWhen( false )] out T result);
}
