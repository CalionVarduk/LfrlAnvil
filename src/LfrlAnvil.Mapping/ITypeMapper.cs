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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

/// <summary>
/// Represents an object capable of mapping objects to different types.
/// </summary>
public interface ITypeMapper
{
    /// <summary>
    /// Attempts to map the provided <paramref name="source"/> of <typeparamref name="TSource"/> type
    /// to the desired <typeparamref name="TDestination"/> type.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="result">
    /// <b>out</b> parameter that returns <paramref name="source"/> mapped to the <typeparamref name="TDestination"/> type
    /// if mapping was successful.
    /// </param>
    /// <typeparam name="TSource">Source object type.</typeparam>
    /// <typeparam name="TDestination">Desired destination type.</typeparam>
    /// <returns><b>true</b> when mapping was successful, otherwise <b>false</b>.</returns>
    bool TryMap<TSource, TDestination>(TSource source, [MaybeNullWhen( false )] out TDestination result);

    /// <summary>
    /// Attempts to map the provided <paramref name="source"/> to the desired <typeparamref name="TDestination"/> type.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="result">
    /// <b>out</b> parameter that returns <paramref name="source"/> mapped to the <typeparamref name="TDestination"/> type
    /// if mapping was successful.
    /// </param>
    /// <typeparam name="TDestination">Desired destination type.</typeparam>
    /// <returns><b>true</b> when mapping was successful, otherwise <b>false</b>.</returns>
    bool TryMap<TDestination>(object source, [MaybeNullWhen( false )] out TDestination result);

    /// <summary>
    /// Attempts to map the provided <paramref name="source"/> to the desired <paramref name="destinationType"/> type.
    /// </summary>
    /// <param name="destinationType">Desired destination type.</param>
    /// <param name="source">Source object.</param>
    /// <param name="result">
    /// <b>out</b> parameter that returns <paramref name="source"/> mapped to the <paramref name="destinationType"/> type
    /// if mapping was successful.
    /// </param>
    /// <returns><b>true</b> when mapping was successful, otherwise <b>false</b>.</returns>
    bool TryMap(Type destinationType, object source, [MaybeNullWhen( false )] out object result);

    /// <summary>
    /// Attempts to map the provided <paramref name="source"/> collection with elements of <typeparamref name="TSource"/> type
    /// to a collection with elements of the desired <typeparamref name="TDestination"/> type.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="result">
    /// <b>out</b> parameter that returns <paramref name="source"/> collection mapped to collection with elements
    /// of the <typeparamref name="TDestination"/> type if mapping was successful.
    /// </param>
    /// <typeparam name="TSource">Source collection's element type.</typeparam>
    /// <typeparam name="TDestination">Desired destination collection's element type.</typeparam>
    /// <returns><b>true</b> when mapping was successful, otherwise <b>false</b>.</returns>
    bool TryMapMany<TSource, TDestination>(IEnumerable<TSource> source, [MaybeNullWhen( false )] out IEnumerable<TDestination> result);

    /// <summary>
    /// Checks whether or not the mapping definition from <paramref name="sourceType"/> to <paramref name="destinationType"/> exists.
    /// </summary>
    /// <param name="sourceType">Source type.</param>
    /// <param name="destinationType">Destination type.</param>
    /// <returns><b>true</b> when mapping definition exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool IsConfigured(Type sourceType, Type destinationType);

    /// <summary>
    /// Returns all defined (source-type, destination-type) mappings.
    /// </summary>
    /// <returns>All defined (source-type, destination-type) mappings.</returns>
    [Pure]
    IEnumerable<TypeMappingKey> GetConfiguredMappings();
}
