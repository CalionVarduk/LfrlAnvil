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

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Mapping.Exceptions;

namespace LfrlAnvil.Mapping;

/// <summary>
/// A lightweight generic container for a source object to map.
/// </summary>
/// <typeparam name="TSource">Source object type.</typeparam>
public readonly struct TypeMappingContext<TSource>
{
    internal TypeMappingContext(ITypeMapper typeMapper, TSource source)
    {
        TypeMapper = typeMapper;
        Source = source;
    }

    /// <summary>
    /// Attached <see cref="ITypeMapper"/> instance.
    /// </summary>
    public ITypeMapper TypeMapper { get; }

    /// <summary>
    /// Source object to map.
    /// </summary>
    public TSource Source { get; }

    /// <summary>
    /// Maps the <see cref="Source"/> to the desired <typeparamref name="TDestination"/> type
    /// using the attached <see cref="TypeMapper"/> instance.
    /// </summary>
    /// <typeparam name="TDestination">Desired destination type.</typeparam>
    /// <returns><see cref="Source"/> mapped to the <typeparamref name="TDestination"/> type.</returns>
    /// <exception cref="UndefinedTypeMappingException">
    /// When mapping from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/> is not defined.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TDestination To<TDestination>()
    {
        return TypeMapper.Map<TSource, TDestination>( Source );
    }

    /// <summary>
    /// Attempts to map the <see cref="Source"/> to the desired <typeparamref name="TDestination"/> type
    /// using the attached <see cref="TypeMapper"/> instance.
    /// </summary>
    /// <param name="result">
    /// <b>out</b> parameter that returns <see cref="Source"/> mapped to the <typeparamref name="TDestination"/> type
    /// if mapping was successful.
    /// </param>
    /// <typeparam name="TDestination">Desired destination type.</typeparam>
    /// <returns><b>true</b> when mapping was successful, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryTo<TDestination>([MaybeNullWhen( false )] out TDestination result)
    {
        return TypeMapper.TryMap( Source, out result );
    }
}
