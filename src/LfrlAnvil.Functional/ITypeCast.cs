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
using LfrlAnvil.Functional.Exceptions;

namespace LfrlAnvil.Functional;

/// <summary>
/// Represents a type-erased result of a type cast.
/// </summary>
/// <typeparam name="TDestination">Destination type.</typeparam>
public interface ITypeCast<out TDestination> : IReadOnlyCollection<TDestination>
{
    /// <summary>
    /// Underlying source object.
    /// </summary>
    object? Source { get; }

    /// <summary>
    /// Specifies whether or not this type cast is valid.
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Specifies whether or not this type cast is invalid.
    /// </summary>
    bool IsInvalid { get; }

    /// <summary>
    /// Gets the underlying type cast result.
    /// </summary>
    /// <returns>Underlying type cast result.</returns>
    /// <exception cref="ValueAccessException">When underlying type cast result does not exist.</exception>
    [Pure]
    TDestination GetResult();

    /// <summary>
    /// Gets the underlying type cast result or a default value when it does not exist.
    /// </summary>
    /// <returns>Underlying type cast result or a default value when it does not exist.</returns>
    [Pure]
    TDestination? GetResultOrDefault();
}
