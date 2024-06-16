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
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines creation of a single table.
/// </summary>
public sealed class SqlCreateTableNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlCreateTableNode(
        SqlRecordSetInfo info,
        bool ifNotExists,
        SqlColumnDefinitionNode[] columns,
        Func<SqlNewTableNode, SqlCreateTableConstraints>? constraintsProvider)
        : base( SqlNodeType.CreateTable )
    {
        Info = info;
        IfNotExists = ifNotExists;
        Columns = columns;
        RecordSet = new SqlNewTableNode( this, alias: null, isOptional: false );
        if ( constraintsProvider is not null )
        {
            var constraints = constraintsProvider( RecordSet );
            PrimaryKey = constraints.PrimaryKey;
            ForeignKeys = constraints.ForeignKeys ?? ReadOnlyArray<SqlForeignKeyDefinitionNode>.Empty;
            Checks = constraints.Checks ?? ReadOnlyArray<SqlCheckDefinitionNode>.Empty;
        }
        else
        {
            PrimaryKey = null;
            ForeignKeys = ReadOnlyArray<SqlForeignKeyDefinitionNode>.Empty;
            Checks = ReadOnlyArray<SqlCheckDefinitionNode>.Empty;
        }
    }

    /// <summary>
    /// Table's name.
    /// </summary>
    public SqlRecordSetInfo Info { get; }

    /// <summary>
    /// Specifies whether or not this table should only be created if it does not already exist in DB.
    /// </summary>
    public bool IfNotExists { get; }

    /// <summary>
    /// Collection of columns.
    /// </summary>
    public ReadOnlyArray<SqlColumnDefinitionNode> Columns { get; }

    /// <summary>
    /// Optional primary key constraint.
    /// </summary>
    public SqlPrimaryKeyDefinitionNode? PrimaryKey { get; }

    /// <summary>
    /// Collection of foreign key constraints.
    /// </summary>
    public ReadOnlyArray<SqlForeignKeyDefinitionNode> ForeignKeys { get; }

    /// <summary>
    /// Collection of check constraints.
    /// </summary>
    public ReadOnlyArray<SqlCheckDefinitionNode> Checks { get; }

    /// <summary>
    /// Underlying <see cref="SqlRecordSetNode"/> instance associated with this node.
    /// </summary>
    public SqlNewTableNode RecordSet { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
