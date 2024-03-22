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
    public static TDataSourceNode Window<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, IEnumerable<SqlWindowDefinitionNode>> windows)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.Window( windows( node ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode Window<TDataSourceNode>(this TDataSourceNode node, IEnumerable<SqlWindowDefinitionNode> windows)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.Window( windows.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode Window<TDataSourceNode>(this TDataSourceNode node, params SqlWindowDefinitionNode[] windows)
        where TDataSourceNode : SqlDataSourceNode
    {
        return windows.Length == 0 ? node : (TDataSourceNode)node.AddTrait( SqlNode.WindowDefinitionTrait( windows ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode OrderBy<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, IEnumerable<SqlOrderByNode>> ordering)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.OrderBy( ordering( node ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode OrderBy<TDataSourceNode>(this TDataSourceNode node, IEnumerable<SqlOrderByNode> ordering)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.OrderBy( ordering.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode OrderBy<TDataSourceNode>(this TDataSourceNode node, params SqlOrderByNode[] ordering)
        where TDataSourceNode : SqlDataSourceNode
    {
        return ordering.Length == 0 ? node : (TDataSourceNode)node.AddTrait( SqlNode.SortTrait( ordering ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode With<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, IEnumerable<SqlCommonTableExpressionNode>> commonTableExpressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.With( commonTableExpressions( node ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode With<TDataSourceNode>(
        this TDataSourceNode node,
        IEnumerable<SqlCommonTableExpressionNode> commonTableExpressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.With( commonTableExpressions.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode With<TDataSourceNode>(
        this TDataSourceNode node,
        params SqlCommonTableExpressionNode[] commonTableExpressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return commonTableExpressions.Length == 0
            ? node
            : (TDataSourceNode)node.AddTrait( SqlNode.CommonTableExpressionTrait( commonTableExpressions ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode Limit<TDataSourceNode>(this TDataSourceNode node, SqlExpressionNode value)
        where TDataSourceNode : SqlDataSourceNode
    {
        return (TDataSourceNode)node.AddTrait( SqlNode.LimitTrait( value ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDataSourceNode Offset<TDataSourceNode>(this TDataSourceNode node, SqlExpressionNode value)
        where TDataSourceNode : SqlDataSourceNode
    {
        return (TDataSourceNode)node.AddTrait( SqlNode.OffsetTrait( value ) );
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
    public static SqlDeleteFromNode ToDeleteFrom(this SqlDataSourceNode node)
    {
        return SqlNode.DeleteFrom( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpdateNode ToUpdate<TDataSourceNode>(
        this TDataSourceNode node,
        Func<TDataSourceNode, IEnumerable<SqlValueAssignmentNode>> assignments)
        where TDataSourceNode : SqlDataSourceNode
    {
        return node.ToUpdate( assignments( node ).ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpdateNode ToUpdate(this SqlDataSourceNode node, params SqlValueAssignmentNode[] assignments)
    {
        return SqlNode.Update( node, assignments );
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
            conflictTarget?.Invoke( recordSet ).ToArray() ?? (ReadOnlyArray<SqlDataFieldNode>?)null );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpsertNode ToUpsert<TRecordSetNode>(
        this SqlValuesNode node,
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
            conflictTarget?.Invoke( recordSet ).ToArray() ?? (ReadOnlyArray<SqlDataFieldNode>?)null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpsertNode ToUpsert(
        this SqlValuesNode node,
        SqlRecordSetNode recordSet,
        ReadOnlyArray<SqlDataFieldNode> insertDataFields,
        Func<SqlRecordSetNode, SqlInternalRecordSetNode, IEnumerable<SqlValueAssignmentNode>> updateAssignments,
        ReadOnlyArray<SqlDataFieldNode>? conflictTarget = null)
    {
        return SqlNode.Upsert( node, recordSet, insertDataFields, updateAssignments, conflictTarget );
    }
}
