using System;
using System.Collections.Generic;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic circular range of elements with constant <see cref="IReadOnlyCollection{T}.Count"/>.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface IRing<T> : IReadOnlyRing<T>
{
    /// <summary>
    /// Gets or sets an element at the specified position.
    /// </summary>
    /// <param name="index">0-based element position.</param>
    new T? this[int index] { get; set; }

    /// <inheritdoc cref="IReadOnlyRing{T}.WriteIndex" />
    new int WriteIndex { get; set; }

    /// <summary>
    /// Sets a value of the next element of this ring located at the <see cref="WriteIndex"/>
    /// and increments the <see cref="WriteIndex"/> by <b>1</b>.
    /// </summary>
    /// <param name="item">Value to set.</param>
    /// <exception cref="IndexOutOfRangeException">When size of this ring is equal to <b>0</b>.</exception>
    void SetNext(T item);

    /// <summary>
    /// Resets all elements in this ring to default value and sets <see cref="WriteIndex"/> to <b>0</b>.
    /// </summary>
    void Clear();
}
