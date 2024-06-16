// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
