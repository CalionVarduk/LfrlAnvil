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
/// Represents an SQL syntax tree node that defines a foreign key constraint.
/// </summary>
public sealed class SqlForeignKeyDefinitionNode : SqlNodeBase
{
    internal SqlForeignKeyDefinitionNode(
        SqlSchemaObjectName name,
        SqlDataFieldNode[] columns,
        SqlRecordSetNode referencedTable,
        SqlDataFieldNode[] referencedColumns,
        ReferenceBehavior onDeleteBehavior,
        ReferenceBehavior onUpdateBehavior)
        : base( SqlNodeType.ForeignKeyDefinition )
    {
        Name = name;
        Columns = columns;
        ReferencedTable = referencedTable;
        ReferencedColumns = referencedColumns;
        OnDeleteBehavior = onDeleteBehavior;
        OnUpdateBehavior = onUpdateBehavior;
    }

    /// <summary>
    /// Foreign key constraint's name.
    /// </summary>
    public SqlSchemaObjectName Name { get; }

    /// <summary>
    /// Collection of columns from source table that this foreign key originates from.
    /// </summary>
    public ReadOnlyArray<SqlDataFieldNode> Columns { get; }

    /// <summary>
    /// Table referenced by this foreign key constraint.
    /// </summary>
    public SqlRecordSetNode ReferencedTable { get; }

    /// <summary>
    /// Collection of columns from <see cref="ReferencedTable"/> referenced by this foreign key constraint.
    /// </summary>
    public ReadOnlyArray<SqlDataFieldNode> ReferencedColumns { get; }

    /// <summary>
    /// Specifies this foreign key constraint's on delete behavior.
    /// </summary>
    public ReferenceBehavior OnDeleteBehavior { get; }

    /// <summary>
    /// Specifies this foreign key constraint's on update behavior.
    /// </summary>
    public ReferenceBehavior OnUpdateBehavior { get; }
}
