// Copyright 2026 Łukasz Furlepa
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
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Contains various extension methods related to SQL objects.
/// </summary>
public static class SqlObjectExtensions
{
    /// <summary>
    /// Creates a new <see cref="SqlDataSourceQueryExpressionNode{TDataSourceNode}"/> based on the provided <paramref name="table"/>,
    /// where by default all columns are selected and ordering is done by primary key.
    /// </summary>
    /// <param name="table">Table to create query for.</param>
    /// <param name="selectionOverride">
    /// Optional delegate which allows to either override the default column selection or skip the column entirely.
    /// </param>
    /// <param name="additionalSelection">Optional delegate which allows to add custom selection.</param>
    /// <param name="orderBy">Optional delegate which allows to override the default ordering by primary key.</param>
    /// <returns>New <see cref="SqlDataSourceQueryExpressionNode{TDataSourceNode}"/> instance.</returns>
    [Pure]
    public static SqlDataSourceQueryExpressionNode<SqlSingleDataSourceNode<SqlTableNode>> CreateQuery(
        this ISqlTable table,
        Func<SqlColumnNode, SqlSelectionOverride>? selectionOverride = null,
        Func<SqlTableNode, IEnumerable<SqlSelectNode>>? additionalSelection = null,
        Func<SqlTableNode, IEnumerable<SqlOrderByNode>>? orderBy = null)
    {
        var columns = table.Node.GetKnownFields();

        var selection = new List<SqlSelectNode>( capacity: columns.Count );
        foreach ( var c in columns )
        {
            SqlSelectNode? value = null;
            if ( selectionOverride is not null )
            {
                var @override = selectionOverride( c );
                if ( @override.IsIgnored )
                    continue;

                value = @override.Node;
            }

            value ??= c.AsSelf();
            selection.Add( value );
        }

        if ( additionalSelection is not null )
            selection.AddRange( additionalSelection( table.Node ) );

        var ordering = orderBy is null
            ? table.Constraints.PrimaryKey.Index.Columns.Select( static e =>
            {
                Assume.IsNotNull( e.Column );
                return SqlNode.OrderBy( e.Column.Node, e.Ordering );
            } )
            : orderBy( table.Node );

        return table.Node.ToDataSource().Select( selection ).OrderBy( ordering );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceQueryExpressionNode{TDataSourceNode}"/> based on the provided <paramref name="table"/>,
    /// where by default all columns are selected and ordering is done by primary key.
    /// </summary>
    /// <param name="table">Table to create query for.</param>
    /// <param name="selectionOverride">
    /// Optional delegate which allows to either override the default column selection or skip the column entirely.
    /// </param>
    /// <param name="additionalSelection">Optional delegate which allows to add custom selection.</param>
    /// <param name="orderBy">Optional delegate which allows to override the default ordering by primary key.</param>
    /// <returns>New <see cref="SqlDataSourceQueryExpressionNode{TDataSourceNode}"/> instance.</returns>
    [Pure]
    public static SqlDataSourceQueryExpressionNode<SqlSingleDataSourceNode<SqlTableBuilderNode>> CreateQuery(
        this ISqlTableBuilder table,
        Func<SqlColumnBuilderNode, SqlSelectionOverride>? selectionOverride = null,
        Func<SqlTableBuilderNode, IEnumerable<SqlSelectNode>>? additionalSelection = null,
        Func<SqlTableBuilderNode, IEnumerable<SqlOrderByNode>>? orderBy = null)
    {
        var columns = table.Node.GetKnownFields();

        var selection = new List<SqlSelectNode>( capacity: columns.Count );
        foreach ( var c in columns )
        {
            SqlSelectNode? value = null;
            if ( selectionOverride is not null )
            {
                var @override = selectionOverride( c );
                if ( @override.IsIgnored )
                    continue;

                value = @override.Node;
            }

            value ??= c.AsSelf();
            selection.Add( value );
        }

        if ( additionalSelection is not null )
            selection.AddRange( additionalSelection( table.Node ) );

        var ordering = orderBy is null
            ? table.Constraints.TryGetPrimaryKey()?.Index.Columns.Expressions.GetUnderlyingArray() ?? [ ]
            : orderBy( table.Node );

        return table.Node.ToDataSource().Select( selection ).OrderBy( ordering );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceQueryExpressionNode{TDataSourceNode}"/> based on the provided <paramref name="table"/>,
    /// where by default all columns are selected and ordering is done by primary key.
    /// </summary>
    /// <param name="table">Table to create query for.</param>
    /// <param name="selectionOverride">
    /// Optional delegate which allows to either override the default column selection or skip the column entirely.
    /// </param>
    /// <param name="additionalSelection">Optional delegate which allows to add custom selection.</param>
    /// <param name="orderBy">Optional delegate which allows to override the default ordering by primary key.</param>
    /// <returns>New <see cref="SqlDataSourceQueryExpressionNode{TDataSourceNode}"/> instance.</returns>
    [Pure]
    public static SqlDataSourceQueryExpressionNode<SqlSingleDataSourceNode<SqlNewTableNode>> CreateQuery(
        this SqlNewTableNode table,
        Func<SqlColumnDefinitionNode, SqlRawDataFieldNode, SqlSelectionOverride>? selectionOverride = null,
        Func<SqlNewTableNode, IEnumerable<SqlSelectNode>>? additionalSelection = null,
        Func<SqlNewTableNode, IEnumerable<SqlOrderByNode>>? orderBy = null)
    {
        var columns = table.CreationNode.Columns;

        var selection = new List<SqlSelectNode>( capacity: columns.Count );
        foreach ( var c in columns )
        {
            var dataField = table.GetField( c.Name );
            SqlSelectNode? value = null;
            if ( selectionOverride is not null )
            {
                var @override = selectionOverride( c, dataField );
                if ( @override.IsIgnored )
                    continue;

                value = @override.Node;
            }

            value ??= dataField.AsSelf();
            selection.Add( value );
        }

        if ( additionalSelection is not null )
            selection.AddRange( additionalSelection( table ) );

        var ordering = orderBy is null
            ? table.CreationNode.PrimaryKey?.Columns.GetUnderlyingArray() ?? [ ]
            : orderBy( table );

        return table.ToDataSource().Select( selection ).OrderBy( ordering );
    }

    /// <summary>
    /// Creates a new <see cref="SqlInsertIntoNode"/> based on the provided <paramref name="table"/> and its columns,
    /// where assigned values are matching parameters.
    /// </summary>
    /// <param name="table">Table to create insert into for.</param>
    /// <param name="valueOverride">
    /// Optional delegate which allows to either override the default matching parameter value or skip the column entirely.
    /// </param>
    /// <returns>New <see cref="SqlInsertIntoNode"/> instance.</returns>
    /// <remarks>Computed and identity columns are always skipped.</remarks>
    [Pure]
    public static SqlInsertIntoNode CreateInsertInto(this ISqlTable table, Func<SqlColumnNode, SqlExpressionOverride>? valueOverride = null)
    {
        var columns = table.Node.GetKnownFields();

        var values = new List<SqlExpressionNode>( capacity: columns.Count );
        var dataFields = new List<SqlDataFieldNode>( capacity: columns.Count );
        foreach ( var c in columns )
        {
            if ( c.Value.ComputationStorage is not null || c.Value.Identity is not null )
                continue;

            SqlExpressionNode? value = null;
            if ( valueOverride is not null )
            {
                var @override = valueOverride( c );
                if ( @override.IsIgnored )
                    continue;

                value = @override.Node;
            }

            value ??= c.CreateParameter();
            dataFields.Add( c );
            values.Add( value );
        }

        return SqlNode.Values( values.ToArray() ).ToInsertInto( table.Node, dataFields.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlInsertIntoNode"/> based on the provided <paramref name="table"/> and its columns,
    /// where assigned values are matching parameters.
    /// </summary>
    /// <param name="table">Table to create insert into for.</param>
    /// <param name="valueOverride">
    /// Optional delegate which allows to either override the default matching parameter value or skip the column entirely.
    /// </param>
    /// <returns>New <see cref="SqlInsertIntoNode"/> instance.</returns>
    /// <remarks>Computed and identity columns are always skipped.</remarks>
    [Pure]
    public static SqlInsertIntoNode CreateInsertInto(
        this ISqlTableBuilder table,
        Func<SqlColumnBuilderNode, SqlExpressionOverride>? valueOverride = null)
    {
        var columns = table.Node.GetKnownFields();

        var values = new List<SqlExpressionNode>( capacity: columns.Count );
        var dataFields = new List<SqlDataFieldNode>( capacity: columns.Count );
        foreach ( var c in columns )
        {
            if ( c.Value.Computation is not null || c.Value.Identity is not null )
                continue;

            SqlExpressionNode? value = null;
            if ( valueOverride is not null )
            {
                var @override = valueOverride( c );
                if ( @override.IsIgnored )
                    continue;

                value = @override.Node;
            }

            value ??= c.CreateParameter();
            dataFields.Add( c );
            values.Add( value );
        }

        return SqlNode.Values( values.ToArray() ).ToInsertInto( table.Node, dataFields.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlInsertIntoNode"/> based on the provided <paramref name="table"/> and its columns,
    /// where assigned values are matching parameters.
    /// </summary>
    /// <param name="table">Table to create insert into for.</param>
    /// <param name="valueOverride">
    /// Optional delegate which allows to either override the default matching parameter value or skip the column entirely.
    /// </param>
    /// <returns>New <see cref="SqlInsertIntoNode"/> instance.</returns>
    /// <remarks>Computed and identity columns are always skipped.</remarks>
    [Pure]
    public static SqlInsertIntoNode CreateInsertInto(
        this SqlNewTableNode table,
        Func<SqlColumnDefinitionNode, SqlRawDataFieldNode, SqlExpressionOverride>? valueOverride = null)
    {
        var columns = table.CreationNode.Columns;

        var values = new List<SqlExpressionNode>( capacity: columns.Count );
        var dataFields = new List<SqlDataFieldNode>( capacity: columns.Count );
        foreach ( var c in columns )
        {
            if ( c.Computation is not null || c.Identity is not null )
                continue;

            var dataField = table.GetField( c.Name );
            SqlExpressionNode? value = null;
            if ( valueOverride is not null )
            {
                var @override = valueOverride( c, dataField );
                if ( @override.IsIgnored )
                    continue;

                value = @override.Node;
            }

            value ??= SqlNode.Parameter( c.Name, c.Type );
            dataFields.Add( dataField );
            values.Add( value );
        }

        return SqlNode.Values( values.ToArray() ).ToInsertInto( table, dataFields.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDeleteFromNode"/> based on the provided <paramref name="table"/>
    /// and a filter created from its primary key columns and matching parameters.
    /// </summary>
    /// <param name="table">Table to create delete from for.</param>
    /// <param name="filterExtension">Optional delegate which allows to add additional filter to the primary key filter.</param>
    /// <returns>New <see cref="SqlDeleteFromNode"/> instance.</returns>
    [Pure]
    public static SqlDeleteFromNode CreateDeleteFrom(this ISqlTable table, Func<SqlTableNode, SqlConditionNode>? filterExtension = null)
    {
        var filter = GetPrimaryKeyFilter( table.Constraints.PrimaryKey.Index.Columns );
        if ( filterExtension is not null )
            filter = filter.And( filterExtension( table.Node ) );

        return table.Node.ToDataSource().AndWhere( filter ).ToDeleteFrom();
    }

    /// <summary>
    /// Creates a new <see cref="SqlDeleteFromNode"/> based on the provided <paramref name="table"/>
    /// and a filter created from its primary key columns and matching parameters.
    /// </summary>
    /// <param name="table">Table to create delete from for.</param>
    /// <param name="filterExtension">Optional delegate which allows to add additional filter to the primary key filter.</param>
    /// <returns>New <see cref="SqlDeleteFromNode"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When primary key builder does not exist.</exception>
    [Pure]
    public static SqlDeleteFromNode CreateDeleteFrom(
        this ISqlTableBuilder table,
        Func<SqlTableBuilderNode, SqlConditionNode>? filterExtension = null)
    {
        var filter = GetPrimaryKeyFilter( table, table.Constraints.GetPrimaryKey().Index.Columns.Expressions );
        if ( filterExtension is not null )
            filter = filter.And( filterExtension( table.Node ) );

        return table.Node.ToDataSource().AndWhere( filter ).ToDeleteFrom();
    }

    /// <summary>
    /// Creates a new <see cref="SqlDeleteFromNode"/> based on the provided <paramref name="table"/>
    /// and a filter created from its primary key columns and matching parameters.
    /// </summary>
    /// <param name="table">Table to create delete from for.</param>
    /// <param name="filterExtension">Optional delegate which allows to add additional filter to the primary key filter.</param>
    /// <returns>New <see cref="SqlDeleteFromNode"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// When primary key is not defined or one of its columns is not a valid data field.
    /// </exception>
    [Pure]
    public static SqlDeleteFromNode CreateDeleteFrom(
        this SqlNewTableNode table,
        Func<SqlNewTableNode, SqlConditionNode>? filterExtension = null)
    {
        if ( table.CreationNode.PrimaryKey is null || table.CreationNode.PrimaryKey.Columns.Count == 0 )
            throw new InvalidOperationException( ExceptionResources.PrimaryKeyIsRequired );

        var filter = GetPrimaryKeyFilter( table.CreationNode.PrimaryKey.Columns );
        if ( filterExtension is not null )
            filter = filter.And( filterExtension( table ) );

        return table.ToDataSource().AndWhere( filter ).ToDeleteFrom();
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpdateNode"/> based on the provided <paramref name="table"/> and its columns,
    /// where assigned values are matching parameters, and a filter created from its primary key columns and matching parameters.
    /// </summary>
    /// <param name="table">Table to create update for.</param>
    /// <param name="valueOverride">
    /// Optional delegate which allows to either override the default assigned matching parameter value
    /// or skip the column entirely from being updated.
    /// </param>
    /// <param name="filterExtension">Optional delegate which allows to add additional filter to the primary key filter.</param>
    /// <returns>New <see cref="SqlUpdateNode"/> instance.</returns>
    /// <remarks>Computed, identity and primary key columns are always skipped from being updated.</remarks>
    [Pure]
    public static SqlUpdateNode CreateUpdate(
        this ISqlTable table,
        Func<SqlColumnNode, SqlExpressionOverride>? valueOverride = null,
        Func<SqlTableNode, SqlConditionNode>? filterExtension = null)
    {
        var pkColumns = table.Constraints.PrimaryKey.Index.Columns;
        var filter = GetPrimaryKeyFilter( pkColumns );
        if ( filterExtension is not null )
            filter = filter.And( filterExtension( table.Node ) );

        var columns = table.Node.GetKnownFields();
        var assignments = new List<SqlValueAssignmentNode>( capacity: columns.Count );
        foreach ( var c in columns )
        {
            if ( c.Value.ComputationStorage is not null || c.Value.Identity is not null || IsInPrimaryKey( pkColumns, c.Value ) )
                continue;

            SqlExpressionNode? value = null;
            if ( valueOverride is not null )
            {
                var @override = valueOverride( c );
                if ( @override.IsIgnored )
                    continue;

                value = @override.Node;
            }

            value ??= c.CreateParameter();
            assignments.Add( c.Assign( value ) );
        }

        return table.Node.ToDataSource().AndWhere( filter ).ToUpdate( assignments.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpdateNode"/> based on the provided <paramref name="table"/> and its columns,
    /// where assigned values are matching parameters, and a filter created from its primary key columns and matching parameters.
    /// </summary>
    /// <param name="table">Table to create update for.</param>
    /// <param name="valueOverride">
    /// Optional delegate which allows to either override the default assigned matching parameter value
    /// or skip the column entirely from being updated.
    /// </param>
    /// <param name="filterExtension">Optional delegate which allows to add additional filter to the primary key filter.</param>
    /// <returns>New <see cref="SqlUpdateNode"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When primary key builder does not exist.</exception>
    /// <remarks>Computed, identity and primary key columns are always skipped from being updated.</remarks>
    [Pure]
    public static SqlUpdateNode CreateUpdate(
        this ISqlTableBuilder table,
        Func<SqlColumnBuilderNode, SqlExpressionOverride>? valueOverride = null,
        Func<SqlTableBuilderNode, SqlConditionNode>? filterExtension = null)
    {
        var pkExpressions = table.Constraints.GetPrimaryKey().Index.Columns.Expressions;
        var filter = GetPrimaryKeyFilter( table, pkExpressions );
        if ( filterExtension is not null )
            filter = filter.And( filterExtension( table.Node ) );

        var columns = table.Node.GetKnownFields();
        var assignments = new List<SqlValueAssignmentNode>( capacity: columns.Count );
        foreach ( var c in columns )
        {
            if ( c.Value.Computation is not null || c.Value.Identity is not null || IsInPrimaryKey( pkExpressions, c.Value ) )
                continue;

            SqlExpressionNode? value = null;
            if ( valueOverride is not null )
            {
                var @override = valueOverride( c );
                if ( @override.IsIgnored )
                    continue;

                value = @override.Node;
            }

            value ??= c.CreateParameter();
            assignments.Add( c.Assign( value ) );
        }

        return table.Node.ToDataSource().AndWhere( filter ).ToUpdate( assignments.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpdateNode"/> based on the provided <paramref name="table"/> and its columns,
    /// where assigned values are matching parameters, and a filter created from its primary key columns and matching parameters.
    /// </summary>
    /// <param name="table">Table to create update for.</param>
    /// <param name="valueOverride">
    /// Optional delegate which allows to either override the default assigned matching parameter value
    /// or skip the column entirely from being updated.
    /// </param>
    /// <param name="filterExtension">Optional delegate which allows to add additional filter to the primary key filter.</param>
    /// <returns>New <see cref="SqlUpdateNode"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// When primary key is not defined or one of its columns is not a valid data field.
    /// </exception>
    /// <remarks>Computed, identity and primary key columns are always skipped from being updated.</remarks>
    [Pure]
    public static SqlUpdateNode CreateUpdate(
        this SqlNewTableNode table,
        Func<SqlColumnDefinitionNode, SqlRawDataFieldNode, SqlExpressionOverride>? valueOverride = null,
        Func<SqlNewTableNode, SqlConditionNode>? filterExtension = null)
    {
        if ( table.CreationNode.PrimaryKey is null || table.CreationNode.PrimaryKey.Columns.Count == 0 )
            throw new InvalidOperationException( ExceptionResources.PrimaryKeyIsRequired );

        var pkExpressions = table.CreationNode.PrimaryKey.Columns;
        var filter = GetPrimaryKeyFilter( pkExpressions );
        if ( filterExtension is not null )
            filter = filter.And( filterExtension( table ) );

        var columns = table.CreationNode.Columns;
        var assignments = new List<SqlValueAssignmentNode>( capacity: columns.Count );
        foreach ( var c in columns )
        {
            if ( c.Computation is not null || c.Identity is not null )
                continue;

            var dataField = table.GetField( c.Name );
            if ( IsInPrimaryKey( pkExpressions, dataField ) )
                continue;

            SqlExpressionNode? value = null;
            if ( valueOverride is not null )
            {
                var @override = valueOverride( c, dataField );
                if ( @override.IsIgnored )
                    continue;

                value = @override.Node;
            }

            value ??= SqlNode.Parameter( c.Name, c.Type );
            assignments.Add( dataField.Assign( value ) );
        }

        return table.ToDataSource().AndWhere( filter ).ToUpdate( assignments.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpsertNode"/> based on the provided <paramref name="table"/> and its columns,
    /// where assigned values are matching parameters.
    /// </summary>
    /// <param name="table">Table to create update for.</param>
    /// <param name="insertValueOverride">
    /// Optional delegate which allows to either override the default assigned matching parameter value for the insert part
    /// or skip the column entirely from being inserted.
    /// </param>
    /// <param name="updateValueOverride">
    /// Optional delegate which allows to either override the default assigned value for the update part
    /// or skip the column entirely from being updated.
    /// </param>
    /// <param name="updateFilter">Optional delegate which allows to set an additional update filter.</param>
    /// <returns>New <see cref="SqlUpsertNode"/> instance.</returns>
    /// <remarks>
    /// Computed and identity columns are always skipped from being inserted.
    /// Computed, identity and primary key columns are always skipped from being updated.
    /// </remarks>
    [Pure]
    public static SqlUpsertNode CreateUpsert(
        this ISqlTable table,
        Func<SqlColumnNode, SqlExpressionOverride>? insertValueOverride = null,
        Func<SqlColumnNode, SqlDataFieldNode, SqlExpressionOverride>? updateValueOverride = null,
        Func<SqlTableNode, SqlInternalRecordSetNode, SqlConditionNode>? updateFilter = null)
    {
        var pkColumns = table.Constraints.PrimaryKey.Index.Columns;
        var columns = table.Node.GetKnownFields();

        var values = new List<SqlExpressionNode>( capacity: columns.Count );
        var insertDataFields = new List<SqlDataFieldNode>( capacity: columns.Count );
        foreach ( var c in columns )
        {
            if ( c.Value.ComputationStorage is not null || c.Value.Identity is not null )
                continue;

            SqlExpressionNode? value = null;
            if ( insertValueOverride is not null )
            {
                var @override = insertValueOverride( c );
                if ( @override.IsIgnored )
                    continue;

                value = @override.Node;
            }

            value ??= c.CreateParameter();
            insertDataFields.Add( c );
            values.Add( value );
        }

        return SqlNode.Values( values.ToArray() )
            .ToUpsert(
                table.Node,
                insertDataFields.ToArray(),
                (_, excluded) =>
                {
                    var assignments = new List<SqlValueAssignmentNode>();
                    foreach ( var c in columns )
                    {
                        if ( c.Value.ComputationStorage is not null
                            || c.Value.Identity is not null
                            || IsInPrimaryKey( pkColumns, c.Value ) )
                            continue;

                        SqlExpressionNode? value = null;
                        var dataField = excluded.GetField( c.Name );
                        if ( updateValueOverride is not null )
                        {
                            var @override = updateValueOverride( c, dataField );
                            if ( @override.IsIgnored )
                                continue;

                            value = @override.Node;
                        }

                        value ??= dataField;
                        assignments.Add( c.Assign( value ) );
                    }

                    return new SqlUpsertNodeUpdatePart( assignments, updateFilter?.Invoke( table.Node, excluded ) );
                },
                pkColumns.Select( static SqlDataFieldNode (c) =>
                    {
                        Assume.IsNotNull( c.Column );
                        return c.Column.Node;
                    } )
                    .ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpsertNode"/> based on the provided <paramref name="table"/> and its columns,
    /// where assigned values are matching parameters.
    /// </summary>
    /// <param name="table">Table to create update for.</param>
    /// <param name="insertValueOverride">
    /// Optional delegate which allows to either override the default assigned matching parameter value for the insert part
    /// or skip the column entirely from being inserted.
    /// </param>
    /// <param name="updateValueOverride">
    /// Optional delegate which allows to either override the default assigned value for the update part
    /// or skip the column entirely from being updated.
    /// </param>
    /// <param name="updateFilter">Optional delegate which allows to set an additional update filter.</param>
    /// <returns>New <see cref="SqlUpsertNode"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When primary key builder does not exist.</exception>
    /// <remarks>
    /// Computed and identity columns are always skipped from being inserted.
    /// Computed, identity and primary key columns are always skipped from being updated.
    /// </remarks>
    [Pure]
    public static SqlUpsertNode CreateUpsert(
        this ISqlTableBuilder table,
        Func<SqlColumnBuilderNode, SqlExpressionOverride>? insertValueOverride = null,
        Func<SqlColumnBuilderNode, SqlDataFieldNode, SqlExpressionOverride>? updateValueOverride = null,
        Func<SqlTableBuilderNode, SqlInternalRecordSetNode, SqlConditionNode>? updateFilter = null)
    {
        var pkExpressions = table.Constraints.GetPrimaryKey().Index.Columns.Expressions;
        var columns = table.Node.GetKnownFields();

        var values = new List<SqlExpressionNode>( capacity: columns.Count );
        var insertDataFields = new List<SqlDataFieldNode>( capacity: columns.Count );
        foreach ( var c in columns )
        {
            if ( c.Value.Computation is not null || c.Value.Identity is not null )
                continue;

            SqlExpressionNode? value = null;
            if ( insertValueOverride is not null )
            {
                var @override = insertValueOverride( c );
                if ( @override.IsIgnored )
                    continue;

                value = @override.Node;
            }

            value ??= c.CreateParameter();
            insertDataFields.Add( c );
            values.Add( value );
        }

        return SqlNode.Values( values.ToArray() )
            .ToUpsert(
                table.Node,
                insertDataFields.ToArray(),
                (_, excluded) =>
                {
                    var assignments = new List<SqlValueAssignmentNode>();
                    foreach ( var c in columns )
                    {
                        if ( c.Value.Computation is not null || c.Value.Identity is not null || IsInPrimaryKey( pkExpressions, c.Value ) )
                            continue;

                        SqlExpressionNode? value = null;
                        var dataField = excluded.GetField( c.Name );
                        if ( updateValueOverride is not null )
                        {
                            var @override = updateValueOverride( c, dataField );
                            if ( @override.IsIgnored )
                                continue;

                            value = @override.Node;
                        }

                        value ??= dataField;
                        assignments.Add( c.Assign( value ) );
                    }

                    return new SqlUpsertNodeUpdatePart( assignments, updateFilter?.Invoke( table.Node, excluded ) );
                },
                pkExpressions.Select( e => SqlHelpers.CastOrThrow<SqlDataFieldNode>( table.Database, e.Expression ) ).ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpsertNode"/> based on the provided <paramref name="table"/> and its columns,
    /// where assigned values are matching parameters.
    /// </summary>
    /// <param name="table">Table to create update for.</param>
    /// <param name="insertValueOverride">
    /// Optional delegate which allows to either override the default assigned matching parameter value for the insert part
    /// or skip the column entirely from being inserted.
    /// </param>
    /// <param name="updateValueOverride">
    /// Optional delegate which allows to either override the default assigned value for the update part
    /// or skip the column entirely from being updated.
    /// </param>
    /// <param name="updateFilter">Optional delegate which allows to set an additional update filter.</param>
    /// <returns>New <see cref="SqlUpsertNode"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// When primary key is not defined or one of its columns is not a valid data field.
    /// </exception>
    /// <remarks>
    /// Computed and identity columns are always skipped from being inserted.
    /// Computed, identity and primary key columns are always skipped from being updated.
    /// </remarks>
    [Pure]
    public static SqlUpsertNode CreateUpsert(
        this SqlNewTableNode table,
        Func<SqlColumnDefinitionNode, SqlRawDataFieldNode, SqlExpressionOverride>? insertValueOverride = null,
        Func<SqlColumnDefinitionNode, SqlRawDataFieldNode, SqlDataFieldNode, SqlExpressionOverride>? updateValueOverride = null,
        Func<SqlNewTableNode, SqlInternalRecordSetNode, SqlConditionNode>? updateFilter = null)
    {
        if ( table.CreationNode.PrimaryKey is null || table.CreationNode.PrimaryKey.Columns.Count == 0 )
            throw new InvalidOperationException( ExceptionResources.PrimaryKeyIsRequired );

        var pkExpressions = table.CreationNode.PrimaryKey.Columns;
        var columns = table.CreationNode.Columns;

        var values = new List<SqlExpressionNode>( capacity: columns.Count );
        var insertDataFields = new List<SqlDataFieldNode>( capacity: columns.Count );
        foreach ( var c in columns )
        {
            if ( c.Computation is not null || c.Identity is not null )
                continue;

            var dataField = table.GetField( c.Name );
            SqlExpressionNode? value = null;
            if ( insertValueOverride is not null )
            {
                var @override = insertValueOverride( c, dataField );
                if ( @override.IsIgnored )
                    continue;

                value = @override.Node;
            }

            value ??= SqlNode.Parameter( c.Name, c.Type );
            insertDataFields.Add( dataField );
            values.Add( value );
        }

        return SqlNode.Values( values.ToArray() )
            .ToUpsert(
                table,
                insertDataFields.ToArray(),
                (_, excluded) =>
                {
                    var assignments = new List<SqlValueAssignmentNode>();
                    foreach ( var c in columns )
                    {
                        if ( c.Computation is not null || c.Identity is not null )
                            continue;

                        var dataField = table.GetField( c.Name );
                        if ( IsInPrimaryKey( pkExpressions, dataField ) )
                            continue;

                        SqlExpressionNode? value = null;
                        var excludedDataField = excluded.GetField( c.Name );
                        if ( updateValueOverride is not null )
                        {
                            var @override = updateValueOverride( c, dataField, excludedDataField );
                            if ( @override.IsIgnored )
                                continue;

                            value = @override.Node;
                        }

                        value ??= excludedDataField;
                        assignments.Add( dataField.Assign( value ) );
                    }

                    return new SqlUpsertNodeUpdatePart( assignments, updateFilter?.Invoke( table, excluded ) );
                },
                pkExpressions.Select( GetDataFieldOrThrow ).ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCreateTableNode"/> based on the provided <paramref name="table"/> and its columns.
    /// </summary>
    /// <param name="table">Table to create temporary table from.</param>
    /// <param name="name">Name of the temporary table.</param>
    /// <param name="columnOverride">
    /// Optional delegate which allows to either override the default column definition or skip the column entirely.
    /// </param>
    /// <param name="ifNotExists">
    /// Specifies whether the temporary table should only be created if it does not already exist in DB. Equal to <b>false</b> by default.
    /// </param>
    /// <param name="constraintsProvider">Optional <see cref="SqlCreateTableConstraints"/> provider.</param>
    /// <returns>New <see cref="SqlCreateTableNode"/> instance.</returns>
    /// <remarks>Default values, computations and identities are not included in default column definitions.</remarks>
    [Pure]
    public static SqlCreateTableNode CreateTempTable(
        this ISqlTable table,
        string name,
        Func<ISqlColumn, SqlColumnDefinitionOverride>? columnOverride = null,
        bool ifNotExists = false,
        Func<ISqlTable, SqlNewTableNode, SqlCreateTableConstraints>? constraintsProvider = null)
    {
        var columns = table.Node.GetKnownFields();

        var columnDefinitions = new List<SqlColumnDefinitionNode>( capacity: columns.Count );
        foreach ( var c in columns )
        {
            SqlColumnDefinitionNode? definition = null;
            if ( columnOverride is not null )
            {
                var @override = columnOverride( c.Value );
                if ( @override.IsIgnored )
                    continue;

                definition = @override.Node;
            }

            definition ??= SqlNode.Column( c.Value.Name, c.Value.TypeDefinition, c.Value.IsNullable );
            columnDefinitions.Add( definition );
        }

        return SqlNode.CreateTable(
            SqlRecordSetInfo.CreateTemporary( name ),
            columnDefinitions.ToArray(),
            ifNotExists,
            constraintsProvider is not null ? t => constraintsProvider( table, t ) : null );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCreateTableNode"/> based on the provided <paramref name="table"/> and its columns.
    /// </summary>
    /// <param name="table">Table to create temporary table from.</param>
    /// <param name="name">Name of the temporary table.</param>
    /// <param name="columnOverride">
    /// Optional delegate which allows to either override the default column definition or skip the column entirely.
    /// </param>
    /// <param name="ifNotExists">
    /// Specifies whether the temporary table should only be created if it does not already exist in DB. Equal to <b>false</b> by default.
    /// </param>
    /// <param name="constraintsProvider">Optional <see cref="SqlCreateTableConstraints"/> provider.</param>
    /// <returns>New <see cref="SqlCreateTableNode"/> instance.</returns>
    /// <remarks>Default values, computations and identities are not included in default column definitions.</remarks>
    [Pure]
    public static SqlCreateTableNode CreateTempTable(
        this ISqlTableBuilder table,
        string name,
        Func<ISqlColumnBuilder, SqlColumnDefinitionOverride>? columnOverride = null,
        bool ifNotExists = false,
        Func<ISqlTableBuilder, SqlNewTableNode, SqlCreateTableConstraints>? constraintsProvider = null)
    {
        var columns = table.Node.GetKnownFields();

        var columnDefinitions = new List<SqlColumnDefinitionNode>( capacity: columns.Count );
        foreach ( var c in columns )
        {
            SqlColumnDefinitionNode? definition = null;
            if ( columnOverride is not null )
            {
                var @override = columnOverride( c.Value );
                if ( @override.IsIgnored )
                    continue;

                definition = @override.Node;
            }

            definition ??= SqlNode.Column( c.Value.Name, c.Value.TypeDefinition, c.Value.IsNullable );
            columnDefinitions.Add( definition );
        }

        return SqlNode.CreateTable(
            SqlRecordSetInfo.CreateTemporary( name ),
            columnDefinitions.ToArray(),
            ifNotExists,
            constraintsProvider is not null ? t => constraintsProvider( table, t ) : null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqlConditionNode GetPrimaryKeyFilter(IReadOnlyList<SqlIndexed<ISqlColumn>> pkColumns)
    {
        var firstColumn = pkColumns[0].Column;
        Assume.IsNotNull( firstColumn );

        SqlConditionNode filter = firstColumn.Node.IsEqualToParameter();
        foreach ( var c in pkColumns.Skip( 1 ) )
        {
            Assume.IsNotNull( c.Column );
            filter = filter.And( c.Column.Node.IsEqualToParameter() );
        }

        return filter;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqlConditionNode GetPrimaryKeyFilter(ISqlTableBuilder table, ReadOnlyArray<SqlOrderByNode> pkExpressions)
    {
        var expr = pkExpressions[0].Expression;
        SqlConditionNode filter = SqlHelpers.CastOrThrow<SqlColumnBuilderNode>( table.Database, expr ).IsEqualToParameter();
        for ( var i = 1; i < pkExpressions.Count; ++i )
        {
            expr = pkExpressions[i].Expression;
            filter = filter.And( SqlHelpers.CastOrThrow<SqlColumnBuilderNode>( table.Database, expr ).IsEqualToParameter() );
        }

        return filter;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqlConditionNode GetPrimaryKeyFilter(ReadOnlyArray<SqlOrderByNode> pkExpressions)
    {
        var column = GetDataFieldOrThrow( pkExpressions[0] );
        SqlConditionNode filter = column.IsEqualTo( SqlNode.Parameter( column.Name, column.Type ) );
        for ( var i = 1; i < pkExpressions.Count; ++i )
        {
            column = GetDataFieldOrThrow( pkExpressions[i] );
            filter = filter.And( column.IsEqualTo( SqlNode.Parameter( column.Name, column.Type ) ) );
        }

        return filter;
    }

    [Pure]
    private static bool IsInPrimaryKey(IReadOnlyList<SqlIndexed<ISqlColumn>> primaryKeyColumns, ISqlColumn column)
    {
        foreach ( var c in primaryKeyColumns )
        {
            if ( ReferenceEquals( c.Column, column ) )
                return true;
        }

        return false;
    }

    [Pure]
    private static bool IsInPrimaryKey(ReadOnlyArray<SqlOrderByNode> primaryKeyExpressions, ISqlColumnBuilder column)
    {
        foreach ( var c in primaryKeyExpressions )
        {
            if ( c.Expression is SqlColumnBuilderNode node && ReferenceEquals( node.Value, column ) )
                return true;
        }

        return false;
    }

    [Pure]
    private static bool IsInPrimaryKey(ReadOnlyArray<SqlOrderByNode> primaryKeyExpressions, SqlDataFieldNode column)
    {
        foreach ( var c in primaryKeyExpressions )
        {
            if ( c.Expression is SqlDataFieldNode node && ReferenceEquals( node, column ) )
                return true;
        }

        return false;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqlRawDataFieldNode GetDataFieldOrThrow(SqlOrderByNode orderBy)
    {
        if ( orderBy.Expression is SqlRawDataFieldNode field )
            return field;

        ExceptionThrower.Throw( new InvalidOperationException( ExceptionResources.PrimaryKeyExpressionIsNotDataField ) );
        return default;
    }
}
