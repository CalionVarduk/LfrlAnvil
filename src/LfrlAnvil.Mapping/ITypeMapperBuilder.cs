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

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Mapping;

/// <summary>
/// Represents a builder of <see cref="ITypeMapper"/> instances.
/// </summary>
public interface ITypeMapperBuilder
{
    /// <summary>
    /// Adds an <see cref="ITypeMappingConfiguration"/> instance to this builder.
    /// </summary>
    /// <param name="configuration"><see cref="ITypeMappingConfiguration"/> instance to add to this builder.</param>
    /// <returns><b>this</b>.</returns>
    ITypeMapperBuilder Configure(ITypeMappingConfiguration configuration);

    /// <summary>
    /// Adds a collection of <see cref="ITypeMappingConfiguration"/> instances to this builder.
    /// </summary>
    /// <param name="configurations">A collection <see cref="ITypeMappingConfiguration"/> instances to add to this builder.</param>
    /// <returns><b>this</b>.</returns>
    ITypeMapperBuilder Configure(IEnumerable<ITypeMappingConfiguration> configurations);

    /// <summary>
    /// Returns all currently registered <see cref="ITypeMappingConfiguration"/> instances in this builder.
    /// </summary>
    /// <returns>All currently registered <see cref="ITypeMappingConfiguration"/> instances.</returns>
    [Pure]
    IEnumerable<ITypeMappingConfiguration> GetConfigurations();

    /// <summary>
    /// Creates a new <see cref="ITypeMapper"/> instance.
    /// </summary>
    /// <returns>New <see cref="ITypeMapper"/> instance.</returns>
    [Pure]
    ITypeMapper Build();
}
