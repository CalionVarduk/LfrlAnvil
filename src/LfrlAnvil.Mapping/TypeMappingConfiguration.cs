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
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

/// <inheritdoc cref="ITypeMappingConfiguration" />
public partial class TypeMappingConfiguration : ITypeMappingConfiguration
{
    private readonly Dictionary<TypeMappingKey, TypeMappingStore> _stores;

    /// <summary>
    /// Creates a new <see cref="TypeMappingConfiguration"/> instance without any mapping definitions.
    /// </summary>
    public TypeMappingConfiguration()
    {
        _stores = new Dictionary<TypeMappingKey, TypeMappingStore>();
    }

    /// <summary>
    /// Sets a <typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.
    /// </summary>
    /// <param name="mapping"><typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.</param>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns><b>this</b>.</returns>
    public TypeMappingConfiguration Configure<TSource, TDestination>(Func<TSource, ITypeMapper, TDestination> mapping)
    {
        var key = new TypeMappingKey( typeof( TSource ), typeof( TDestination ) );
        _stores[key] = TypeMappingStore.Create( mapping );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores()
    {
        return _stores;
    }
}
