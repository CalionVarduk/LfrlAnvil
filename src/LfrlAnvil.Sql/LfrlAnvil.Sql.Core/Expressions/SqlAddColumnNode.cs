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
/// Represents an SQL syntax tree statement node that defines an addition of a single column to a table.
/// </summary>
public sealed class SqlAddColumnNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlAddColumnNode(SqlRecordSetInfo table, SqlColumnDefinitionNode definition)
        : base( SqlNodeType.AddColumn )
    {
        Table = table;
        Definition = definition;
    }

    /// <summary>
    /// Source table.
    /// </summary>
    public SqlRecordSetInfo Table { get; }

    /// <summary>
    /// Definition of the column to add.
    /// </summary>
    public SqlColumnDefinitionNode Definition { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
