using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public interface IDependencyContainerBuilder : IDependencyLocatorBuilder
{
    new IDependencyContainerBuilder SetDefaultLifetime(DependencyLifetime lifetime);

    [Pure]
    DependencyContainerBuildResult<IDisposableDependencyContainer> TryBuild();
}
