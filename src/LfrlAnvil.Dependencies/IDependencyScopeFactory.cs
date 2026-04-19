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

using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a dependency scope factory.
/// </summary>
public interface IDependencyScopeFactory
{
    /// <summary>
    /// Creates a new <see cref="IChildDependencyScope"/>.
    /// </summary>
    /// <param name="name">Optional child scope's name.</param>
    /// <returns>New <see cref="IChildDependencyScope"/> instance.</returns>
    /// <exception cref="NamedDependencyScopeCreationException">When the scope is named and that name already exists.</exception>
    [Pure]
    IChildDependencyScope BeginScope(string? name = null);
}
