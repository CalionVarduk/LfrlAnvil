using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public interface IDependencyContainerBuilder : IDependencyLocatorBuilder
{
    Type InjectablePropertyType { get; }
    Type OptionalDependencyAttributeType { get; }

    new IDependencyContainerBuilder SetDefaultLifetime(DependencyLifetime lifetime);
    new IDependencyContainerBuilder SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy);

    IDependencyContainerBuilder SetInjectablePropertyType(Type openGenericType);
    IDependencyContainerBuilder SetOptionalDependencyAttributeType(Type attributeType);

    [Pure]
    IDependencyLocatorBuilder<TKey> GetKeyedLocator<TKey>(TKey key)
        where TKey : notnull;

    [Pure]
    DependencyContainerBuildResult<IDisposableDependencyContainer> TryBuild();
}
