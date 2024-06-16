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

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a function invocation.
/// </summary>
public abstract class SqlFunctionExpressionNode : SqlExpressionNode
{
    /// <summary>
    /// Creates a new <see cref="SqlFunctionExpressionNode"/> instance with <see cref="SqlFunctionType.Custom"/> type.
    /// </summary>
    /// <param name="arguments">Sequential collection of invocation arguments.</param>
    protected SqlFunctionExpressionNode(SqlExpressionNode[] arguments)
        : this( SqlFunctionType.Custom, arguments ) { }

    internal SqlFunctionExpressionNode(SqlFunctionType functionType, SqlExpressionNode[] arguments)
        : base( SqlNodeType.FunctionExpression )
    {
        Assume.IsDefined( functionType );
        FunctionType = functionType;
        Arguments = arguments;
    }

    /// <summary>
    /// Sequential collection of invocation arguments.
    /// </summary>
    public ReadOnlyArray<SqlExpressionNode> Arguments { get; }

    /// <summary>
    /// <see cref="SqlFunctionType"/> of this function.
    /// </summary>
    public SqlFunctionType FunctionType { get; }
}
