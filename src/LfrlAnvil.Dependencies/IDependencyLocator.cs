using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public interface IDependencyLocator
{
    IDependencyScope AttachedScope { get; }

    [Pure]
    object Resolve(Type type);

    [Pure]
    T Resolve<T>()
        where T : class;

    [Pure]
    object? TryResolve(Type type);

    [Pure]
    T? TryResolve<T>()
        where T : class;
}
