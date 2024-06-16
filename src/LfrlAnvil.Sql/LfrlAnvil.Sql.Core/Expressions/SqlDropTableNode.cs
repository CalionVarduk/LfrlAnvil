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
/// Represents an SQL syntax tree statement node that defines a removal of a single table.
/// </summary>
public sealed class SqlDropTableNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlDropTableNode(SqlRecordSetInfo table, bool ifExists)
        : base( SqlNodeType.DropTable )
    {
        Table = table;
        IfExists = ifExists;
    }

    /// <summary>
    /// Table's name.
    /// </summary>
    public SqlRecordSetInfo Table { get; }

    /// <summary>
    /// Specifies whether or not the removal attempt should only be made if this table exists in DB.
    /// </summary>
    public bool IfExists { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
