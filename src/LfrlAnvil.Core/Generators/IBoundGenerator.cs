using System;

namespace LfrlAnvil.Generators;

public interface IBoundGenerator<T> : IGenerator<T>
    where T : IComparable<T>
{
    Bounds<T> Bounds { get; }
}
