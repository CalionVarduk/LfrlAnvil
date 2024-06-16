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

using System.Data;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines a start of a DB transaction.
/// </summary>
public sealed class SqlBeginTransactionNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlBeginTransactionNode(IsolationLevel isolationLevel)
        : base( SqlNodeType.BeginTransaction )
    {
        IsolationLevel = isolationLevel;
    }

    /// <summary>
    /// Transaction's <see cref="System.Data.IsolationLevel"/>.
    /// </summary>
    public IsolationLevel IsolationLevel { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
