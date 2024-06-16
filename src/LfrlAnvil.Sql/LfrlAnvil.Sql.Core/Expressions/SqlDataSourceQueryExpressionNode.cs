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
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a type-erased query expression based on <see cref="SqlDataSourceNode"/>.
/// </summary>
public abstract class SqlDataSourceQueryExpressionNode : SqlExtendableQueryExpressionNode
{
    internal SqlDataSourceQueryExpressionNode(Chain<SqlTraitNode> traits)
        : base( SqlNodeType.DataSourceQuery, traits ) { }

    /// <summary>
    /// Underlying data source.
    /// </summary>
    public abstract SqlDataSourceNode DataSource { get; }

    /// <summary>
    /// Creates a new SQL data source query expression node with added <paramref name="selection"/>.
    /// </summary>
    /// <param name="selection">Collection of expressions to add to <see cref="SqlQueryExpressionNode.Selection"/>.</param>
    /// <returns>New SQL data source query expression node.</returns>
    [Pure]
    public abstract SqlDataSourceQueryExpressionNode Select(params SqlSelectNode[] selection);
}

/// <summary>
/// Represents an SQL syntax tree expression node that defines a generic query expression based on <see cref="SqlDataSourceNode"/>.
/// </summary>
/// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
public sealed class SqlDataSourceQueryExpressionNode<TDataSourceNode> : SqlDataSourceQueryExpressionNode
    where TDataSourceNode : SqlDataSourceNode
{
    internal SqlDataSourceQueryExpressionNode(TDataSourceNode dataSource, ReadOnlyArray<SqlSelectNode> selection)
        : base( Chain<SqlTraitNode>.Empty )
    {
        DataSource = dataSource;
        Selection = selection;
    }

    private SqlDataSourceQueryExpressionNode(SqlDataSourceQueryExpressionNode<TDataSourceNode> @base, Chain<SqlTraitNode> traits)
        : base( traits )
    {
        DataSource = @base.DataSource;
        Selection = @base.Selection;
    }

    private SqlDataSourceQueryExpressionNode(
        SqlDataSourceQueryExpressionNode<TDataSourceNode> @base,
        ReadOnlyArray<SqlSelectNode> selection)
        : base( @base.Traits )
    {
        DataSource = @base.DataSource;
        Selection = selection;
    }

    /// <inheritdoc />
    public override TDataSourceNode DataSource { get; }

    /// <inheritdoc />
    public override ReadOnlyArray<SqlSelectNode> Selection { get; }

    /// <inheritdoc />
    [Pure]
    public override SqlDataSourceQueryExpressionNode<TDataSourceNode> Select(params SqlSelectNode[] selection)
    {
        if ( selection.Length == 0 )
            return this;

        var newSelection = new SqlSelectNode[Selection.Count + selection.Length];
        Selection.AsSpan().CopyTo( newSelection );
        selection.CopyTo( newSelection, Selection.Count );
        return new SqlDataSourceQueryExpressionNode<TDataSourceNode>( this, newSelection );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDataSourceQueryExpressionNode<TDataSourceNode> AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDataSourceQueryExpressionNode<TDataSourceNode> SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlDataSourceQueryExpressionNode<TDataSourceNode>( this, traits );
    }
}
