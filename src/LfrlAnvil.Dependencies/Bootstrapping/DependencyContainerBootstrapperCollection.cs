using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Bootstrapping;

public sealed class DependencyContainerBootstrapperCollection
    : DependencyContainerBootstrapper, IDependencyContainerBootstrapperCollection<DependencyContainerBuilder>
{
    private readonly List<IDependencyContainerBootstrapper<DependencyContainerBuilder>> _inner;

    public DependencyContainerBootstrapperCollection()
    {
        _inner = new List<IDependencyContainerBootstrapper<DependencyContainerBuilder>>();
    }

    public IDependencyContainerBootstrapper<DependencyContainerBuilder> this[int index] => _inner[index];
    public int Count => _inner.Count;

    public DependencyContainerBootstrapperCollection Add(IDependencyContainerBootstrapper<DependencyContainerBuilder> bootstrapper)
    {
        _inner.Add( bootstrapper );
        return this;
    }

    protected override void BootstrapCore(DependencyContainerBuilder builder)
    {
        foreach ( var bootstrapper in _inner )
            bootstrapper.Bootstrap( builder );
    }

    [Pure]
    public IEnumerator<IDependencyContainerBootstrapper<DependencyContainerBuilder>> GetEnumerator()
    {
        return _inner.GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
