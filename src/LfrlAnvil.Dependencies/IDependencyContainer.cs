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

using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a dependency container.
/// </summary>
public interface IDependencyContainer
{
    /// <summary>
    /// Specifies the root scope of this container.
    /// </summary>
    IDependencyScope RootScope { get; }

    /// <summary>
    /// Attempts to return the named scope.
    /// </summary>
    /// <param name="name">Scope's name.</param>
    /// <returns>Named <see cref="IDependencyScope"/> instance or null when named scope does not exist.</returns>
    [Pure]
    IDependencyScope? TryGetScope(string name);

    /// <summary>
    /// Returns the named scope.
    /// </summary>
    /// <param name="name">Scope's name.</param>
    /// <returns>Named <see cref="IDependencyScope"/> instance.</returns>
    /// <exception cref="DependencyScopeNotFoundException">When named scope does not exist.</exception>
    [Pure]
    IDependencyScope GetScope(string name);
}
