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
using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a type-erased dependency locator.
/// </summary>
public interface IDependencyLocator
{
    /// <summary>
    /// <see cref="IDependencyScope"/> attached to this locator.
    /// </summary>
    IDependencyScope AttachedScope { get; }

    /// <summary>
    /// Key type of this locator or null when it is not keyed.
    /// </summary>
    Type? KeyType { get; }

    /// <summary>
    /// Key value of this locator or null when it is not keyed.
    /// </summary>
    object? Key { get; }

    /// <summary>
    /// Specifies whether or not this locator is keyed.
    /// </summary>
    bool IsKeyed { get; }

    /// <summary>
    /// Attempts to resolve a dependency of the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to resolve.</param>
    /// <returns>Instance of the resolved dependency or null when the resolution has failed.</returns>
    /// <exception cref="CircularDependencyReferenceException">When a circular dependency reference has been detected.</exception>
    [Pure]
    object? TryResolveUnsafe(Type type);

    /// <summary>
    /// Attempts to get the lifetime of a dependency of the provided <paramref name="type"/> resolvable by this locator.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns>
    /// <see cref="DependencyLifetime"/> of the provided dependency
    /// or null when the <paramref name="type"/> is not resolvable by this locator.
    /// </returns>
    [Pure]
    DependencyLifetime? TryGetLifetime(Type type);

    /// <summary>
    /// Returns all types resolvable by this locator.
    /// </summary>
    /// <returns>Collection of types resolvable by this locator.</returns>
    [Pure]
    Type[] GetResolvableTypes();
}

/// <summary>
/// Represents a generic keyed dependency locator.
/// </summary>
public interface IDependencyLocator<out TKey> : IDependencyLocator
    where TKey : notnull
{
    /// <summary>
    /// Key value of this locator.
    /// </summary>
    new TKey Key { get; }
}
