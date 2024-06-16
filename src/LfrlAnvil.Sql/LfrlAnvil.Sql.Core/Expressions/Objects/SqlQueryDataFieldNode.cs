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

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a single data field of a query record set.
/// </summary>
public sealed class SqlQueryDataFieldNode : SqlDataFieldNode
{
    internal SqlQueryDataFieldNode(SqlRecordSetNode recordSet, string name, SqlSelectNode selection, SqlExpressionNode? expression)
        : base( recordSet, SqlNodeType.QueryDataField )
    {
        Name = name;
        Selection = selection;
        Expression = expression;
    }

    /// <summary>
    /// Source selection.
    /// </summary>
    public SqlSelectNode Selection { get; }

    /// <summary>
    /// Expression associated with this data field.
    /// </summary>S
    public SqlExpressionNode? Expression { get; }

    /// <inheritdoc />
    public override string Name { get; }

    /// <inheritdoc />
    [Pure]
    public override SqlQueryDataFieldNode ReplaceRecordSet(SqlRecordSetNode recordSet)
    {
        return new SqlQueryDataFieldNode( recordSet, Name, Selection, Expression );
    }
}
