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

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree select node that defines a single expression selection
/// for an <see cref="SqlCompoundQueryExpressionNode"/>.
/// </summary>
public sealed class SqlSelectCompoundFieldNode : SqlSelectNode
{
    internal SqlSelectCompoundFieldNode(string name, Origin[] origins)
        : base( SqlNodeType.SelectCompoundField )
    {
        Name = name;
        Origins = origins;
    }

    /// <summary>
    /// Name of this selected field.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Collection of selections from queries that compose an <see cref="SqlCompoundQueryExpressionNode"/> instance,
    /// which is the source of this field.
    /// </summary>
    public ReadOnlyArray<Origin> Origins { get; }

    internal override void VisitExpressions(ISqlSelectNodeExpressionVisitor visitor)
    {
        visitor.Handle( Name, null );
    }

    /// <summary>
    /// Represents a selection from a single query that is a component of an <see cref="SqlCompoundQueryExpressionNode"/>.
    /// </summary>
    /// <param name="QueryIndex">0-based index of the source query within the compound query expression.</param>
    /// <param name="Selection">Selection from the source query.</param>
    /// <param name="Expression">Expression associated with a data field from the <see cref="Selection"/> from the source query.</param>
    public readonly record struct Origin(int QueryIndex, SqlSelectNode Selection, SqlExpressionNode? Expression);
}
