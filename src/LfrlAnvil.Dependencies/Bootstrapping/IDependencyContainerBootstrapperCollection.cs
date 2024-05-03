using System.Collections.Generic;

namespace LfrlAnvil.Dependencies.Bootstrapping;

/// <summary>
/// Represents a collection of <see cref="IDependencyContainerBuilder"/> bootstrappers.
/// </summary>
/// <typeparam name="TBuilder">Dependency container builder type.</typeparam>
public interface IDependencyContainerBootstrapperCollection<in TBuilder>
    : IDependencyContainerBootstrapper<TBuilder>, IReadOnlyList<IDependencyContainerBootstrapper<TBuilder>>
    where TBuilder : IDependencyContainerBuilder { }
