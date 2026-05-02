// Copyright 2026 Łukasz Furlepa
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
/// Represents an open generic dependency resolution builder.
/// </summary>
public interface IOpenGenericDependencyBuilder
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
    IOpenGenericDependencyImplementorBuilder? Implementor { get; }

    /// <summary>
    /// Related dependency range builder instance.
    /// </summary>
    IOpenGenericDependencyRangeBuilder RangeBuilder { get; }

    /// <summary>
    /// Specifies whether this dependency is included in the related <see cref="RangeBuilder"/>.
    /// </summary>
    bool IsIncludedInRange { get; }

    /// <summary>
    /// Sets <see cref="IsIncludedInRange"/> of this instance.
    /// </summary>
    /// <param name="included">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns><b>this</b>.</returns>
    IOpenGenericDependencyBuilder IncludeInRange(bool included = true);

    /// <summary>
    /// Sets <see cref="Lifetime"/> of this instance.
    /// </summary>
    /// <param name="lifetime">Lifetime to set.</param>
    /// <returns><b>this</b>.</returns>
    IOpenGenericDependencyBuilder SetLifetime(DependencyLifetime lifetime);

    /// <summary>
    /// Specifies that this dependency should be implemented through a shared implementor of the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Shared implementor's type.</param>
    /// <param name="configuration">Optional configurator of the shared implementor.</param>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Resets <see cref="Implementor"/> to null.</remarks>
    IOpenGenericDependencyBuilder FromSharedImplementor(Type type, Action<IDependencyImplementorOptions>? configuration = null);

    /// <summary>
    /// Specifies that this dependency should be implemented by the best suited constructor of this dependency's type.
    /// </summary>
    /// <param name="configuration">Optional configurator of the constructor invocation.</param>
    /// <returns><see cref="Implementor"/>.</returns>
    /// <remarks>Resets <see cref="SharedImplementorKey"/> to null.</remarks>
    IOpenGenericDependencyImplementorBuilder FromConstructor(
        Action<IOpenGenericDependencyConstructorInvocationOptions>? configuration = null);

    /// <summary>
    /// Specifies that this dependency should be implemented by the provided constructor.
    /// </summary>
    /// <param name="info">Constructor to use for creating dependency instances.</param>
    /// <param name="configuration">Optional configurator of the constructor invocation.</param>
    /// <returns><see cref="Implementor"/>.</returns>
    /// <remarks>Resets <see cref="SharedImplementorKey"/> to null.</remarks>
    IOpenGenericDependencyImplementorBuilder FromConstructor(
        ConstructorInfo info,
        Action<IOpenGenericDependencyConstructorInvocationOptions>? configuration = null);

    /// <summary>
    /// Specifies that this dependency should be implemented by the best suited constructor of the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Implementor's type.</param>
    /// <param name="configuration">Optional configurator of the constructor invocation.</param>
    /// <returns><see cref="Implementor"/>.</returns>
    /// <remarks>Resets <see cref="SharedImplementorKey"/> to null.</remarks>
    IOpenGenericDependencyImplementorBuilder FromType(
        Type type,
        Action<IOpenGenericDependencyConstructorInvocationOptions>? configuration = null);
}
