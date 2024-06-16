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

using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree select node that defines a single expression selection.
/// </summary>
public sealed class SqlSelectFieldNode : SqlSelectNode
{
    internal SqlSelectFieldNode(SqlExpressionNode expression, string? alias)
        : base( SqlNodeType.SelectField )
    {
        Expression = expression;
        Alias = alias;
    }

    /// <summary>
    /// Selected expression.
    /// </summary>
    public SqlExpressionNode Expression { get; }

    /// <summary>
    /// Optional alias of the selected <see cref="Expression"/>.
    /// </summary>
    public string? Alias { get; }

    /// <summary>
    /// Name of this selected field.
    /// </summary>
    /// <remarks>
    /// Equal to <see cref="Alias"/> when it is not null,
    /// otherwise equal to <see cref="SqlDataFieldNode.Name"/> of the underlying <see cref="SqlDataFieldNode"/> <see cref="Expression"/>.
    /// </remarks>
    public string FieldName => Alias ?? ReinterpretCast.To<SqlDataFieldNode>( Expression ).Name;

    internal override void VisitExpressions(ISqlSelectNodeExpressionVisitor visitor)
    {
        visitor.Handle( FieldName, Expression );
    }
}
