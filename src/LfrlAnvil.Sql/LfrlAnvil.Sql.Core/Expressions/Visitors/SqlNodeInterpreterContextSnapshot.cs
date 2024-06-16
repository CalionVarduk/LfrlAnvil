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
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <summary>
/// Represents a snapshot of an <see cref="SqlNodeInterpreterContext"/> state.
/// </summary>
public readonly struct SqlNodeInterpreterContextSnapshot
{
    private readonly string? _sql;
    private readonly SqlParameterNode[]? _parameters;

    internal SqlNodeInterpreterContextSnapshot(SqlNodeInterpreterContext context)
    {
        _sql = context.Sql.ToString();
        _parameters = Array.Empty<SqlParameterNode>();

        var parameters = context.Parameters;
        if ( parameters.Count > 0 )
        {
            var i = 0;
            _parameters = new SqlParameterNode[parameters.Count];
            foreach ( var (name, type, index) in parameters )
                _parameters[i++] = SqlNode.Parameter( name, type, index );
        }
    }

    /// <summary>
    /// Underlying SQL statement.
    /// </summary>
    public string Sql => _sql ?? string.Empty;

    /// <summary>
    /// Collection of SQL parameter nodes.
    /// </summary>
    public ReadOnlySpan<SqlParameterNode> Parameters => _parameters;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlNodeInterpreterContextSnapshot"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return Sql;
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawExpressionNode"/> instance from this snapshot.
    /// </summary>
    /// <param name="type">Optional runtime type of the result of this expression.</param>
    /// <returns>New <see cref="SqlRawExpressionNode"/> instance.</returns>
    [Pure]
    public SqlRawExpressionNode ToExpression(TypeNullability? type = null)
    {
        return SqlNode.RawExpression( Sql, type, _parameters ?? Array.Empty<SqlParameterNode>() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawConditionNode"/> instance from this snapshot.
    /// </summary>
    /// <returns>New <see cref="SqlRawConditionNode"/> instance.</returns>
    [Pure]
    public SqlRawConditionNode ToCondition()
    {
        return SqlNode.RawCondition( Sql, _parameters ?? Array.Empty<SqlParameterNode>() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawStatementNode"/> instance from this snapshot.
    /// </summary>
    /// <returns>New <see cref="SqlRawStatementNode"/> instance.</returns>
    [Pure]
    public SqlRawStatementNode ToStatement()
    {
        return SqlNode.RawStatement( Sql, _parameters ?? Array.Empty<SqlParameterNode>() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawQueryExpressionNode"/> instance from this snapshot.
    /// </summary>
    /// <returns>New <see cref="SqlRawQueryExpressionNode"/> instance.</returns>
    [Pure]
    public SqlRawQueryExpressionNode ToQuery()
    {
        return SqlNode.RawQuery( Sql, _parameters ?? Array.Empty<SqlParameterNode>() );
    }
}
