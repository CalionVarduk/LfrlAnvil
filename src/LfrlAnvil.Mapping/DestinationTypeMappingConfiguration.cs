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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

/// <summary>
/// Represents a configuration of possibly multiple type mapping definitions to a single <typeparamref name="TDestination"/> type.
/// </summary>
/// <typeparam name="TDestination">Destination type.</typeparam>
public class DestinationTypeMappingConfiguration<TDestination> : ITypeMappingConfiguration
{
    private readonly Dictionary<Type, TypeMappingStore> _stores;

    /// <summary>
    /// Creates a new <see cref="DestinationTypeMappingConfiguration{TDestination}"/> instance without any mapping definitions.
    /// </summary>
    public DestinationTypeMappingConfiguration()
    {
        _stores = new Dictionary<Type, TypeMappingStore>();
    }

    /// <summary>
    /// Destination type.
    /// </summary>
    public Type DestinationType => typeof( TDestination );

    /// <summary>
    /// Sets a <typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.
    /// </summary>
    /// <param name="mapping"><typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.</param>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <returns><b>this</b>.</returns>
    public DestinationTypeMappingConfiguration<TDestination> Configure<TSource>(Func<TSource, ITypeMapper, TDestination> mapping)
    {
        _stores[typeof( TSource )] = TypeMappingStore.Create( mapping );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores()
    {
        return _stores.Select( kv => KeyValuePair.Create( new TypeMappingKey( kv.Key, DestinationType ), kv.Value ) );
    }
}
