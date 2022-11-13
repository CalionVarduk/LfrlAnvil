using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public interface IDependencyContainerBuilder : IDependencyLocatorBuilder
{
    new IDependencyContainerBuilder SetDefaultLifetime(DependencyLifetime lifetime);
    new IDependencyContainerBuilder SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy);

    [Pure]
    DependencyContainerBuildResult<IDisposableDependencyContainer> TryBuild();
}
