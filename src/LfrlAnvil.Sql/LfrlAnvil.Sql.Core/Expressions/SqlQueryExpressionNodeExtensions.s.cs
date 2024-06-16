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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Contains <see cref="SqlQueryExpressionNode"/> extension methods.
/// </summary>
public static class SqlQueryExpressionNodeExtensions
{
    /// <summary>
    /// Creates a new <see cref="SqlQueryRecordSetNode"/> instance.
    /// </summary>
    /// <param name="node">Underlying <see cref="SqlQueryExpressionNode"/> instance.</param>
    /// <param name="alias">Alias of this record set.</param>
    /// <returns>New <see cref="SqlQueryRecordSetNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryRecordSetNode AsSet(this SqlQueryExpressionNode node, string alias)
    {
        return SqlNode.QueryRecordSet( node, alias );
    }

    /// <summary>
    /// Creates a new <see cref="SqlOrdinalCommonTableExpressionNode"/> instance.
    /// </summary>
    /// <param name="query">Underlying query that defines this common table expression.</param>
    /// <param name="name">Name of this common table expression.</param>
    /// <returns>New <see cref="SqlOrdinalCommonTableExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrdinalCommonTableExpressionNode ToCte(this SqlQueryExpressionNode query, string name)
    {
        return SqlNode.OrdinalCommonTableExpression( query, name );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRecursiveCommonTableExpressionNode"/> instance.
    /// </summary>
    /// <param name="query">Underlying query that defines this common table expression.</param>
    /// <param name="name">Name of this common table expression.</param>
    /// <returns>New <see cref="SqlRecursiveCommonTableExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecursiveCommonTableExpressionNode ToRecursiveCte(this SqlCompoundQueryExpressionNode query, string name)
    {
        return SqlNode.RecursiveCommonTableExpression( query, name );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRecursiveCommonTableExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">
    /// Ordinal common table expression that defines the non-recursive portion of the recursive common table expression.
    /// </param>
    /// <param name="components">
    /// Provider of collection of recursive queries that sequentially follow after the first non-recursive query
    /// defined by the ordinal common table expression.
    /// </param>
    /// <returns>New <see cref="SqlRecursiveCommonTableExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecursiveCommonTableExpressionNode ToRecursive(
        this SqlOrdinalCommonTableExpressionNode node,
        Func<SqlCommonTableExpressionRecordSetNode, IEnumerable<SqlCompoundQueryComponentNode>> components)
    {
        return node.ToRecursive( components( node.RecordSet ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRecursiveCommonTableExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">
    /// Ordinal common table expression that defines the non-recursive portion of the recursive common table expression.
    /// </param>
    /// <param name="components">
    /// Collection of recursive queries that sequentially follow after the first non-recursive query
    /// defined by the ordinal common table expression.
    /// </param>
    /// <returns>New <see cref="SqlRecursiveCommonTableExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecursiveCommonTableExpressionNode ToRecursive(
        this SqlOrdinalCommonTableExpressionNode node,
        IEnumerable<SqlCompoundQueryComponentNode> components)
    {
        return node.ToRecursive( components.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRecursiveCommonTableExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">
    /// Ordinal common table expression that defines the non-recursive portion of the recursive common table expression.
    /// </param>
    /// <param name="components">
    /// Collection of recursive queries that sequentially follow after the first non-recursive query
    /// defined by the ordinal common table expression.
    /// </param>
    /// <returns>New <see cref="SqlRecursiveCommonTableExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlRecursiveCommonTableExpressionNode ToRecursive(
        this SqlOrdinalCommonTableExpressionNode node,
        params SqlCompoundQueryComponentNode[] components)
    {
        var query = new SqlCompoundQueryExpressionNode( node.Query, components );
        return query.ToRecursiveCte( node.Name );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCompoundQueryComponentNode"/> instance with <see cref="SqlCompoundQueryOperator.Union"/> operator.
    /// </summary>
    /// <param name="node">Underlying query.</param>
    /// <returns>New <see cref="SqlCompoundQueryComponentNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCompoundQueryComponentNode ToUnion(this SqlQueryExpressionNode node)
    {
        return SqlNode.UnionWith( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCompoundQueryComponentNode"/> instance with <see cref="SqlCompoundQueryOperator.UnionAll"/> operator.
    /// </summary>
    /// <param name="node">Underlying query.</param>
    /// <returns>New <see cref="SqlCompoundQueryComponentNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCompoundQueryComponentNode ToUnionAll(this SqlQueryExpressionNode node)
    {
        return SqlNode.UnionAllWith( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCompoundQueryComponentNode"/> instance with <see cref="SqlCompoundQueryOperator.Intersect"/> operator.
    /// </summary>
    /// <param name="node">Underlying query.</param>
    /// <returns>New <see cref="SqlCompoundQueryComponentNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCompoundQueryComponentNode ToIntersect(this SqlQueryExpressionNode node)
    {
        return SqlNode.IntersectWith( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCompoundQueryComponentNode"/> instance with <see cref="SqlCompoundQueryOperator.Except"/> operator.
    /// </summary>
    /// <param name="node">Underlying query.</param>
    /// <returns>New <see cref="SqlCompoundQueryComponentNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCompoundQueryComponentNode ToExcept(this SqlQueryExpressionNode node)
    {
        return SqlNode.ExceptWith( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCompoundQueryComponentNode"/> instance.
    /// </summary>
    /// <param name="node">Underlying query.</param>
    /// <param name="operator">Compound query operator with which this query should be included.</param>
    /// <returns>New <see cref="SqlCompoundQueryComponentNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCompoundQueryComponentNode ToCompound(this SqlQueryExpressionNode node, SqlCompoundQueryOperator @operator)
    {
        return SqlNode.CompoundWith( @operator, node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCompoundQueryExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First underlying query.</param>
    /// <param name="followingQueries">Collection of queries that sequentially follow after the first query.</param>
    /// <returns>New <see cref="SqlCompoundQueryExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCompoundQueryExpressionNode CompoundWith(
        this SqlQueryExpressionNode node,
        IEnumerable<SqlCompoundQueryComponentNode> followingQueries)
    {
        return node.CompoundWith( followingQueries.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCompoundQueryExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First underlying query.</param>
    /// <param name="followingQueries">Collection of queries that sequentially follow after the first query.</param>
    /// <returns>New <see cref="SqlCompoundQueryExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCompoundQueryExpressionNode CompoundWith(
        this SqlQueryExpressionNode node,
        params SqlCompoundQueryComponentNode[] followingQueries)
    {
        return SqlNode.CompoundQuery( node, followingQueries );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlDistinctTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Distinct<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AddTrait( SqlNode.DistinctTrait() );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlFilterTraitNode"/>
    /// with <see cref="SqlFilterTraitNode.IsConjunction"/> set to <b>true</b>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="filter">Underlying predicate provider.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> AndWhere<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AndWhere( filter( node.DataSource ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlFilterTraitNode"/>
    /// with <see cref="SqlFilterTraitNode.IsConjunction"/> set to <b>true</b>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="filter">Underlying predicate.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> AndWhere<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AddTrait( SqlNode.FilterTrait( filter, isConjunction: true ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlFilterTraitNode"/>
    /// with <see cref="SqlFilterTraitNode.IsConjunction"/> set to <b>false</b>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="filter">Underlying predicate provider.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> OrWhere<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.OrWhere( filter( node.DataSource ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlFilterTraitNode"/>
    /// with <see cref="SqlFilterTraitNode.IsConjunction"/> set to <b>false</b>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="filter">Underlying predicate.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> OrWhere<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AddTrait( SqlNode.FilterTrait( filter, isConjunction: false ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlAggregationTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="expressions">Provider of collection of expressions to aggregate by.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> GroupBy<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        Func<TDataSourceNode, IEnumerable<SqlExpressionNode>> expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.GroupBy( expressions( node.DataSource ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlAggregationTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="expressions">Collection of expressions to aggregate by.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> GroupBy<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        IEnumerable<SqlExpressionNode> expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.GroupBy( expressions.ToArray() );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlAggregationTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="expressions">Collection of expressions to aggregate by.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> GroupBy<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        params SqlExpressionNode[] expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return expressions.Length == 0 ? node : node.AddTrait( SqlNode.AggregationTrait( expressions ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlAggregationFilterTraitNode"/>
    /// with <see cref="SqlAggregationFilterTraitNode.IsConjunction"/> set to <b>true</b>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="filter">Underlying predicate provider.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> AndHaving<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AndHaving( filter( node.DataSource ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlAggregationFilterTraitNode"/>
    /// with <see cref="SqlAggregationFilterTraitNode.IsConjunction"/> set to <b>true</b>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="filter">Underlying predicate.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> AndHaving<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AddTrait( SqlNode.AggregationFilterTrait( filter, isConjunction: true ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlAggregationFilterTraitNode"/>
    /// with <see cref="SqlAggregationFilterTraitNode.IsConjunction"/> set to <b>false</b>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="filter">Underlying predicate provider.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> OrHaving<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.OrHaving( filter( node.DataSource ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlAggregationFilterTraitNode"/>
    /// with <see cref="SqlAggregationFilterTraitNode.IsConjunction"/> set to <b>false</b>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="filter">Underlying predicate.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> OrHaving<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AddTrait( SqlNode.AggregationFilterTrait( filter, isConjunction: false ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlWindowDefinitionTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="windows">Provider of collection of window definitions.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Window<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        Func<TDataSourceNode, IEnumerable<SqlWindowDefinitionNode>> windows)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.Window( windows( node.DataSource ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlWindowDefinitionTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="windows">Collection of window definitions.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Window<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        IEnumerable<SqlWindowDefinitionNode> windows)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.Window( windows.ToArray() );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlWindowDefinitionTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="windows">Collection of window definitions.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Window<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        params SqlWindowDefinitionNode[] windows)
        where TDataSourceNode : SqlDataSourceNode
    {
        return windows.Length == 0 ? node : node.AddTrait( SqlNode.WindowDefinitionTrait( windows ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlSortTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="ordering">Provider of collection of ordering definitions.</param>
    /// <typeparam name="TQueryExpressionNode">SQL query node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode OrderBy<TQueryExpressionNode>(
        this TQueryExpressionNode node,
        Func<TQueryExpressionNode, IEnumerable<SqlOrderByNode>> ordering)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return node.OrderBy( ordering( node ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlSortTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="ordering">Collection of ordering definitions.</param>
    /// <typeparam name="TQueryExpressionNode">SQL query node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode OrderBy<TQueryExpressionNode>(this TQueryExpressionNode node, IEnumerable<SqlOrderByNode> ordering)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return node.OrderBy( ordering.ToArray() );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlSortTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="ordering">Collection of ordering definitions.</param>
    /// <typeparam name="TQueryExpressionNode">SQL query node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode OrderBy<TQueryExpressionNode>(this TQueryExpressionNode node, params SqlOrderByNode[] ordering)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return ordering.Length == 0 ? node : ( TQueryExpressionNode )node.AddTrait( SqlNode.SortTrait( ordering ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlCommonTableExpressionTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="commonTableExpressions">Provider of collection of common table expressions.</param>
    /// <typeparam name="TQueryExpressionNode">SQL query node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode With<TQueryExpressionNode>(
        this TQueryExpressionNode node,
        Func<TQueryExpressionNode, IEnumerable<SqlCommonTableExpressionNode>> commonTableExpressions)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return node.With( commonTableExpressions( node ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlCommonTableExpressionTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="commonTableExpressions">Collection of common table expressions.</param>
    /// <typeparam name="TQueryExpressionNode">SQL query node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode With<TQueryExpressionNode>(
        this TQueryExpressionNode node,
        IEnumerable<SqlCommonTableExpressionNode> commonTableExpressions)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return node.With( commonTableExpressions.ToArray() );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlCommonTableExpressionTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="commonTableExpressions">Collection of common table expressions.</param>
    /// <typeparam name="TQueryExpressionNode">SQL query node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode With<TQueryExpressionNode>(
        this TQueryExpressionNode node,
        params SqlCommonTableExpressionNode[] commonTableExpressions)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return commonTableExpressions.Length == 0
            ? node
            : ( TQueryExpressionNode )node.AddTrait( SqlNode.CommonTableExpressionTrait( commonTableExpressions ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlLimitTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="value">Underlying value.</param>
    /// <typeparam name="TQueryExpressionNode">SQL query node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode Limit<TQueryExpressionNode>(this TQueryExpressionNode node, SqlExpressionNode value)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return ( TQueryExpressionNode )node.AddTrait( SqlNode.LimitTrait( value ) );
    }

    /// <summary>
    /// Decorates the provided SQL query node with an <see cref="SqlOffsetTraitNode"/>.
    /// </summary>
    /// <param name="node">Query node to decorate.</param>
    /// <param name="value">Underlying value.</param>
    /// <typeparam name="TQueryExpressionNode">SQL query node type.</typeparam>
    /// <returns>Decorated SQL query node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode Offset<TQueryExpressionNode>(this TQueryExpressionNode node, SqlExpressionNode value)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return ( TQueryExpressionNode )node.AddTrait( SqlNode.OffsetTrait( value ) );
    }

    /// <summary>
    /// Creates a new SQL data source query expression node with added selection.
    /// </summary>
    /// <param name="node">Source query.</param>
    /// <param name="selector">Provider of collection of expressions to add to <see cref="SqlQueryExpressionNode.Selection"/>.</param>
    /// <returns>New SQL data source query expression node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Select<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        Func<TDataSourceNode, IEnumerable<SqlSelectNode>> selector)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.Select( selector( node.DataSource ) );
    }

    /// <summary>
    /// Creates a new SQL data source query expression node with added <paramref name="selection"/>.
    /// </summary>
    /// <param name="node">Source query.</param>
    /// <param name="selection">Collection of expressions to add to <see cref="SqlQueryExpressionNode.Selection"/>.</param>
    /// <returns>New SQL data source query expression node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Select<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        IEnumerable<SqlSelectNode> selection)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.Select( selection.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlExistsConditionNode"/> instance.
    /// </summary>
    /// <param name="node">Sub-query to check.</param>
    /// <returns>New <see cref="SqlExistsConditionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExistsConditionNode Exists(this SqlQueryExpressionNode node)
    {
        return SqlNode.Exists( node );
    }

    /// <summary>
    /// Creates a new negated <see cref="SqlExistsConditionNode"/> instance.
    /// </summary>
    /// <param name="node">Sub-query to check.</param>
    /// <returns>New negated <see cref="SqlExistsConditionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExistsConditionNode NotExists(this SqlQueryExpressionNode node)
    {
        return SqlNode.NotExists( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCreateViewNode"/> instance.
    /// </summary>
    /// <param name="node">Underlying source query expression that defines this view.</param>
    /// <param name="view">View's name.</param>
    /// <param name="replaceIfExists">
    /// Specifies whether or not the view should be replaced if it already exists in DB. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New <see cref="SqlCreateViewNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCreateViewNode ToCreateView(this SqlQueryExpressionNode node, SqlRecordSetInfo view, bool replaceIfExists = false)
    {
        return SqlNode.CreateView( view, node, replaceIfExists );
    }

    /// <summary>
    /// Creates a new <see cref="SqlInsertIntoNode"/> instance.
    /// </summary>
    /// <param name="node"><see cref="SqlQueryExpressionNode"/> source of records to be inserted.</param>
    /// <param name="recordSet">Table to insert into.</param>
    /// <param name="dataFields">Provider of collection of record set data fields that this insertion refers to.</param>
    /// <returns>New <see cref="SqlInsertIntoNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlInsertIntoNode ToInsertInto<TRecordSetNode>(
        this SqlQueryExpressionNode node,
        TRecordSetNode recordSet,
        Func<TRecordSetNode, IEnumerable<SqlDataFieldNode>> dataFields)
        where TRecordSetNode : SqlRecordSetNode
    {
        return node.ToInsertInto( recordSet, dataFields( recordSet ).ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlInsertIntoNode"/> instance.
    /// </summary>
    /// <param name="node"><see cref="SqlQueryExpressionNode"/> source of records to be inserted.</param>
    /// <param name="recordSet">Table to insert into.</param>
    /// <param name="dataFields">Collection of record set data fields that this insertion refers to.</param>
    /// <returns>New <see cref="SqlInsertIntoNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlInsertIntoNode ToInsertInto(
        this SqlQueryExpressionNode node,
        SqlRecordSetNode recordSet,
        params SqlDataFieldNode[] dataFields)
    {
        return SqlNode.InsertInto( node, recordSet, dataFields );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpsertNode"/> instance.
    /// </summary>
    /// <param name="node"><see cref="SqlQueryExpressionNode"/> source of records to be inserted or updated.</param>
    /// <param name="recordSet">Table to upsert into.</param>
    /// <param name="insertDataFields">
    /// Provider of collection of record set data fields that the insertion part of this upsert refers to.
    /// </param>
    /// <param name="updateAssignments">
    /// Provider of a collection of value assignments that the update part of this upsert refers to.
    /// The first parameter is the table to upsert into and the second parameter is the <see cref="SqlUpsertNode.UpdateSource"/>
    /// of the created upsert node.
    /// </param>
    /// <param name="conflictTarget">
    /// Optional provider of collection of data fields from the table that define the insertion conflict target.
    /// Empty conflict target may cause the table's primary key to be used instead. Equal to null by default.
    /// </param>
    /// <returns>New <see cref="SqlUpsertNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpsertNode ToUpsert<TRecordSetNode>(
        this SqlQueryExpressionNode node,
        TRecordSetNode recordSet,
        Func<TRecordSetNode, IEnumerable<SqlDataFieldNode>> insertDataFields,
        Func<TRecordSetNode, SqlInternalRecordSetNode, IEnumerable<SqlValueAssignmentNode>> updateAssignments,
        Func<TRecordSetNode, IEnumerable<SqlDataFieldNode>>? conflictTarget = null)
        where TRecordSetNode : SqlRecordSetNode
    {
        return node.ToUpsert(
            recordSet,
            insertDataFields( recordSet ).ToArray(),
            (r, i) => updateAssignments( ReinterpretCast.To<TRecordSetNode>( r ), i ),
            conflictTarget?.Invoke( recordSet ).ToArray() ?? ( ReadOnlyArray<SqlDataFieldNode>? )null );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpsertNode"/> instance.
    /// </summary>
    /// <param name="node"><see cref="SqlQueryExpressionNode"/> source of records to be inserted or updated.</param>
    /// <param name="recordSet">Table to upsert into.</param>
    /// <param name="insertDataFields">Collection of record set data fields that the insertion part of this upsert refers to.</param>
    /// <param name="updateAssignments">
    /// Provider of a collection of value assignments that the update part of this upsert refers to.
    /// The first parameter is the table to upsert into and the second parameter is the <see cref="SqlUpsertNode.UpdateSource"/>
    /// of the created upsert node.
    /// </param>
    /// <param name="conflictTarget">
    /// Optional collection of data fields from the table that define the insertion conflict target.
    /// Empty conflict target may cause the table's primary key to be used instead. Equal to null by default.
    /// </param>
    /// <returns>New <see cref="SqlUpsertNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpsertNode ToUpsert(
        this SqlQueryExpressionNode node,
        SqlRecordSetNode recordSet,
        ReadOnlyArray<SqlDataFieldNode> insertDataFields,
        Func<SqlRecordSetNode, SqlInternalRecordSetNode, IEnumerable<SqlValueAssignmentNode>> updateAssignments,
        ReadOnlyArray<SqlDataFieldNode>? conflictTarget = null)
    {
        return SqlNode.Upsert( node, recordSet, insertDataFields, updateAssignments, conflictTarget );
    }
}
