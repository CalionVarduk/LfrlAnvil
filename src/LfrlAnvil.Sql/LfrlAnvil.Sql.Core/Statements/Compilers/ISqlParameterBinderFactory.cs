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
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents a factory of delegates used by <see cref="SqlParameterBinderExpression"/> instances.
/// </summary>
public interface ISqlParameterBinderFactory
{
    /// <summary>
    /// SQL dialect that this factory is associated with.
    /// </summary>
    SqlDialect Dialect { get; }

    /// <summary>
    /// Specifies whether or not this factory supports positional parameters.
    /// </summary>
    bool SupportsPositionalParameters { get; }

    /// <summary>
    /// Creates a new <see cref="SqlParameterBinder"/> instance.
    /// </summary>
    /// <param name="options">Optional <see cref="SqlParameterBinderCreationOptions"/>.</param>
    /// <returns>New <see cref="SqlParameterBinder"/> instance.</returns>
    [Pure]
    SqlParameterBinder Create(SqlParameterBinderCreationOptions? options = null);

    /// <summary>
    /// Creates a new <see cref="SqlParameterBinderExpression"/> instance.
    /// </summary>
    /// <param name="sourceType">Parameter source type.</param>
    /// <param name="options">Optional <see cref="SqlParameterBinderCreationOptions"/>.</param>
    /// <returns>New <see cref="SqlParameterBinderExpression"/> instance.</returns>
    /// <exception cref="SqlCompilerException">
    /// When <paramref name="sourceType"/> is not a valid parameter source type or does not contain any valid members.
    /// </exception>
    [Pure]
    SqlParameterBinderExpression CreateExpression(Type sourceType, SqlParameterBinderCreationOptions? options = null);
}
