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
/// Represents an SQL syntax tree expression node that defines an invocation of a custom aggregate function.
/// </summary>
public sealed class SqlNamedAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlNamedAggregateFunctionExpressionNode(
        SqlSchemaObjectName name,
        ReadOnlyArray<SqlExpressionNode> arguments,
        Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Named, arguments, traits )
    {
        Name = name;
    }

    /// <summary>
    /// Aggregate function's name.
    /// </summary>
    public SqlSchemaObjectName Name { get; }

    /// <inheritdoc />
    [Pure]
    public override SqlNamedAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlNamedAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlNamedAggregateFunctionExpressionNode( Name, Arguments, traits );
    }
}
