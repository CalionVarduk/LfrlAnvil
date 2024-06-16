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
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Generators;

/// <summary>
/// Represents a type-erased generator of objects.
/// </summary>
public interface IGenerator
{
    /// <summary>
    /// Generates a new object.
    /// </summary>
    /// <returns>Generated object.</returns>
    /// <exception cref="ValueGenerationException">When object could not be generated.</exception>
    object? Generate();

    /// <summary>
    /// Attempts to generate a new object.
    /// </summary>
    /// <param name="result"><b>out</b> parameter that returns generated object, if successful.</param>
    /// <returns><b>true</b> when object was generated successfully, otherwise <b>false</b>.</returns>
    bool TryGenerate(out object? result);
}

/// <summary>
/// Represents a generic generator of objects.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
public interface IGenerator<T> : IGenerator
{
    /// <summary>
    /// Generates a new object.
    /// </summary>
    /// <returns>Generated object.</returns>
    /// <exception cref="ValueGenerationException">When object could not be generated.</exception>
    new T Generate();

    /// <summary>
    /// Attempts to generate a new object.
    /// </summary>
    /// <param name="result"><b>out</b> parameter that returns generated object, if successful.</param>
    /// <returns><b>true</b> when object was generated successfully, otherwise <b>false</b>.</returns>
    bool TryGenerate([MaybeNullWhen( false )] out T result);
}
