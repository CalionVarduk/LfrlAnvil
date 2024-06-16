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

using System;
using System.Reflection;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a dependency resolution builder.
/// </summary>
public interface IDependencyBuilder
{
    /// <summary>
    /// Dependency's type.
    /// </summary>
    Type DependencyType { get; }

    /// <summary>
    /// Dependency's lifetime.
    /// </summary>
    DependencyLifetime Lifetime { get; }

    /// <summary>
    /// Key of the shared implementor that implements this dependency.
    /// </summary>
    IDependencyKey? SharedImplementorKey { get; }

    /// <summary>
    /// Builder of an implementor of this dependency.
    /// </summary>
    IDependencyImplementorBuilder? Implementor { get; }

    /// <summary>
    /// Related dependency range builder instance.
    /// </summary>
    IDependencyRangeBuilder RangeBuilder { get; }

    /// <summary>
    /// Specifies whether or not this dependency is included in the related <see cref="RangeBuilder"/>.
    /// </summary>
    bool IsIncludedInRange { get; }

    /// <summary>
    /// Sets <see cref="IsIncludedInRange"/> of this instance.
    /// </summary>
    /// <param name="included">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyBuilder IncludeInRange(bool included = true);

    /// <summary>
    /// Sets <see cref="Lifetime"/> of this instance.
    /// </summary>
    /// <param name="lifetime">Lifetime to set.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyBuilder SetLifetime(DependencyLifetime lifetime);

    /// <summary>
    /// Specifies that this dependency should be implemented through a shared implementor of the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Shared implementor's type.</param>
    /// <param name="configuration">Optional configurator of the shared implementor.</param>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Resets <see cref="Implementor"/> to null.</remarks>
    IDependencyBuilder FromSharedImplementor(Type type, Action<IDependencyImplementorOptions>? configuration = null);

    /// <summary>
    /// Specifies that this dependency should be implemented by the best suited constructor of this dependency's type.
    /// </summary>
    /// <param name="configuration">Optional configurator of the constructor invocation.</param>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Resets <see cref="SharedImplementorKey"/> to null.</remarks>
    IDependencyImplementorBuilder FromConstructor(Action<IDependencyConstructorInvocationOptions>? configuration = null);

    /// <summary>
    /// Specifies that this dependency should be implemented by the provided constructor.
    /// </summary>
    /// <param name="info">Constructor to use for creating dependency instances.</param>
    /// <param name="configuration">Optional configurator of the constructor invocation.</param>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Resets <see cref="SharedImplementorKey"/> to null.</remarks>
    IDependencyImplementorBuilder FromConstructor(
        ConstructorInfo info,
        Action<IDependencyConstructorInvocationOptions>? configuration = null);

    /// <summary>
    /// Specifies that this dependency should be implemented by the best suited constructor of the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Implementor's type.</param>
    /// <param name="configuration">Optional configurator of the constructor invocation.</param>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Resets <see cref="SharedImplementorKey"/> to null.</remarks>
    IDependencyImplementorBuilder FromType(Type type, Action<IDependencyConstructorInvocationOptions>? configuration = null);

    /// <summary>
    /// Specifies that this dependency should be implemented by the provided explicit <paramref name="factory"/>.
    /// </summary>
    /// <param name="factory">Explicit creator of dependency instances.</param>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Resets <see cref="SharedImplementorKey"/> to null.</remarks>
    IDependencyImplementorBuilder FromFactory(Func<IDependencyScope, object> factory);
}
