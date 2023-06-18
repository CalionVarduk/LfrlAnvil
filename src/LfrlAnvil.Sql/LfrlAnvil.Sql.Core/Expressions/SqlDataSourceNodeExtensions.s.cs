using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions.Decorators;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public static class SqlDataSourceNodeExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode Distinct<TDataSourceNode>(this TDataSourceNode node)
        where TDataSourceNode : SqlDataSourceNode
    {
        return (TDataSourceNode)node.Decorate( SqlNode.DistinctDecorator() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode AndWhere<TDataSourceNode>(this TDataSourceNode node, Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AndWhere( filter( node ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode AndWhere<TDataSourceNode>(this TDataSourceNode node, SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return (TDataSourceNode)node.Decorate( SqlNode.FilterDecorator( filter, isConjunction: true ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode OrWhere<TDataSourceNode>(this TDataSourceNode node, Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.OrWhere( filter( node ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode OrWhere<TDataSourceNode>(this TDataSourceNode node, SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return (TDataSourceNode)node.Decorate( SqlNode.FilterDecorator( filter, isConjunction: false ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode GroupBy<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, IEnumerable<SqlExpressionNode>> expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.GroupBy( expressions( node ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode GroupBy<TDataSourceNode>(this TDataSourceNode node, IEnumerable<SqlExpressionNode> expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.GroupBy( expressions.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode GroupBy<TDataSourceNode>(this TDataSourceNode node, params SqlExpressionNode[] expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return expressions.Length == 0 ? node : (TDataSourceNode)node.Decorate( SqlNode.AggregationDecorator( expressions ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode AndHaving<TDataSourceNode>(this TDataSourceNode node, Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AndHaving( filter( node ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode AndHaving<TDataSourceNode>(this TDataSourceNode node, SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return (TDataSourceNode)node.Decorate( SqlNode.AggregationFilterDecorator( filter, isConjunction: true ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode OrHaving<TDataSourceNode>(this TDataSourceNode node, Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.OrHaving( filter( node ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode OrHaving<TDataSourceNode>(this TDataSourceNode node, SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return (TDataSourceNode)node.Decorate( SqlNode.AggregationFilterDecorator( filter, isConjunction: false ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> OrderBy<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, IEnumerable<SqlOrderByNode>> ordering)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.OrderBy( ordering( node ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> OrderBy<TDataSourceNode>(
        this TDataSourceNode node,
        IEnumerable<SqlOrderByNode> ordering)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.OrderBy( ordering.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> OrderBy<TDataSourceNode>(
        this TDataSourceNode node,
        params SqlOrderByNode[] ordering)
        where TDataSourceNode : SqlDataSourceNode
    {
        return ordering.Length == 0
            ? SqlNode.Query( node, Array.Empty<SqlSelectNode>() )
            : SqlNode.Query( node, SqlNode.SortDecorator( ordering ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> With<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, IEnumerable<SqlCommonTableExpressionNode>> commonTableExpressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.With( commonTableExpressions( node ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> With<TDataSourceNode>(
        this TDataSourceNode node,
        IEnumerable<SqlCommonTableExpressionNode> commonTableExpressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.With( commonTableExpressions.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> With<TDataSourceNode>(
        this TDataSourceNode node,
        params SqlCommonTableExpressionNode[] commonTableExpressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return commonTableExpressions.Length == 0
            ? SqlNode.Query( node, Array.Empty<SqlSelectNode>() )
            : SqlNode.Query( node, SqlNode.CommonTableExpressionDecorator( commonTableExpressions ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Limit<TDataSourceNode>(
        this TDataSourceNode node,
        SqlExpressionNode value)
        where TDataSourceNode : SqlDataSourceNode
    {
        return SqlNode.Query( node, SqlNode.LimitDecorator( value ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Offset<TDataSourceNode>(
        this TDataSourceNode node,
        SqlExpressionNode value)
        where TDataSourceNode : SqlDataSourceNode
    {
        return SqlNode.Query( node, SqlNode.OffsetDecorator( value ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSelectAllNode GetAll(this SqlDataSourceNode node)
    {
        return SqlNode.SelectAll( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Select<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, IEnumerable<SqlSelectNode>> selector)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.Select( selector( node ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Select<TDataSourceNode>(
        this TDataSourceNode node,
        IEnumerable<SqlSelectNode> selection)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.Select( selection.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Select<TDataSourceNode>(
        this TDataSourceNode node,
        params SqlSelectNode[] selection)
        where TDataSourceNode : SqlDataSourceNode
    {
        return SqlNode.Query( node, selection );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExistsConditionNode Exists(this SqlDataSourceNode node)
    {
        return node.Select( node.GetAll() ).Exists();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExistsConditionNode NotExists(this SqlDataSourceNode node)
    {
        return node.Select( node.GetAll() ).NotExists();
    }
}
