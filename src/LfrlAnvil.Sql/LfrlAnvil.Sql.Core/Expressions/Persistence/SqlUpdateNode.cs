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

namespace LfrlAnvil.Sql.Expressions.Persistence;

/// <summary>
/// Represents an SQL syntax tree statement node that defines an update of existing records in a data source.
/// </summary>
public sealed class SqlUpdateNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlUpdateNode(SqlDataSourceNode dataSource, SqlValueAssignmentNode[] assignments)
        : base( SqlNodeType.Update )
    {
        DataSource = dataSource;
        Assignments = assignments;
    }

    /// <summary>
    /// Data source that defines records to be updated.
    /// </summary>
    /// <remarks>Records will be updated in the <see cref="SqlDataSourceNode.From"/> record set.</remarks>
    public SqlDataSourceNode DataSource { get; }

    /// <summary>
    /// Collection of value assignments that this update refers to.
    /// </summary>
    public ReadOnlyArray<SqlValueAssignmentNode> Assignments { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
