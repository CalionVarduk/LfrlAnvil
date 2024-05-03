using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Bootstrapping;

/// <summary>
/// Represents a collection of <see cref="DependencyContainerBuilder"/> bootstrappers.
/// </summary>
public sealed class DependencyContainerBootstrapperCollection
    : DependencyContainerBootstrapper, IDependencyContainerBootstrapperCollection<DependencyContainerBuilder>
{
    private readonly List<IDependencyContainerBootstrapper<DependencyContainerBuilder>> _inner;

    /// <summary>
    /// Creates a new empty <see cref="DependencyContainerBootstrapperCollection"/> instance.
    /// </summary>
    public DependencyContainerBootstrapperCollection()
    {
        _inner = new List<IDependencyContainerBootstrapper<DependencyContainerBuilder>>();
    }

    /// <inheritdoc />
    public IDependencyContainerBootstrapper<DependencyContainerBuilder> this[int index] => _inner[index];

    /// <inheritdoc />
    public int Count => _inner.Count;

    /// <summary>
    /// Adds the provided <paramref name="bootstrapper"/> to this collection.
    /// </summary>
    /// <param name="bootstrapper">Bootstrapper to add.</param>
    /// <returns><b>this</b>.</returns>
    public DependencyContainerBootstrapperCollection Add(IDependencyContainerBootstrapper<DependencyContainerBuilder> bootstrapper)
    {
        _inner.Add( bootstrapper );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<IDependencyContainerBootstrapper<DependencyContainerBuilder>> GetEnumerator()
    {
        return _inner.GetEnumerator();
    }

    /// <inheritdoc />
    protected override void BootstrapCore(DependencyContainerBuilder builder)
    {
        foreach ( var bootstrapper in _inner )
            bootstrapper.Bootstrap( builder );
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
