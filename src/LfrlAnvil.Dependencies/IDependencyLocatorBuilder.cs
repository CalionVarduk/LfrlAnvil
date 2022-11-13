using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public interface IDependencyLocatorBuilder
{
    DependencyLifetime DefaultLifetime { get; }

    IDependencyImplementorBuilder AddSharedImplementor(Type type);
    IDependencyBuilder Add(Type type);
    IDependencyLocatorBuilder SetDefaultLifetime(DependencyLifetime lifetime);

    [Pure]
    IDependencyImplementorBuilder? TryGetSharedImplementor(Type type);

    [Pure]
    IDependencyBuilder? TryGetDependency(Type type);
}
