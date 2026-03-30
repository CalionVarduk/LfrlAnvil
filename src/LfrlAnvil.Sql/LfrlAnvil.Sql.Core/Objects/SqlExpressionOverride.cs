// Copyright 2026 Łukasz Furlepa
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
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents a default SQL expression node override.
/// </summary>
public readonly record struct SqlExpressionOverride
{
    private SqlExpressionOverride(SqlExpressionNode? node, bool isIgnored)
    {
        Node = node;
        IsIgnored = isIgnored;
    }

    /// <summary>
    /// Node to use instead of a default value.
    /// </summary>
    public readonly SqlExpressionNode? Node;

    /// <summary>
    /// Specifies whether the expression should be completely ignored.
    /// </summary>
    public readonly bool IsIgnored;

    /// <summary>
    /// Represents usage of a default value.
    /// </summary>
    public static SqlExpressionOverride UseDefault => new SqlExpressionOverride( null, false );

    /// <summary>
    /// Represents an ignored expression.
    /// </summary>
    public static SqlExpressionOverride Ignore => new SqlExpressionOverride( null, true );

    /// <summary>
    /// Creates an override using a custom <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Node to use instead of a default value.</param>
    /// <returns>New <see cref="SqlExpressionOverride"/> instance.</returns>
    [Pure]
    public static implicit operator SqlExpressionOverride(SqlExpressionNode node)
    {
        return new SqlExpressionOverride( node, false );
    }
}
