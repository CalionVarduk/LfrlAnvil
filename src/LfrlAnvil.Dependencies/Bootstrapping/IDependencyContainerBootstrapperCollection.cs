using System.Collections.Generic;

namespace LfrlAnvil.Dependencies.Bootstrapping;

public interface IDependencyContainerBootstrapperCollection<in TBuilder>
    : IDependencyContainerBootstrapper<TBuilder>, IReadOnlyList<IDependencyContainerBootstrapper<TBuilder>>
    where TBuilder : IDependencyContainerBuilder { }
