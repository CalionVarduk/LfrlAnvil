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
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Exceptions;

/// <summary>
/// Represents an error that occurred due to an <see cref="ISqlNodeVisitor"/> instance encountering an unrecognized type of node.
/// </summary>
public class UnrecognizedSqlNodeException : NotSupportedException
{
    /// <summary>
    /// Creates a new <see cref="UnrecognizedSqlNodeException"/> instance.
    /// </summary>
    /// <param name="visitor">SQL node visitor that failed.</param>
    /// <param name="node">Unrecognized node.</param>
    public UnrecognizedSqlNodeException(ISqlNodeVisitor visitor, SqlNodeBase node)
        : base( ExceptionResources.UnrecognizedSqlNode( visitor.GetType(), node ) )
    {
        Visitor = visitor;
        Node = node;
    }

    /// <summary>
    /// SQL node visitor that failed.
    /// </summary>
    public ISqlNodeVisitor Visitor { get; }

    /// <summary>
    /// Unrecognized node.
    /// </summary>
    public SqlNodeBase Node { get; }
}
