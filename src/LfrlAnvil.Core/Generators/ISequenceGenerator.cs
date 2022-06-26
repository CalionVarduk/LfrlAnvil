using System;

namespace LfrlAnvil.Generators;

public interface ISequenceGenerator<T> : IBoundGenerator<T>
    where T : IComparable<T>
{
    T Step { get; }
}
