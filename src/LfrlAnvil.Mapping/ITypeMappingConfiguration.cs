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
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

/// <summary>
/// Represents a configuration of possibly multiple type mapping definitions.
/// </summary>
public interface ITypeMappingConfiguration
{
    /// <summary>
    /// Returns all type mapping definitions created by this configuration.
    /// </summary>
    /// <returns>All type mapping definitions created by this configuration.</returns>
    [Pure]
    IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores();
}
