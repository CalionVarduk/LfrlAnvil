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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL column computation.
/// </summary>
/// <param name="Expression">Computation's expression.</param>
/// <param name="Storage">Computation's storage type.</param>
public readonly record struct SqlColumnComputation(SqlExpressionNode Expression, SqlColumnComputationStorage Storage)
{
    /// <summary>
    /// Creates a new <see cref="SqlColumnComputation"/> instance with <see cref="SqlColumnComputationStorage.Virtual"/> storage type.
    /// </summary>
    /// <param name="expression">Computation's expression.</param>
    /// <returns>New <see cref="SqlColumnComputation"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnComputation Virtual(SqlExpressionNode expression)
    {
        return new SqlColumnComputation( expression, SqlColumnComputationStorage.Virtual );
    }

    /// <summary>
    /// Creates a new <see cref="SqlColumnComputation"/> instance with <see cref="SqlColumnComputationStorage.Stored"/> storage type.
    /// </summary>
    /// <param name="expression">Computation's expression.</param>
    /// <returns>New <see cref="SqlColumnComputation"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnComputation Stored(SqlExpressionNode expression)
    {
        return new SqlColumnComputation( expression, SqlColumnComputationStorage.Stored );
    }
}
