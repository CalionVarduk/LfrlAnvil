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
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an aggregate function invocation.
/// </summary>
public abstract class SqlAggregateFunctionExpressionNode : SqlExpressionNode
{
    /// <summary>
    /// Creates a new <see cref="SqlAggregateFunctionExpressionNode"/> instance with <see cref="SqlFunctionType.Custom"/> type.
    /// </summary>
    /// <param name="arguments">Sequential collection of invocation arguments.</param>
    /// <param name="traits">Collection of decorating traits.</param>
    protected SqlAggregateFunctionExpressionNode(ReadOnlyArray<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : this( SqlFunctionType.Custom, arguments, traits ) { }

    internal SqlAggregateFunctionExpressionNode(
        SqlFunctionType functionType,
        ReadOnlyArray<SqlExpressionNode> arguments,
        Chain<SqlTraitNode> traits)
        : base( SqlNodeType.AggregateFunctionExpression )
    {
        Assume.IsDefined( functionType );
        FunctionType = functionType;
        Arguments = arguments;
        Traits = traits;
    }

    /// <summary>
    /// <see cref="SqlFunctionType"/> of this aggregate function.
    /// </summary>
    public SqlFunctionType FunctionType { get; }

    /// <summary>
    /// Sequential collection of invocation arguments.
    /// </summary>
    public ReadOnlyArray<SqlExpressionNode> Arguments { get; }

    /// <summary>
    /// Collection of decorating traits.
    /// </summary>
    public Chain<SqlTraitNode> Traits { get; }

    /// <summary>
    /// Creates a new SQL aggregate function invocation expression syntax tree node by adding a new <paramref name="trait"/>.
    /// </summary>
    /// <param name="trait">Trait to add.</param>
    /// <returns>New SQL aggregate function invocation expression syntax tree node.</returns>
    [Pure]
    public virtual SqlAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <summary>
    /// Creates a new SQL aggregate function invocation expression syntax tree node by changing the <see cref="Traits"/> collection.
    /// </summary>
    /// <param name="traits">Collection of traits to set.</param>
    /// <returns>New SQL aggregate function invocation expression syntax tree node.</returns>
    [Pure]
    public abstract SqlAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits);
}
