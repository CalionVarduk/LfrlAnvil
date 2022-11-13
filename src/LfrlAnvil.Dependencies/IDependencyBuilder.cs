using System;

namespace LfrlAnvil.Dependencies;

public interface IDependencyBuilder
{
    Type DependencyType { get; }
    DependencyLifetime Lifetime { get; }
    Type? SharedImplementorType { get; }
    IDependencyImplementorBuilder? Implementor { get; }

    IDependencyBuilder SetLifetime(DependencyLifetime lifetime);
    IDependencyBuilder FromSharedImplementor(Type type);

    IDependencyImplementorBuilder FromFactory(Func<IDependencyScope, object> factory);
    // TODO: extend, must support factories, explicit ctors, explicit shared implementor keys, explicit implementor types (ctor auto-discovery) & self
}
