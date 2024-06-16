﻿// Copyright 2024 Łukasz Furlepa
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
/// Represents an SQL syntax tree statement node that defines a removal of a single view.
/// </summary>
public sealed class SqlDropViewNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlDropViewNode(SqlRecordSetInfo view, bool ifExists)
        : base( SqlNodeType.DropView )
    {
        View = view;
        IfExists = ifExists;
    }

    /// <summary>
    /// View's name.
    /// </summary>
    public SqlRecordSetInfo View { get; }

    /// <summary>
    /// Specifies whether or not the removal attempt should only be made if this view exists in DB.
    /// </summary>
    public bool IfExists { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
