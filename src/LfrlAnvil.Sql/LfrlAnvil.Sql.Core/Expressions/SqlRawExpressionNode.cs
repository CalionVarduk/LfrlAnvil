﻿// Copyright 2024 Łukasz Furlepa
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

using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a raw SQL expression.
/// </summary>
public sealed class SqlRawExpressionNode : SqlExpressionNode
{
    internal SqlRawExpressionNode(string sql, TypeNullability? type, SqlParameterNode[] parameters)
        : base( SqlNodeType.RawExpression )
    {
        Sql = sql;
        Type = type;
        Parameters = parameters;
    }

    /// <summary>
    /// Raw SQL expression.
    /// </summary>
    public string Sql { get; }

    /// <summary>
    /// Collection of parameter nodes.
    /// </summary>
    public ReadOnlyArray<SqlParameterNode> Parameters { get; }

    /// <summary>
    /// Optional runtime type of the result of this expression.
    /// </summary>
    public TypeNullability? Type { get; }
}
