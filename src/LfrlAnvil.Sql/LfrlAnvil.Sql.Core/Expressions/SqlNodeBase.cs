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

using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree node.
/// </summary>
public abstract class SqlNodeBase
{
    /// <summary>
    /// Creates a new <see cref="SqlNodeBase"/> instance of <see cref="SqlNodeType.Unknown"/> type.
    /// </summary>
    protected SqlNodeBase()
        : this( SqlNodeType.Unknown ) { }

    internal SqlNodeBase(SqlNodeType nodeType)
    {
        Assume.IsDefined( nodeType );
        NodeType = nodeType;
    }

    /// <summary>
    /// Specifies the type of this node.
    /// </summary>
    public SqlNodeType NodeType { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="SqlNodeBase"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public sealed override string ToString()
    {
        var interpreter = new SqlNodeDebugInterpreter();
        ToString( interpreter );
        return interpreter.Context.Sql.ToString();
    }

    /// <summary>
    /// Interpreters this node in order to create its string representation.
    /// </summary>
    /// <param name="interpreter">SQL node debug interpreter to use.</param>
    protected virtual void ToString(SqlNodeDebugInterpreter interpreter)
    {
        interpreter.Visit( this );
    }
}
