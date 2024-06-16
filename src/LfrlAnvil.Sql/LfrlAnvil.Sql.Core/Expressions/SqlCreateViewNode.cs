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
/// Represents an SQL syntax tree statement node that defines creation of a single view.
/// </summary>
public sealed class SqlCreateViewNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlCreateViewNode(SqlRecordSetInfo info, bool replaceIfExists, SqlQueryExpressionNode source)
        : base( SqlNodeType.CreateView )
    {
        Info = info;
        ReplaceIfExists = replaceIfExists;
        Source = source;
    }

    /// <summary>
    /// View's name.
    /// </summary>
    public SqlRecordSetInfo Info { get; }

    /// <summary>
    /// Specifies whether or not the view should be replaced if it already exists in DB.
    /// </summary>
    public bool ReplaceIfExists { get; }

    /// <summary>
    /// Underlying source query expression that defines this view.
    /// </summary>
    public SqlQueryExpressionNode Source { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
