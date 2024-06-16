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

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a query expression that can be decorated with traits.
/// </summary>
public abstract class SqlExtendableQueryExpressionNode : SqlQueryExpressionNode
{
    internal SqlExtendableQueryExpressionNode(SqlNodeType nodeType, Chain<SqlTraitNode> traits)
        : base( nodeType )
    {
        Traits = traits;
    }

    /// <summary>
    /// Collection of decorating traits.
    /// </summary>
    public Chain<SqlTraitNode> Traits { get; }

    /// <summary>
    /// Creates a new SQL query expression syntax tree node by adding a new <paramref name="trait"/>.
    /// </summary>
    /// <param name="trait">Trait to add.</param>
    /// <returns>New SQL query expression syntax tree node.</returns>
    [Pure]
    public abstract SqlExtendableQueryExpressionNode AddTrait(SqlTraitNode trait);

    /// <summary>
    /// Creates a new SQL query expression syntax tree node by changing the <see cref="Traits"/> collection.
    /// </summary>
    /// <param name="traits">Collection of traits to set.</param>
    /// <returns>New SQL query expression syntax tree node.</returns>
    [Pure]
    public abstract SqlExtendableQueryExpressionNode SetTraits(Chain<SqlTraitNode> traits);
}
