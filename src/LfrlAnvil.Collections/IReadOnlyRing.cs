using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic read-only circular range of elements with constant <see cref="IReadOnlyCollection{T}.Count"/>.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface IReadOnlyRing<out T> : IReadOnlyList<T?>
{
    /// <summary>
    /// Specifies a 0-based index of the next position that this ring will overwrite.
    /// </summary>
    int WriteIndex { get; }

    /// <summary>
    /// Calculates a 0-based index within the bounds of this ring.
    /// </summary>
    /// <param name="index">Base index to calculate.</param>
    /// <returns>0-based index within the bounds of this ring.</returns>
    /// <exception cref="DivideByZeroException">When size of this ring is equal to <b>0</b>.</exception>
    [Pure]
    int GetWrappedIndex(int index);

    /// <summary>
    /// Calculates a 0-based index within the bounds of this ring from (<see cref="WriteIndex"/> + <paramref name="offset"/>) expression.
    /// </summary>
    /// <param name="offset">Value to add to the <see cref="WriteIndex"/>.</param>
    /// <returns>0-based index within the bounds of this ring.</returns>
    /// <exception cref="DivideByZeroException">When size of this ring is equal to <b>0</b>.</exception>
    [Pure]
    int GetWriteIndex(int offset);

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that enumerates over all elements of this ring,
    /// starting from the provided <paramref name="readIndex"/>.
    /// </summary>
    /// <param name="readIndex">0-based index of an element to start at.</param>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    IEnumerable<T?> Read(int readIndex);
}
