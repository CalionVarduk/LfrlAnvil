using System;

namespace LfrlAnvil.Generators;

/// <summary>
/// Represents a generic sequence generator of objects within specified range.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
public interface ISequenceGenerator<T> : IBoundGenerator<T>
    where T : IComparable<T>
{
    /// <summary>
    /// Difference between two consecutively generated values.
    /// </summary>
    T Step { get; }
}
