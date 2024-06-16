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
/// Represents an error that occurred due to an <see cref="ISqlNodeVisitor"/> instance being unable to handle a node.
/// </summary>
public class SqlNodeVisitorException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="SqlNodeVisitorException"/> instance.
    /// </summary>
    /// <param name="reason">Description of a reason behind this error.</param>
    /// <param name="visitor">SQL node visitor that failed.</param>
    /// <param name="node">Node that caused the failure.</param>
    public SqlNodeVisitorException(string reason, ISqlNodeVisitor visitor, SqlNodeBase node)
        : base( ExceptionResources.FailedWhileVisitingNode( reason, visitor.GetType(), node ) )
    {
        Visitor = visitor;
        Node = node;
    }

    /// <summary>
    /// SQL node visitor that failed.
    /// </summary>
    public ISqlNodeVisitor Visitor { get; }

    /// <summary>
    /// Node that caused the failure.
    /// </summary>
    public SqlNodeBase Node { get; }
}
