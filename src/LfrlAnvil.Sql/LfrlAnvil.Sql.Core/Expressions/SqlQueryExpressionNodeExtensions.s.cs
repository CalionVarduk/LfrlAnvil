using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public static class SqlQueryExpressionNodeExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryRecordSetNode AsSet(this SqlQueryExpressionNode node, string alias)
    {
        return SqlNode.QueryRecordSet( node, alias );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrdinalCommonTableExpressionNode ToCte(this SqlQueryExpressionNode query, string name)
    {
        return SqlNode.OrdinalCommonTableExpression( query, name );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecursiveCommonTableExpressionNode ToRecursiveCte(this SqlCompoundQueryExpressionNode query, string name)
    {
        return SqlNode.RecursiveCommonTableExpression( query, name );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecursiveCommonTableExpressionNode ToRecursive(
        this SqlOrdinalCommonTableExpressionNode node,
        Func<SqlCommonTableExpressionRecordSetNode, IEnumerable<SqlCompoundQueryComponentNode>> components)
    {
        return node.ToRecursive( components( node.RecordSet ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRecursiveCommonTableExpressionNode ToRecursive(
        this SqlOrdinalCommonTableExpressionNode node,
        IEnumerable<SqlCompoundQueryComponentNode> components)
    {
        return node.ToRecursive( components.ToArray() );
    }

    [Pure]
    public static SqlRecursiveCommonTableExpressionNode ToRecursive(
        this SqlOrdinalCommonTableExpressionNode node,
        params SqlCompoundQueryComponentNode[] components)
    {
        var query = new SqlCompoundQueryExpressionNode( node.Query, components );
        return query.ToRecursiveCte( node.Name );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCompoundQueryComponentNode ToUnion(this SqlQueryExpressionNode node)
    {
        return SqlNode.UnionWith( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCompoundQueryComponentNode ToUnionAll(this SqlQueryExpressionNode node)
    {
        return SqlNode.UnionAllWith( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCompoundQueryComponentNode ToIntersect(this SqlQueryExpressionNode node)
    {
        return SqlNode.IntersectWith( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCompoundQueryComponentNode ToExcept(this SqlQueryExpressionNode node)
    {
        return SqlNode.ExceptWith( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCompoundQueryComponentNode ToCompound(this SqlQueryExpressionNode node, SqlCompoundQueryOperator @operator)
    {
        return SqlNode.CompoundWith( @operator, node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCompoundQueryExpressionNode CompoundWith(
        this SqlQueryExpressionNode node,
        IEnumerable<SqlCompoundQueryComponentNode> followingQueries)
    {
        return node.CompoundWith( followingQueries.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCompoundQueryExpressionNode CompoundWith(
        this SqlQueryExpressionNode node,
        params SqlCompoundQueryComponentNode[] followingQueries)
    {
        return SqlNode.CompoundQuery( node, followingQueries );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Distinct<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AddTrait( SqlNode.DistinctTrait() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> AndWhere<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AndWhere( filter( node.DataSource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> AndWhere<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AddTrait( SqlNode.FilterTrait( filter, isConjunction: true ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> OrWhere<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.OrWhere( filter( node.DataSource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> OrWhere<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AddTrait( SqlNode.FilterTrait( filter, isConjunction: false ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> GroupBy<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        Func<TDataSourceNode, IEnumerable<SqlExpressionNode>> expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.GroupBy( expressions( node.DataSource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> GroupBy<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        IEnumerable<SqlExpressionNode> expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.GroupBy( expressions.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> GroupBy<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        params SqlExpressionNode[] expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return expressions.Length == 0 ? node : node.AddTrait( SqlNode.AggregationTrait( expressions ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> AndHaving<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AndHaving( filter( node.DataSource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> AndHaving<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AddTrait( SqlNode.AggregationFilterTrait( filter, isConjunction: true ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> OrHaving<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        Func<TDataSourceNode, SqlConditionNode> filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.OrHaving( filter( node.DataSource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> OrHaving<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.AddTrait( SqlNode.AggregationFilterTrait( filter, isConjunction: false ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode OrderBy<TQueryExpressionNode>(
        this TQueryExpressionNode node,
        Func<TQueryExpressionNode, IEnumerable<SqlOrderByNode>> ordering)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return node.OrderBy( ordering( node ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode OrderBy<TQueryExpressionNode>(this TQueryExpressionNode node, IEnumerable<SqlOrderByNode> ordering)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return node.OrderBy( ordering.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode OrderBy<TQueryExpressionNode>(this TQueryExpressionNode node, params SqlOrderByNode[] ordering)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return ordering.Length == 0 ? node : (TQueryExpressionNode)node.AddTrait( SqlNode.SortTrait( ordering ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode With<TQueryExpressionNode>(
        this TQueryExpressionNode node,
        Func<TQueryExpressionNode, IEnumerable<SqlCommonTableExpressionNode>> commonTableExpressions)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return node.With( commonTableExpressions( node ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode With<TQueryExpressionNode>(
        this TQueryExpressionNode node,
        IEnumerable<SqlCommonTableExpressionNode> commonTableExpressions)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return node.With( commonTableExpressions.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode With<TQueryExpressionNode>(
        this TQueryExpressionNode node,
        params SqlCommonTableExpressionNode[] commonTableExpressions)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return commonTableExpressions.Length == 0
            ? node
            : (TQueryExpressionNode)node.AddTrait( SqlNode.CommonTableExpressionTrait( commonTableExpressions ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode Limit<TQueryExpressionNode>(this TQueryExpressionNode node, SqlExpressionNode value)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return (TQueryExpressionNode)node.AddTrait( SqlNode.LimitTrait( value ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TQueryExpressionNode Offset<TQueryExpressionNode>(this TQueryExpressionNode node, SqlExpressionNode value)
        where TQueryExpressionNode : SqlExtendableQueryExpressionNode
    {
        return (TQueryExpressionNode)node.AddTrait( SqlNode.OffsetTrait( value ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Select<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        Func<TDataSourceNode, IEnumerable<SqlSelectNode>> selector)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.Select( selector( node.DataSource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Select<TDataSourceNode>(
        this SqlDataSourceQueryExpressionNode<TDataSourceNode> node,
        IEnumerable<SqlSelectNode> selection)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.Select( selection.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExistsConditionNode Exists(this SqlQueryExpressionNode node)
    {
        return SqlNode.Exists( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExistsConditionNode NotExists(this SqlQueryExpressionNode node)
    {
        return SqlNode.NotExists( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCreateViewNode ToCreateView(this SqlQueryExpressionNode node, string schemaName, string name, bool ifNotExists = false)
    {
        return SqlNode.CreateView( schemaName, name, node, ifNotExists );
    }
}
