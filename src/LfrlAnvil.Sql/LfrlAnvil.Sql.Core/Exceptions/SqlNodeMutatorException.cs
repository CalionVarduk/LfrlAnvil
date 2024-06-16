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
/// Represents an error that occurred due to an invalid node type returned by <see cref="SqlNodeMutatorContext"/>.
/// </summary>
public class SqlNodeMutatorException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="SqlNodeMutatorException"/> instance.
    /// </summary>
    /// <param name="parent">Parent node.</param>
    /// <param name="node">Original node.</param>
    /// <param name="result">Invalid original node's replacement.</param>
    /// <param name="expectedType">Expected node type.</param>
    /// <param name="description">Error description.</param>
    public SqlNodeMutatorException(SqlNodeBase parent, SqlNodeBase node, SqlNodeBase result, Type expectedType, string description)
        : base( ExceptionResources.InvalidNodeMutatorResult( parent, node, result, expectedType, description ) )
    {
        Parent = parent;
        Node = node;
        Result = result;
        ExpectedType = expectedType;
    }

    /// <summary>
    /// Parent node.
    /// </summary>
    public SqlNodeBase Parent { get; }

    /// <summary>
    /// Original node.
    /// </summary>
    public SqlNodeBase Node { get; }

    /// <summary>
    /// Invalid original node's replacement.
    /// </summary>
    public SqlNodeBase Result { get; }

    /// <summary>
    /// Expected node type.
    /// </summary>
    public Type ExpectedType { get; }
}
