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
using LfrlAnvil.Functional.Exceptions;

namespace LfrlAnvil.Functional;

/// <summary>
/// Represents a type-erased result of an action that may throw an error.
/// </summary>
public interface IErratic
{
    /// <summary>
    /// Specifies whether or not this instance contains an error.
    /// </summary>
    bool HasError { get; }

    /// <summary>
    /// Specifies whether or not this instance contains a value.
    /// </summary>
    bool IsOk { get; }

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    /// <returns>Underlying value.</returns>
    /// <exception cref="ValueAccessException">When underlying value does not exist.</exception>
    [Pure]
    object GetValue();

    /// <summary>
    /// Gets the underlying value or a default value when it does not exist.
    /// </summary>
    /// <returns>Underlying value or a default value when it does not exist.</returns>
    [Pure]
    object? GetValueOrDefault();

    /// <summary>
    /// Gets the underlying error.
    /// </summary>
    /// <returns>Underlying error.</returns>
    /// <exception cref="ValueAccessException">When underlying error does not exist.</exception>
    [Pure]
    Exception GetError();

    /// <summary>
    /// Gets the underlying error or null when it does not exist.
    /// </summary>
    /// <returns>Underlying error or null when it does not exist.</returns>
    [Pure]
    Exception? GetErrorOrDefault();
}
