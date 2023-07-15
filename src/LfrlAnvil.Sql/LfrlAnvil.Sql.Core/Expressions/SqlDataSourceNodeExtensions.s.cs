﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public static class SqlDataSourceNodeExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode Distinct<TDataSourceNode>(this TDataSourceNode node)
        where TDataSourceNode : SqlDataSourceNode
    {
        return (TDataSourceNode)node.AddTrait( SqlNode.DistinctTrait() );
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
        return (TDataSourceNode)node.AddTrait( SqlNode.FilterTrait( filter, isConjunction: true ) );
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
        return (TDataSourceNode)node.AddTrait( SqlNode.FilterTrait( filter, isConjunction: false ) );
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
        return expressions.Length == 0 ? node : (TDataSourceNode)node.AddTrait( SqlNode.AggregationTrait( expressions ) );
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
        return (TDataSourceNode)node.AddTrait( SqlNode.AggregationFilterTrait( filter, isConjunction: true ) );
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
        return (TDataSourceNode)node.AddTrait( SqlNode.AggregationFilterTrait( filter, isConjunction: false ) );
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
            : SqlNode.Query( node, SqlNode.SortTrait( ordering ) );
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
            : SqlNode.Query( node, SqlNode.CommonTableExpressionTrait( commonTableExpressions ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Limit<TDataSourceNode>(
        this TDataSourceNode node,
        SqlExpressionNode value)
        where TDataSourceNode : SqlDataSourceNode
    {
        return SqlNode.Query( node, SqlNode.LimitTrait( value ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Offset<TDataSourceNode>(
        this TDataSourceNode node,
        SqlExpressionNode value)
        where TDataSourceNode : SqlDataSourceNode
    {
        return SqlNode.Query( node, SqlNode.OffsetTrait( value ) );
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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDeleteFromNode ToDeleteFrom<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, SqlRecordSetNode> recordSet)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.ToDeleteFrom( recordSet( node ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDeleteFromNode ToDeleteFrom(this SqlDataSourceNode node, SqlRecordSetNode recordSet)
    {
        return SqlNode.DeleteFrom( node, recordSet );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDeleteFromNode ToDeleteFrom<TRecordSetNode>(this SqlSingleDataSourceNode<TRecordSetNode> node)
        where TRecordSetNode : SqlRecordSetNode
    {
        return node.ToDeleteFrom( node.From );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpdateNode ToUpdate<TDataSourceNode, TRecordSetNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, TRecordSetNode> recordSet,
        Func<TRecordSetNode, IEnumerable<SqlValueAssignmentNode>> assignments)
        where TDataSourceNode : SqlDataSourceNode
        where TRecordSetNode : SqlRecordSetNode
    {
        var set = recordSet( node );
        return node.ToUpdate( set, assignments( set ).ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpdateNode ToUpdate(
        this SqlDataSourceNode node,
        SqlRecordSetNode recordSet,
        params SqlValueAssignmentNode[] assignments)
    {
        return SqlNode.Update( node, recordSet, assignments );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpdateNode ToUpdate<TRecordSetNode>(
        this SqlSingleDataSourceNode<TRecordSetNode> node,
        Func<TRecordSetNode, IEnumerable<SqlValueAssignmentNode>> assignments)
        where TRecordSetNode : SqlRecordSetNode
    {
        return node.ToUpdate( assignments( node.From ).ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpdateNode ToUpdate<TRecordSetNode>(
        this SqlSingleDataSourceNode<TRecordSetNode> node,
        params SqlValueAssignmentNode[] assignments)
        where TRecordSetNode : SqlRecordSetNode
    {
        return node.ToUpdate( node.From, assignments );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlInsertIntoNode ToInsertInto(
        this SqlQueryExpressionNode node,
        SqlRecordSetNode recordSet,
        params SqlDataFieldNode[] dataFields)
    {
        return SqlNode.InsertInto( node, recordSet, dataFields );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlInsertIntoNode ToInsertInto<TRecordSetNode>(
        this SqlValuesNode node,
        TRecordSetNode recordSet,
        Func<TRecordSetNode, IEnumerable<SqlDataFieldNode>> dataFields)
        where TRecordSetNode : SqlRecordSetNode
    {
        return node.ToInsertInto( recordSet, dataFields( recordSet ).ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlInsertIntoNode ToInsertInto(this SqlValuesNode node, SqlRecordSetNode recordSet, params SqlDataFieldNode[] dataFields)
    {
        return SqlNode.InsertInto( node, recordSet, dataFields );
    }
}
