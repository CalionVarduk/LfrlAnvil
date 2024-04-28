using System;
using System.Collections.Generic;

namespace LfrlAnvil;

/// <summary>
/// Represents a memoized collection, that is a collection that is lazily materialized.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface IMemoizedCollection<T> : IReadOnlyCollection<T>
{
    /// <summary>
    /// <see cref="Lazy{T}"/> collection source.
    /// </summary>
    Lazy<IReadOnlyCollection<T>> Source { get; }

    /// <summary>
    /// Specifies whether or not this collection has been materialized.
    /// </summary>
    bool IsMaterialized { get; }
}
