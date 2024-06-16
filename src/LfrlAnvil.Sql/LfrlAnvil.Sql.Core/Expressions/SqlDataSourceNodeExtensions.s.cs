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
/// Contains <see cref="SqlDataSourceNode"/> extension methods.
/// </summary>
public static class SqlDataSourceNodeExtensions
{
    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlDistinctTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode Distinct<TDataSourceNode>(this TDataSourceNode node)
        where TDataSourceNode : SqlDataSourceNode
    {
        return ( TDataSourceNode )node.AddTrait( SqlNode.DistinctTrait() );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlFilterTraitNode"/>
    /// with <see cref="SqlFilterTraitNode.IsConjunction"/> set to <b>true</b>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="filter">Underlying predicate provider.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode AndWhere<TDataSourceNode>(this TDataSourceNode node, Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AndWhere( filter( node ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlFilterTraitNode"/>
    /// with <see cref="SqlFilterTraitNode.IsConjunction"/> set to <b>true</b>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="filter">Underlying predicate.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode AndWhere<TDataSourceNode>(this TDataSourceNode node, SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return ( TDataSourceNode )node.AddTrait( SqlNode.FilterTrait( filter, isConjunction: true ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlFilterTraitNode"/>
    /// with <see cref="SqlFilterTraitNode.IsConjunction"/> set to <b>false</b>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="filter">Underlying predicate provider.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode OrWhere<TDataSourceNode>(this TDataSourceNode node, Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.OrWhere( filter( node ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlFilterTraitNode"/>
    /// with <see cref="SqlFilterTraitNode.IsConjunction"/> set to <b>false</b>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="filter">Underlying predicate.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode OrWhere<TDataSourceNode>(this TDataSourceNode node, SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return ( TDataSourceNode )node.AddTrait( SqlNode.FilterTrait( filter, isConjunction: false ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlAggregationTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="expressions">Provider of collection of expressions to aggregate by.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode GroupBy<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, IEnumerable<SqlExpressionNode>> expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.GroupBy( expressions( node ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlAggregationTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="expressions">Collection of expressions to aggregate by.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode GroupBy<TDataSourceNode>(this TDataSourceNode node, IEnumerable<SqlExpressionNode> expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.GroupBy( expressions.ToArray() );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlAggregationTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="expressions">Collection of expressions to aggregate by.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode GroupBy<TDataSourceNode>(this TDataSourceNode node, params SqlExpressionNode[] expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return expressions.Length == 0 ? node : ( TDataSourceNode )node.AddTrait( SqlNode.AggregationTrait( expressions ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlAggregationFilterTraitNode"/>
    /// with <see cref="SqlAggregationFilterTraitNode.IsConjunction"/> set to <b>true</b>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="filter">Underlying predicate provider.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode AndHaving<TDataSourceNode>(this TDataSourceNode node, Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AndHaving( filter( node ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlAggregationFilterTraitNode"/>
    /// with <see cref="SqlAggregationFilterTraitNode.IsConjunction"/> set to <b>true</b>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="filter">Underlying predicate.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode AndHaving<TDataSourceNode>(this TDataSourceNode node, SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return ( TDataSourceNode )node.AddTrait( SqlNode.AggregationFilterTrait( filter, isConjunction: true ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlAggregationFilterTraitNode"/>
    /// with <see cref="SqlAggregationFilterTraitNode.IsConjunction"/> set to <b>false</b>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="filter">Underlying predicate provider.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode OrHaving<TDataSourceNode>(this TDataSourceNode node, Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.OrHaving( filter( node ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlAggregationFilterTraitNode"/>
    /// with <see cref="SqlAggregationFilterTraitNode.IsConjunction"/> set to <b>false</b>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="filter">Underlying predicate.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode OrHaving<TDataSourceNode>(this TDataSourceNode node, SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return ( TDataSourceNode )node.AddTrait( SqlNode.AggregationFilterTrait( filter, isConjunction: false ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlWindowDefinitionTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="windows">Provider of collection of window definitions.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode Window<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, IEnumerable<SqlWindowDefinitionNode>> windows)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.Window( windows( node ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlWindowDefinitionTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="windows">Collection of window definitions.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode Window<TDataSourceNode>(this TDataSourceNode node, IEnumerable<SqlWindowDefinitionNode> windows)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.Window( windows.ToArray() );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlWindowDefinitionTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="windows">Collection of window definitions.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode Window<TDataSourceNode>(this TDataSourceNode node, params SqlWindowDefinitionNode[] windows)
        where TDataSourceNode : SqlDataSourceNode
    {
        return windows.Length == 0 ? node : ( TDataSourceNode )node.AddTrait( SqlNode.WindowDefinitionTrait( windows ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlSortTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="ordering">Provider of collection of ordering definitions.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode OrderBy<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, IEnumerable<SqlOrderByNode>> ordering)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.OrderBy( ordering( node ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlSortTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="ordering">Collection of ordering definitions.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode OrderBy<TDataSourceNode>(this TDataSourceNode node, IEnumerable<SqlOrderByNode> ordering)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.OrderBy( ordering.ToArray() );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlSortTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="ordering">Collection of ordering definitions.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode OrderBy<TDataSourceNode>(this TDataSourceNode node, params SqlOrderByNode[] ordering)
        where TDataSourceNode : SqlDataSourceNode
    {
        return ordering.Length == 0 ? node : ( TDataSourceNode )node.AddTrait( SqlNode.SortTrait( ordering ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlCommonTableExpressionTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="commonTableExpressions">Provider of collection of common table expressions.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode With<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, IEnumerable<SqlCommonTableExpressionNode>> commonTableExpressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.With( commonTableExpressions( node ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlCommonTableExpressionTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="commonTableExpressions">Collection of common table expressions.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode With<TDataSourceNode>(
        this TDataSourceNode node,
        IEnumerable<SqlCommonTableExpressionNode> commonTableExpressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.With( commonTableExpressions.ToArray() );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlCommonTableExpressionTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="commonTableExpressions">Collection of common table expressions.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode With<TDataSourceNode>(
        this TDataSourceNode node,
        params SqlCommonTableExpressionNode[] commonTableExpressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return commonTableExpressions.Length == 0
            ? node
            : ( TDataSourceNode )node.AddTrait( SqlNode.CommonTableExpressionTrait( commonTableExpressions ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlLimitTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="value">Underlying value.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode Limit<TDataSourceNode>(this TDataSourceNode node, SqlExpressionNode value)
        where TDataSourceNode : SqlDataSourceNode
    {
        return ( TDataSourceNode )node.AddTrait( SqlNode.LimitTrait( value ) );
    }

    /// <summary>
    /// Decorates the provided SQL data source node with an <see cref="SqlOffsetTraitNode"/>.
    /// </summary>
    /// <param name="node">Data source node to decorate.</param>
    /// <param name="value">Underlying value.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>Decorated SQL data source node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode Offset<TDataSourceNode>(this TDataSourceNode node, SqlExpressionNode value)
        where TDataSourceNode : SqlDataSourceNode
    {
        return ( TDataSourceNode )node.AddTrait( SqlNode.OffsetTrait( value ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSelectAllNode"/> instance.
    /// </summary>
    /// <param name="node">Data source to select all data fields from.</param>
    /// <returns>New <see cref="SqlSelectAllNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSelectAllNode GetAll(this SqlDataSourceNode node)
    {
        return SqlNode.SelectAll( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceQueryExpressionNode{TDataSourceNode}"/> instance.
    /// </summary>
    /// <param name="node">Underlying data source.</param>
    /// <param name="selector">Provider of collection of expressions to include in this query's selection.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>New <see cref="SqlDataSourceQueryExpressionNode{TDataSourceNode}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Select<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, IEnumerable<SqlSelectNode>> selector)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.Select( selector( node ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceQueryExpressionNode{TDataSourceNode}"/> instance.
    /// </summary>
    /// <param name="node">Underlying data source.</param>
    /// <param name="selection">Collection of expressions to include in this query's selection.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>New <see cref="SqlDataSourceQueryExpressionNode{TDataSourceNode}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Select<TDataSourceNode>(
        this TDataSourceNode node,
        IEnumerable<SqlSelectNode> selection)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.Select( selection.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceQueryExpressionNode{TDataSourceNode}"/> instance.
    /// </summary>
    /// <param name="node">Underlying data source.</param>
    /// <param name="selection">Collection of expressions to include in this query's selection.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>New <see cref="SqlDataSourceQueryExpressionNode{TDataSourceNode}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Select<TDataSourceNode>(
        this TDataSourceNode node,
        params SqlSelectNode[] selection)
        where TDataSourceNode : SqlDataSourceNode
    {
        return SqlNode.Query( node, selection );
    }

    /// <summary>
    /// Creates a new <see cref="SqlExistsConditionNode"/> instance.
    /// </summary>
    /// <param name="node">Data source of the sub-query to check.</param>
    /// <returns>New <see cref="SqlExistsConditionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExistsConditionNode Exists(this SqlDataSourceNode node)
    {
        return node.Select( node.GetAll() ).Exists();
    }

    /// <summary>
    /// Creates a new negated <see cref="SqlExistsConditionNode"/> instance.
    /// </summary>
    /// <param name="node">Data source of the sub-query to check.</param>
    /// <returns>New negated <see cref="SqlExistsConditionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExistsConditionNode NotExists(this SqlDataSourceNode node)
    {
        return node.Select( node.GetAll() ).NotExists();
    }

    /// <summary>
    /// Creates a new <see cref="SqlDeleteFromNode"/> instance.
    /// </summary>
    /// <param name="node">Data source that defines records to be deleted.</param>
    /// <returns>New <see cref="SqlDeleteFromNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDeleteFromNode ToDeleteFrom(this SqlDataSourceNode node)
    {
        return SqlNode.DeleteFrom( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpdateNode"/> instance.
    /// </summary>
    /// <param name="node">Data source that defines records to be updated.</param>
    /// <param name="assignments">Provider of collection of value assignments that this update refers to.</param>
    /// <returns>New <see cref="SqlUpdateNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpdateNode ToUpdate<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, IEnumerable<SqlValueAssignmentNode>> assignments)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.ToUpdate( assignments( node ).ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpdateNode"/> instance.
    /// </summary>
    /// <param name="node">Data source that defines records to be updated.</param>
    /// <param name="assignments">Collection of value assignments that this update refers to.</param>
    /// <returns>New <see cref="SqlUpdateNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpdateNode ToUpdate(this SqlDataSourceNode node, params SqlValueAssignmentNode[] assignments)
    {
        return SqlNode.Update( node, assignments );
    }
}
