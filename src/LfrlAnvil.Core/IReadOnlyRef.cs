using System.Collections.Generic;

namespace LfrlAnvil;

/// <summary>
/// Represents a read-only generic boxed value.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public interface IReadOnlyRef<out T> : IReadOnlyList<T>
{
    /// <summary>
    /// Underlying value.
    /// </summary>
    T Value { get; }
}
