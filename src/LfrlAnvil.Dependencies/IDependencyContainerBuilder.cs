using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public interface IDependencyContainerBuilder : IDependencyLocatorBuilder
{
    IDependencyContainerConfigurationBuilder Configuration { get; }

    new IDependencyContainerBuilder SetDefaultLifetime(DependencyLifetime lifetime);
    new IDependencyContainerBuilder SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy);

    [Pure]
    IDependencyLocatorBuilder<TKey> GetKeyedLocator<TKey>(TKey key)
        where TKey : notnull;

    [Pure]
    DependencyContainerBuildResult<IDisposableDependencyContainer> TryBuild();
}
