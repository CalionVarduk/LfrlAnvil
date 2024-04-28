using System;

namespace LfrlAnvil.Generators;

/// <summary>
/// Represents a generic generator of objects within specified range.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
public interface IBoundGenerator<T> : IGenerator<T>
    where T : IComparable<T>
{
    /// <summary>
    /// Range of values that can be generated.
    /// </summary>
    Bounds<T> Bounds { get; }
}
