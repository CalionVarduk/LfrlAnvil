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

namespace LfrlAnvil.Dependencies.Bootstrapping;

/// <summary>
/// Represents an <see cref="IDependencyContainerBuilder"/> bootstrapper that contains a set of dependency definitions.
/// </summary>
/// <typeparam name="TBuilder">Dependency container builder type.</typeparam>
public interface IDependencyContainerBootstrapper<in TBuilder>
    where TBuilder : IDependencyContainerBuilder
{
    /// <summary>
    /// Populates the provided dependency container <paramref name="builder"/>
    /// with the set of dependency definitions stored by this bootstrapper.
    /// </summary>
    /// <param name="builder">Dependency container builder to populate.</param>
    void Bootstrap(TBuilder builder);
}
