using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public interface IDependencyContainerBuilder : IDependencyLocatorBuilder
{
    Type InjectablePropertyType { get; }

    new IDependencyContainerBuilder SetDefaultLifetime(DependencyLifetime lifetime);
    new IDependencyContainerBuilder SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy);

    IDependencyContainerBuilder SetInjectablePropertyType(Type openGenericType);

    [Pure]
    DependencyContainerBuildResult<IDisposableDependencyContainer> TryBuild();
}
