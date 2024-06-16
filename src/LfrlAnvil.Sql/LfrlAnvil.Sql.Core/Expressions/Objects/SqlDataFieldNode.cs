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
/// Represents an SQL syntax tree expression node that defines a single data field of a record set.
/// </summary>
public abstract class SqlDataFieldNode : SqlExpressionNode
{
    internal SqlDataFieldNode(SqlRecordSetNode recordSet, SqlNodeType nodeType)
        : base( nodeType )
    {
        RecordSet = recordSet;
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataFieldNode"/> instance of <see cref="SqlNodeType.Unknown"/> type.
    /// </summary>
    /// <param name="recordSet"><see cref="SqlRecordSetNode"/> that this data field belongs to.</param>
    protected SqlDataFieldNode(SqlRecordSetNode recordSet)
    {
        RecordSet = recordSet;
    }

    /// <summary>
    /// <see cref="SqlRecordSetNode"/> that this data field belongs to.
    /// </summary>
    public SqlRecordSetNode RecordSet { get; }

    /// <summary>
    /// Name of this data field.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Creates a new SQL data field node with changed <see cref="RecordSet"/>.
    /// </summary>
    /// <param name="recordSet">Record set to set.</param>
    /// <returns>New SQL data field node.</returns>
    [Pure]
    public abstract SqlDataFieldNode ReplaceRecordSet(SqlRecordSetNode recordSet);

    /// <summary>
    /// Converts the <paramref name="node"/> to <see cref="SqlSelectFieldNode"/> type.
    /// </summary>
    /// <param name="node">Node to convert.</param>
    /// <returns>New <see cref="SqlSelectFieldNode"/> instance.</returns>
    [Pure]
    public static implicit operator SqlSelectFieldNode(SqlDataFieldNode node)
    {
        return node.AsSelf();
    }
}
