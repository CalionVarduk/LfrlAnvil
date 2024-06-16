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
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mapping;

public partial class TypeMappingConfiguration
{
    /// <summary>
    /// Creates a new <see cref="SingleTypeMappingConfiguration{TSource,TDestination}"/> instance.
    /// </summary>
    /// <param name="mapping"><typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.</param>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns>New <see cref="SingleTypeMappingConfiguration{TSource,TDestination}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SingleTypeMappingConfiguration<TSource, TDestination> Create<TSource, TDestination>(
        Func<TSource, ITypeMapper, TDestination> mapping)
    {
        return new SingleTypeMappingConfiguration<TSource, TDestination>( mapping );
    }
}
