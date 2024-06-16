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
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Contains various extension methods related to <see cref="SqlNodeBase"/> type.
/// </summary>
public static class SqlNodeExtensions
{
    /// <summary>
    /// Creates a new <see cref="SqlTableNode"/> instance or returns table's <see cref="ISqlTable.Node"/>
    /// when provided <paramref name="alias"/> is null.
    /// </summary>
    /// <param name="table">Underlying <see cref="ISqlTable"/> instance.</param>
    /// <param name="alias">Optional alias of this record set. Equal to null by default.</param>
    /// <returns>
    /// New <see cref="SqlTableNode"/> instance or table's <see cref="ISqlTable.Node"/>
    /// when provided <paramref name="alias"/> is null.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTableNode ToRecordSet(this ISqlTable table, string? alias = null)
    {
        return alias is null ? table.Node : SqlNode.Table( table, alias );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTableBuilderNode"/> instance or returns table's <see cref="ISqlTableBuilder.Node"/>
    /// when provided <paramref name="alias"/> is null.
    /// </summary>
    /// <param name="table">Underlying <see cref="ISqlTableBuilder"/> instance.</param>
    /// <param name="alias">Optional alias of this record set. Equal to null by default.</param>
    /// <returns>
    /// New <see cref="SqlTableBuilderNode"/> instance or table's <see cref="ISqlTableBuilder.Node"/>
    /// when provided <paramref name="alias"/> is null.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTableBuilderNode ToRecordSet(this ISqlTableBuilder table, string? alias = null)
    {
        return alias is null ? table.Node : SqlNode.Table( table, alias );
    }

    /// <summary>
    /// Creates a new <see cref="SqlViewNode"/> instance or returns view's <see cref="ISqlView.Node"/>
    /// when provided <paramref name="alias"/> is null.
    /// </summary>
    /// <param name="view">Underlying <see cref="ISqlView"/> instance.</param>
    /// <param name="alias">Optional alias of this record set. Equal to null by default.</param>
    /// <returns>
    /// New <see cref="SqlViewNode"/> instance or view's <see cref="ISqlView.Node"/>
    /// when provided <paramref name="alias"/> is null.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlViewNode ToRecordSet(this ISqlView view, string? alias = null)
    {
        return alias is null ? view.Node : SqlNode.View( view, alias );
    }

    /// <summary>
    /// Creates a new <see cref="SqlViewBuilderNode"/> instance or returns view's <see cref="ISqlViewBuilder.Node"/>
    /// when provided <paramref name="alias"/> is null.
    /// </summary>
    /// <param name="view">Underlying <see cref="ISqlViewBuilder"/> instance.</param>
    /// <param name="alias">Optional alias of this record set. Equal to null by default.</param>
    /// <returns>
    /// New <see cref="SqlViewBuilderNode"/> instance or view's <see cref="ISqlViewBuilder.Node"/>
    /// when provided <paramref name="alias"/> is null.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlViewBuilderNode ToRecordSet(this ISqlViewBuilder view, string? alias = null)
    {
        return alias is null ? view.Node : SqlNode.View( view, alias );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNewTableNode"/> instance or returns created table's <see cref="SqlCreateTableNode.RecordSet"/>
    /// when provided <paramref name="alias"/> is null.
    /// </summary>
    /// <param name="node">Underlying <see cref="SqlCreateTableNode"/> instance.</param>
    /// <param name="alias">Optional alias of this record set. Equal to null by default.</param>
    /// <returns>
    /// New <see cref="SqlNewTableNode"/> instance or created table's <see cref="SqlCreateTableNode.RecordSet"/>
    /// when provided <paramref name="alias"/> is null.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNewTableNode AsSet(this SqlCreateTableNode node, string? alias = null)
    {
        return alias is null ? node.RecordSet : SqlNode.NewTableRecordSet( node, alias );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNewViewNode"/> instance.
    /// </summary>
    /// <param name="node">Underlying <see cref="SqlCreateViewNode"/> instance.</param>
    /// <param name="alias">Optional alias of this record set. Equal to null by default.</param>
    /// <returns>New <see cref="SqlNewViewNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNewViewNode AsSet(this SqlCreateViewNode node, string? alias = null)
    {
        return SqlNode.NewViewRecordSet( node, alias );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNamedFunctionRecordSetNode"/> instance.
    /// </summary>
    /// <param name="node">Underlying <see cref="SqlNamedFunctionExpressionNode"/> instance.</param>
    /// <param name="alias">Alias of this record set.</param>
    /// <returns>New <see cref="SqlNamedFunctionRecordSetNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNamedFunctionRecordSetNode AsSet(this SqlNamedFunctionExpressionNode node, string alias)
    {
        return SqlNode.NamedFunctionRecordSet( node, alias );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSelectFieldNode"/> instance.
    /// </summary>
    /// <param name="node">Selected expression.</param>
    /// <param name="alias">Alias of the selected expression.</param>
    /// <returns>New <see cref="SqlSelectFieldNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSelectFieldNode As(this SqlExpressionNode node, string alias)
    {
        return SqlNode.Select( node, alias );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSelectFieldNode"/> instance without an alias.
    /// </summary>
    /// <param name="node">Selected data field.</param>
    /// <returns>New <see cref="SqlSelectFieldNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSelectFieldNode AsSelf(this SqlDataFieldNode node)
    {
        return SqlNode.Select( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSelectFieldNode"/> instance.
    /// </summary>
    /// <param name="node">Selected condition that will be converted to an expression.</param>
    /// <param name="alias">Alias of the selected expression.</param>
    /// <returns>New <see cref="SqlSelectFieldNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSelectFieldNode As(this SqlConditionNode node, string alias)
    {
        return node.ToValue().As( alias );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceQueryExpressionNode{SqlDummyDataSourceNode}"/> instance.
    /// </summary>
    /// <param name="node">Single data field selection to include in query's selection.</param>
    /// <returns>New <see cref="SqlDataSourceQueryExpressionNode{SqlDummyDataSourceNode}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<SqlDummyDataSourceNode> ToQuery(this SqlSelectFieldNode node)
    {
        return SqlNode.DummyDataSource().Select( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlValueAssignmentNode"/> instance.
    /// </summary>
    /// <param name="node">Data field to assign value to.</param>
    /// <param name="value">Value to assign.</param>
    /// <returns>New <see cref="SqlValueAssignmentNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlValueAssignmentNode Assign(this SqlDataFieldNode node, SqlExpressionNode value)
    {
        return SqlNode.ValueAssignment( node, value );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSelectExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Underlying selection.</param>
    /// <returns>New <see cref="SqlSelectExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSelectExpressionNode ToExpression(this SqlSelectNode node)
    {
        return SqlNode.SelectExpression( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSingleDataSourceNode{TRecordSetNode}"/> instance.
    /// </summary>
    /// <param name="node"><see cref="SqlRecordSetNode"/> instance from which this data source's definition begins.</param>
    /// <typeparam name="TRecordSetNode">SQL record set node type.</typeparam>
    /// <returns>New <see cref="SqlSingleDataSourceNode{TRecordSetNode}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSingleDataSourceNode<TRecordSetNode> ToDataSource<TRecordSetNode>(this TRecordSetNode node)
        where TRecordSetNode : SqlRecordSetNode
    {
        return SqlNode.SingleDataSource( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiDataSourceNode"/> instance.
    /// </summary>
    /// <param name="node">First <see cref="SqlRecordSetNode"/> instance from which this data source's definition begins.</param>
    /// <param name="joins">
    /// Sequential collection of all <see cref="SqlDataSourceJoinOnNode"/> instances that define this data source.
    /// </param>
    /// <returns>New <see cref="SqlMultiDataSourceNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlRecordSetNode node, IEnumerable<SqlDataSourceJoinOnNode> joins)
    {
        return node.Join( joins.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiDataSourceNode"/> instance.
    /// </summary>
    /// <param name="node">First <see cref="SqlRecordSetNode"/> instance from which this data source's definition begins.</param>
    /// <param name="joins">
    /// Sequential collection of all <see cref="SqlDataSourceJoinOnNode"/> instances that define this data source.
    /// </param>
    /// <returns>New <see cref="SqlMultiDataSourceNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlRecordSetNode node, params SqlDataSourceJoinOnNode[] joins)
    {
        return SqlNode.Join( node, joins );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiDataSourceNode"/> instance.
    /// </summary>
    /// <param name="node">First <see cref="SqlRecordSetNode"/> instance from which this data source's definition begins.</param>
    /// <param name="definitions">
    /// Sequential collection of all <see cref="SqlJoinDefinition"/> instances that define this data source.
    /// </param>
    /// <returns>New <see cref="SqlMultiDataSourceNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlRecordSetNode node, IEnumerable<SqlJoinDefinition> definitions)
    {
        return node.Join( definitions.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiDataSourceNode"/> instance.
    /// </summary>
    /// <param name="node">First <see cref="SqlRecordSetNode"/> instance from which this data source's definition begins.</param>
    /// <param name="definitions">
    /// Sequential collection of all <see cref="SqlJoinDefinition"/> instances that define this data source.
    /// </param>
    /// <returns>New <see cref="SqlMultiDataSourceNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlRecordSetNode node, params SqlJoinDefinition[] definitions)
    {
        return SqlNode.Join( node, definitions );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiDataSourceNode"/> instance.
    /// </summary>
    /// <param name="node"><see cref="SqlDataSourceNode"/> instance from which this data source's definition begins.</param>
    /// <param name="joins">
    /// Sequential collection of all <see cref="SqlDataSourceJoinOnNode"/> instances that define this data source.
    /// </param>
    /// <returns>New <see cref="SqlMultiDataSourceNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlDataSourceNode node, IEnumerable<SqlDataSourceJoinOnNode> joins)
    {
        return node.Join( joins.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiDataSourceNode"/> instance.
    /// </summary>
    /// <param name="node"><see cref="SqlDataSourceNode"/> instance from which this data source's definition begins.</param>
    /// <param name="joins">
    /// Sequential collection of all <see cref="SqlDataSourceJoinOnNode"/> instances that define this data source.
    /// </param>
    /// <returns>New <see cref="SqlMultiDataSourceNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlDataSourceNode node, params SqlDataSourceJoinOnNode[] joins)
    {
        return SqlNode.Join( node, joins );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiDataSourceNode"/> instance.
    /// </summary>
    /// <param name="node"><see cref="SqlDataSourceNode"/> instance from which this data source's definition begins.</param>
    /// <param name="definitions">
    /// Sequential collection of all <see cref="SqlJoinDefinition"/> instances that define this data source.
    /// </param>
    /// <returns>New <see cref="SqlMultiDataSourceNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlDataSourceNode node, IEnumerable<SqlJoinDefinition> definitions)
    {
        return node.Join( definitions.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiDataSourceNode"/> instance.
    /// </summary>
    /// <param name="node"><see cref="SqlDataSourceNode"/> instance from which this data source's definition begins.</param>
    /// <param name="definitions">
    /// Sequential collection of all <see cref="SqlJoinDefinition"/> instances that define this data source.
    /// </param>
    /// <returns>New <see cref="SqlMultiDataSourceNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlDataSourceNode node, params SqlJoinDefinition[] definitions)
    {
        return SqlNode.Join( node, definitions );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceJoinOnNode"/> instance with <see cref="SqlJoinType.Inner"/> type.
    /// </summary>
    /// <param name="node">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <param name="onExpression">Condition of this join operation.</param>
    /// <returns>New <see cref="SqlDataSourceJoinOnNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceJoinOnNode InnerOn(this SqlRecordSetNode node, SqlConditionNode onExpression)
    {
        return SqlNode.InnerJoinOn( node, onExpression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceJoinOnNode"/> instance with <see cref="SqlJoinType.Left"/> type.
    /// </summary>
    /// <param name="node">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <param name="onExpression">Condition of this join operation.</param>
    /// <returns>New <see cref="SqlDataSourceJoinOnNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceJoinOnNode LeftOn(this SqlRecordSetNode node, SqlConditionNode onExpression)
    {
        return SqlNode.LeftJoinOn( node, onExpression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceJoinOnNode"/> instance with <see cref="SqlJoinType.Right"/> type.
    /// </summary>
    /// <param name="node">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <param name="onExpression">Condition of this join operation.</param>
    /// <returns>New <see cref="SqlDataSourceJoinOnNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceJoinOnNode RightOn(this SqlRecordSetNode node, SqlConditionNode onExpression)
    {
        return SqlNode.RightJoinOn( node, onExpression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceJoinOnNode"/> instance with <see cref="SqlJoinType.Full"/> type.
    /// </summary>
    /// <param name="node">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <param name="onExpression">Condition of this join operation.</param>
    /// <returns>New <see cref="SqlDataSourceJoinOnNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceJoinOnNode FullOn(this SqlRecordSetNode node, SqlConditionNode onExpression)
    {
        return SqlNode.FullJoinOn( node, onExpression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceJoinOnNode"/> instance with <see cref="SqlJoinType.Cross"/> type.
    /// </summary>
    /// <param name="node">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <returns>New <see cref="SqlDataSourceJoinOnNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceJoinOnNode Cross(this SqlRecordSetNode node)
    {
        return SqlNode.CrossJoin( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSelectRecordSetNode"/> instance.
    /// </summary>
    /// <param name="node">Single record set to select all data fields from.</param>
    /// <returns>New <see cref="SqlSelectRecordSetNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSelectRecordSetNode GetAll(this SqlRecordSetNode node)
    {
        return SqlNode.SelectAll( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawDataFieldNode"/> instance.
    /// </summary>
    /// <param name="node"><see cref="SqlRecordSetNode"/> that this data field belongs to.</param>
    /// <param name="name">Name of this data field.</param>
    /// <param name="type">Optional runtime type of this data field. Equal to null by default.</param>
    /// <returns>New <see cref="SqlRawDataFieldNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRawDataFieldNode GetRawField(this SqlRecordSetNode node, string name, TypeNullability? type)
    {
        return SqlNode.RawDataField( node, name, type );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTypeCastExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Underlying value to cast to a different type.</param>
    /// <typeparam name="T">Target runtime type.</typeparam>
    /// <returns>New <see cref="SqlTypeCastExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTypeCastExpressionNode CastTo<T>(this SqlExpressionNode node)
    {
        return node.CastTo( typeof( T ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTypeCastExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Underlying value to cast to a different type.</param>
    /// <param name="type">Target runtime type.</param>
    /// <returns>New <see cref="SqlTypeCastExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTypeCastExpressionNode CastTo(this SqlExpressionNode node, Type type)
    {
        return SqlNode.TypeCast( node, type );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTypeCastExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Underlying value to cast to a different type.</param>
    /// <param name="typeDefinition"><see cref="ISqlColumnTypeDefinition"/> instance that defines the target type.</param>
    /// <returns>New <see cref="SqlTypeCastExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTypeCastExpressionNode CastTo(this SqlExpressionNode node, ISqlColumnTypeDefinition typeDefinition)
    {
        return SqlNode.TypeCast( node, typeDefinition );
    }

    /// <summary>
    /// Creates a new <see cref="SqlConditionValueNode"/> instance.
    /// </summary>
    /// <param name="node">Underlying condition.</param>
    /// <returns>New <see cref="SqlConditionValueNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConditionValueNode ToValue(this SqlConditionNode node)
    {
        return SqlNode.Value( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNegateExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Operand.</param>
    /// <returns>New <see cref="SqlNegateExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNegateExpressionNode Negate(this SqlExpressionNode node)
    {
        return SqlNode.Negate( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseNotExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Operand.</param>
    /// <returns>New <see cref="SqlBitwiseNotExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBitwiseNotExpressionNode BitwiseNot(this SqlExpressionNode node)
    {
        return SqlNode.BitwiseNot( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlAddExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlAddExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAddExpressionNode Add(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.Add( node, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlAddExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlAddExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConcatExpressionNode Concat(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.Concat( node, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSubtractExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlSubtractExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSubtractExpressionNode Subtract(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.Subtract( node, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiplyExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlMultiplyExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiplyExpressionNode Multiply(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.Multiply( node, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDivideExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlDivideExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDivideExpressionNode Divide(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.Divide( node, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlModuloExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlModuloExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlModuloExpressionNode Modulo(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.Modulo( node, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseAndExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlBitwiseAndExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBitwiseAndExpressionNode BitwiseAnd(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.BitwiseAnd( node, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseOrExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlBitwiseOrExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBitwiseOrExpressionNode BitwiseOr(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.BitwiseOr( node, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseXorExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlBitwiseXorExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBitwiseXorExpressionNode BitwiseXor(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.BitwiseXor( node, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseLeftShiftExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlBitwiseLeftShiftExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBitwiseLeftShiftExpressionNode BitwiseLeftShift(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.BitwiseLeftShift( node, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseRightShiftExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlBitwiseRightShiftExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBitwiseRightShiftExpressionNode BitwiseRightShift(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.BitwiseRightShift( node, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlEqualToConditionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlEqualToConditionNode"/> instance.</returns>
    /// <remarks>Null values will be replaced with <see cref="SqlNullNode"/> instances.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlEqualToConditionNode IsEqualTo(this SqlExpressionNode? node, SqlExpressionNode? right)
    {
        return SqlNode.EqualTo( node ?? SqlNode.Null(), right ?? SqlNode.Null() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNotEqualToConditionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlNotEqualToConditionNode"/> instance.</returns>
    /// <remarks>Null values will be replaced with <see cref="SqlNullNode"/> instances.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNotEqualToConditionNode IsNotEqualTo(this SqlExpressionNode? node, SqlExpressionNode? right)
    {
        return SqlNode.NotEqualTo( node ?? SqlNode.Null(), right ?? SqlNode.Null() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlGreaterThanConditionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlGreaterThanConditionNode"/> instance.</returns>
    /// <remarks>Null values will be replaced with <see cref="SqlNullNode"/> instances.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlGreaterThanConditionNode IsGreaterThan(this SqlExpressionNode? node, SqlExpressionNode? right)
    {
        return SqlNode.GreaterThan( node ?? SqlNode.Null(), right ?? SqlNode.Null() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLessThanConditionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlLessThanConditionNode"/> instance.</returns>
    /// <remarks>Null values will be replaced with <see cref="SqlNullNode"/> instances.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLessThanConditionNode IsLessThan(this SqlExpressionNode? node, SqlExpressionNode? right)
    {
        return SqlNode.LessThan( node ?? SqlNode.Null(), right ?? SqlNode.Null() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlGreaterThanOrEqualToConditionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlGreaterThanOrEqualToConditionNode"/> instance.</returns>
    /// <remarks>Null values will be replaced with <see cref="SqlNullNode"/> instances.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlGreaterThanOrEqualToConditionNode IsGreaterThanOrEqualTo(this SqlExpressionNode? node, SqlExpressionNode? right)
    {
        return SqlNode.GreaterThanOrEqualTo( node ?? SqlNode.Null(), right ?? SqlNode.Null() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLessThanOrEqualToConditionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlLessThanOrEqualToConditionNode"/> instance.</returns>
    /// <remarks>Null values will be replaced with <see cref="SqlNullNode"/> instances.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLessThanOrEqualToConditionNode IsLessThanOrEqualTo(this SqlExpressionNode? node, SqlExpressionNode? right)
    {
        return SqlNode.LessThanOrEqualTo( node ?? SqlNode.Null(), right ?? SqlNode.Null() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBetweenConditionNode"/> instance.
    /// </summary>
    /// <param name="node">Value to check.</param>
    /// <param name="min">Minimum acceptable value.</param>
    /// <param name="max">Maximum acceptable value.</param>
    /// <returns>New <see cref="SqlBetweenConditionNode"/> instance.</returns>
    /// <remarks>Null values will be replaced with <see cref="SqlNullNode"/> instances.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBetweenConditionNode IsBetween(this SqlExpressionNode? node, SqlExpressionNode? min, SqlExpressionNode? max)
    {
        return SqlNode.Between( node ?? SqlNode.Null(), min ?? SqlNode.Null(), max ?? SqlNode.Null() );
    }

    /// <summary>
    /// Creates a new negated <see cref="SqlBetweenConditionNode"/> instance.
    /// </summary>
    /// <param name="node">Value to check.</param>
    /// <param name="min">Minimum acceptable value.</param>
    /// <param name="max">Maximum acceptable value.</param>
    /// <returns>New negated <see cref="SqlBetweenConditionNode"/> instance.</returns>
    /// <remarks>Null values will be replaced with <see cref="SqlNullNode"/> instances.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBetweenConditionNode IsNotBetween(this SqlExpressionNode? node, SqlExpressionNode? min, SqlExpressionNode? max)
    {
        return SqlNode.NotBetween( node ?? SqlNode.Null(), min ?? SqlNode.Null(), max ?? SqlNode.Null() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLikeConditionNode"/> instance.
    /// </summary>
    /// <param name="node">Value to check.</param>
    /// <param name="pattern">String pattern to check the value against.</param>
    /// <param name="escape">Optional escape character for the pattern. Equal to null by default.</param>
    /// <returns>New <see cref="SqlLikeConditionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLikeConditionNode Like(this SqlExpressionNode node, SqlExpressionNode pattern, SqlExpressionNode? escape = null)
    {
        return SqlNode.Like( node, pattern, escape );
    }

    /// <summary>
    /// Creates a new negated <see cref="SqlLikeConditionNode"/> instance.
    /// </summary>
    /// <param name="node">Value to check.</param>
    /// <param name="pattern">String pattern to check the value against.</param>
    /// <param name="escape">Optional escape character for the pattern. Equal to null by default.</param>
    /// <returns>New negated <see cref="SqlLikeConditionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLikeConditionNode NotLike(this SqlExpressionNode node, SqlExpressionNode pattern, SqlExpressionNode? escape = null)
    {
        return SqlNode.NotLike( node, pattern, escape );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLikeConditionNode"/> instance with changed <see cref="SqlLikeConditionNode.Escape"/>.
    /// </summary>
    /// <param name="node">Value to check.</param>
    /// <param name="escape">Escape character for the pattern.</param>
    /// <returns>New <see cref="SqlLikeConditionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLikeConditionNode Escape(this SqlLikeConditionNode node, SqlExpressionNode escape)
    {
        return new SqlLikeConditionNode( node.Value, node.Pattern, escape, node.IsNegated );
    }

    /// <summary>
    /// Creates a new <see cref="SqlExistsConditionNode"/> instance.
    /// </summary>
    /// <param name="node">Record set of the sub-query to check.</param>
    /// <returns>New <see cref="SqlExistsConditionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExistsConditionNode Exists(this SqlRecordSetNode node)
    {
        return node.ToDataSource().Exists();
    }

    /// <summary>
    /// Creates a new negated <see cref="SqlExistsConditionNode"/> instance.
    /// </summary>
    /// <param name="node">Record set of the sub-query to check.</param>
    /// <returns>New negated <see cref="SqlExistsConditionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExistsConditionNode NotExists(this SqlRecordSetNode node)
    {
        return node.ToDataSource().NotExists();
    }

    /// <summary>
    /// Creates a new <see cref="SqlInConditionNode"/> instance or <see cref="SqlFalseNode"/>
    /// when <paramref name="expressions"/> are empty.
    /// </summary>
    /// <param name="node">Value to check.</param>
    /// <param name="expressions">Collection of values that the value is compared against.</param>
    /// <returns>New <see cref="SqlInConditionNode"/> instance or <see cref="SqlFalseNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConditionNode In(this SqlExpressionNode node, IEnumerable<SqlExpressionNode> expressions)
    {
        return node.In( expressions.ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlInConditionNode"/> instance or <see cref="SqlFalseNode"/>
    /// when <paramref name="expressions"/> are empty.
    /// </summary>
    /// <param name="node">Value to check.</param>
    /// <param name="expressions">Collection of values that the value is compared against.</param>
    /// <returns>New <see cref="SqlInConditionNode"/> instance or <see cref="SqlFalseNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConditionNode In(this SqlExpressionNode node, params SqlExpressionNode[] expressions)
    {
        return SqlNode.In( node, expressions );
    }

    /// <summary>
    /// Creates a new negated <see cref="SqlInConditionNode"/> instance or <see cref="SqlTrueNode"/>
    /// when <paramref name="expressions"/> are empty.
    /// </summary>
    /// <param name="node">Value to check.</param>
    /// <param name="expressions">Collection of values that the value is compared against.</param>
    /// <returns>New negated <see cref="SqlInConditionNode"/> instance or <see cref="SqlTrueNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConditionNode NotIn(this SqlExpressionNode node, IEnumerable<SqlExpressionNode> expressions)
    {
        return node.NotIn( expressions.ToArray() );
    }

    /// <summary>
    /// Creates a new negated <see cref="SqlInConditionNode"/> instance or <see cref="SqlTrueNode"/>
    /// when <paramref name="expressions"/> are empty.
    /// </summary>
    /// <param name="node">Value to check.</param>
    /// <param name="expressions">Collection of values that the value is compared against.</param>
    /// <returns>New negated <see cref="SqlInConditionNode"/> instance or <see cref="SqlTrueNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConditionNode NotIn(this SqlExpressionNode node, params SqlExpressionNode[] expressions)
    {
        return SqlNode.NotIn( node, expressions );
    }

    /// <summary>
    /// Creates a new <see cref="SqlInQueryConditionNode"/> instance.
    /// </summary>
    /// <param name="node">Value to check.</param>
    /// <param name="query">Sub-query that the value is compared against.</param>
    /// <returns>New <see cref="SqlInQueryConditionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlInQueryConditionNode InQuery(this SqlExpressionNode node, SqlQueryExpressionNode query)
    {
        return SqlNode.InQuery( node, query );
    }

    /// <summary>
    /// Creates a new negated <see cref="SqlInQueryConditionNode"/> instance.
    /// </summary>
    /// <param name="node">Value to check.</param>
    /// <param name="query">Sub-query that the value is compared against.</param>
    /// <returns>New negated <see cref="SqlInQueryConditionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlInQueryConditionNode NotInQuery(this SqlExpressionNode node, SqlQueryExpressionNode query)
    {
        return SqlNode.NotInQuery( node, query );
    }

    /// <summary>
    /// Creates a new <see cref="SqlAndConditionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlAndConditionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAndConditionNode And(this SqlConditionNode node, SqlConditionNode right)
    {
        return SqlNode.And( node, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlOrConditionNode"/> instance.
    /// </summary>
    /// <param name="node">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlOrConditionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrConditionNode Or(this SqlConditionNode node, SqlConditionNode right)
    {
        return SqlNode.Or( node, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSwitchCaseNode"/> instance.
    /// </summary>
    /// <param name="node">Underlying condition.</param>
    /// <param name="value">Underlying expression.</param>
    /// <returns>New <see cref="SqlSwitchCaseNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSwitchCaseNode Then(this SqlConditionNode node, SqlExpressionNode value)
    {
        return SqlNode.SwitchCase( node, value );
    }

    /// <summary>
    /// Creates a new <see cref="SqlOrderByNode"/> instance with <see cref="Sql.OrderBy.Asc"/> ordering.
    /// </summary>
    /// <param name="node">Underlying expression.</param>
    /// <returns>New <see cref="SqlOrderByNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrderByNode Asc(this SqlExpressionNode node)
    {
        return SqlNode.OrderByAsc( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlOrderByNode"/> instance with <see cref="Sql.OrderBy.Desc"/> ordering.
    /// </summary>
    /// <param name="node">Underlying expression.</param>
    /// <returns>New <see cref="SqlOrderByNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrderByNode Desc(this SqlExpressionNode node)
    {
        return SqlNode.OrderByDesc( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlOrderByNode"/> instance with <see cref="Sql.OrderBy.Asc"/> ordering.
    /// </summary>
    /// <param name="node">Underlying selection.</param>
    /// <returns>New <see cref="SqlOrderByNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrderByNode Asc(this SqlSelectNode node)
    {
        return SqlNode.OrderByAsc( node.ToExpression() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlOrderByNode"/> instance with <see cref="Sql.OrderBy.Desc"/> ordering.
    /// </summary>
    /// <param name="node">Underlying selection.</param>
    /// <returns>New <see cref="SqlOrderByNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrderByNode Desc(this SqlSelectNode node)
    {
        return SqlNode.OrderByDesc( node.ToExpression() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCoalesceFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First argument of the function.</param>
    /// <param name="other">Collection of following function's arguments.</param>
    /// <returns>New <see cref="SqlCoalesceFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCoalesceFunctionExpressionNode Coalesce(this SqlExpressionNode node, params SqlExpressionNode[] other)
    {
        var nodes = new SqlExpressionNode[other.Length + 1];
        nodes[0] = node;
        other.CopyTo( nodes, index: 1 );
        return SqlNode.Functions.Coalesce( nodes );
    }

    /// <summary>
    /// Creates a new <see cref="SqlExtractDateFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to extract date part from.</param>
    /// <returns>New <see cref="SqlExtractDateFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExtractDateFunctionExpressionNode ExtractDate(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ExtractDate( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlExtractTimeOfDayFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to extract time of day part from.</param>
    /// <returns>New <see cref="SqlExtractTimeOfDayFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExtractTimeOfDayFunctionExpressionNode ExtractTimeOfDay(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ExtractTimeOfDay( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlExtractDayFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to extract day of year component from.</param>
    /// <returns>New <see cref="SqlExtractDayFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExtractDayFunctionExpressionNode ExtractDayOfYear(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ExtractDayOfYear( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlExtractDayFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to extract day of month component from.</param>
    /// <returns>New <see cref="SqlExtractDayFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExtractDayFunctionExpressionNode ExtractDayOfMonth(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ExtractDayOfMonth( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlExtractDayFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to extract day of week component from.</param>
    /// <returns>New <see cref="SqlExtractDayFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExtractDayFunctionExpressionNode ExtractDayOfWeek(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ExtractDayOfWeek( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlExtractTemporalUnitFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to extract the desired date or time component from.</param>
    /// <param name="unit"><see cref="SqlTemporalUnit"/> that specifies the date or time component to extract.</param>
    /// <returns>New <see cref="SqlExtractTemporalUnitFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExtractTemporalUnitFunctionExpressionNode ExtractTemporalUnit(this SqlExpressionNode node, SqlTemporalUnit unit)
    {
        return SqlNode.Functions.ExtractTemporalUnit( node, unit );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTemporalAddFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to add value to.</param>
    /// <param name="value">Value to add.</param>
    /// <param name="unit"><see cref="SqlTemporalUnit"/> that specifies the unit of the added value.</param>
    /// <returns>New <see cref="SqlTemporalAddFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTemporalAddFunctionExpressionNode TemporalAdd(
        this SqlExpressionNode node,
        SqlExpressionNode value,
        SqlTemporalUnit unit)
    {
        return SqlNode.Functions.TemporalAdd( node, value, unit );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTemporalDiffFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression that defines the start value.</param>
    /// <param name="end">Expression that defines the end value.</param>
    /// <param name="unit"><see cref="SqlTemporalUnit"/> that specifies the unit of the returned result.</param>
    /// <returns>New <see cref="SqlTemporalDiffFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTemporalDiffFunctionExpressionNode TemporalDiff(
        this SqlExpressionNode node,
        SqlExpressionNode end,
        SqlTemporalUnit unit)
    {
        return SqlNode.Functions.TemporalDiff( node, end, unit );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLengthFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate length from.</param>
    /// <returns>New <see cref="SqlLengthFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLengthFunctionExpressionNode Length(this SqlExpressionNode node)
    {
        return SqlNode.Functions.Length( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlByteLengthFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate byte length from.</param>
    /// <returns>New <see cref="SqlByteLengthFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlByteLengthFunctionExpressionNode ByteLength(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ByteLength( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlToLowerFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to convert to lowercase.</param>
    /// <returns>New <see cref="SqlToLowerFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlToLowerFunctionExpressionNode ToLower(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ToLower( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlToUpperFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to convert to uppercase.</param>
    /// <returns>New <see cref="SqlToUpperFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlToUpperFunctionExpressionNode ToUpper(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ToUpper( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTrimStartFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to trim at the start.</param>
    /// <param name="characters">Optional characters to trim away. Equal to null by default.</param>
    /// <returns>New <see cref="SqlTrimStartFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTrimStartFunctionExpressionNode TrimStart(this SqlExpressionNode node, SqlExpressionNode? characters = null)
    {
        return SqlNode.Functions.TrimStart( node, characters );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTrimEndFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to trim at the end.</param>
    /// <param name="characters">Optional characters to trim away. Equal to null by default.</param>
    /// <returns>New <see cref="SqlTrimEndFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTrimEndFunctionExpressionNode TrimEnd(this SqlExpressionNode node, SqlExpressionNode? characters = null)
    {
        return SqlNode.Functions.TrimEnd( node, characters );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTrimFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to trim at both ends.</param>
    /// <param name="characters">Optional characters to trim away. Equal to null by default.</param>
    /// <returns>New <see cref="SqlTrimFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTrimFunctionExpressionNode Trim(this SqlExpressionNode node, SqlExpressionNode? characters = null)
    {
        return SqlNode.Functions.Trim( node, characters );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSubstringFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to extract a substring from.</param>
    /// <param name="startIndex">Position of the first character of the substring.</param>
    /// <param name="length">Optional length of the substring. Equal to null by default.</param>
    /// <returns>New <see cref="SqlSubstringFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSubstringFunctionExpressionNode Substring(
        this SqlExpressionNode node,
        SqlExpressionNode startIndex,
        SqlExpressionNode? length = null)
    {
        return SqlNode.Functions.Substring( node, startIndex, length );
    }

    /// <summary>
    /// Creates a new <see cref="SqlReplaceFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to replace occurrences in.</param>
    /// <param name="oldValue">Value to replace.</param>
    /// <param name="newValue">Replacement value.</param>
    /// <returns>New <see cref="SqlReplaceFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlReplaceFunctionExpressionNode Replace(
        this SqlExpressionNode node,
        SqlExpressionNode oldValue,
        SqlExpressionNode newValue)
    {
        return SqlNode.Functions.Replace( node, oldValue, newValue );
    }

    /// <summary>
    /// Creates a new <see cref="SqlReverseFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to reverse.</param>
    /// <returns>New <see cref="SqlReverseFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlReverseFunctionExpressionNode Reverse(this SqlExpressionNode node)
    {
        return SqlNode.Functions.Reverse( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlIndexOfFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to find the first occurrence in.</param>
    /// <param name="value">Value to search for.</param>
    /// <returns>New <see cref="SqlIndexOfFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexOfFunctionExpressionNode IndexOf(this SqlExpressionNode node, SqlExpressionNode value)
    {
        return SqlNode.Functions.IndexOf( node, value );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLastIndexOfFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to find the last occurrence in.</param>
    /// <param name="value">Value to search for.</param>
    /// <returns>New <see cref="SqlLastIndexOfFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLastIndexOfFunctionExpressionNode LastIndexOf(this SqlExpressionNode node, SqlExpressionNode value)
    {
        return SqlNode.Functions.LastIndexOf( node, value );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSignFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the sign from.</param>
    /// <returns>New <see cref="SqlSignFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSignFunctionExpressionNode Sign(this SqlExpressionNode node)
    {
        return SqlNode.Functions.Sign( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlAbsFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the absolute value from.</param>
    /// <returns>New <see cref="SqlAbsFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAbsFunctionExpressionNode Abs(this SqlExpressionNode node)
    {
        return SqlNode.Functions.Abs( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlFloorFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the floor value from.</param>
    /// <returns>New <see cref="SqlFloorFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlFloorFunctionExpressionNode Floor(this SqlExpressionNode node)
    {
        return SqlNode.Functions.Floor( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCeilingFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the ceiling value from.</param>
    /// <returns>New <see cref="SqlCeilingFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCeilingFunctionExpressionNode Ceiling(this SqlExpressionNode node)
    {
        return SqlNode.Functions.Ceiling( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTruncateFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the truncated value from.</param>
    /// <param name="precision">Optional decimal precision of the truncation. Equal to null by default.</param>
    /// <returns>New <see cref="SqlTruncateFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTruncateFunctionExpressionNode Truncate(this SqlExpressionNode node, SqlExpressionNode? precision = null)
    {
        return SqlNode.Functions.Truncate( node, precision );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRoundFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the rounded value from.</param>
    /// <param name="precision">Decimal rounding precision.</param>
    /// <returns>New <see cref="SqlRoundFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRoundFunctionExpressionNode Round(this SqlExpressionNode node, SqlExpressionNode precision)
    {
        return SqlNode.Functions.Round( node, precision );
    }

    /// <summary>
    /// Creates a new <see cref="SqlPowerFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to raise to the desired power.</param>
    /// <param name="power">Expression that defines the desired power to raise to.</param>
    /// <returns>New <see cref="SqlPowerFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlPowerFunctionExpressionNode Power(this SqlExpressionNode node, SqlExpressionNode power)
    {
        return SqlNode.Functions.Power( node, power );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSquareRootFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the square root from.</param>
    /// <returns>New <see cref="SqlSquareRootFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSquareRootFunctionExpressionNode SquareRoot(this SqlExpressionNode node)
    {
        return SqlNode.Functions.SquareRoot( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCountAggregateFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the number of records for.</param>
    /// <returns>New <see cref="SqlCountAggregateFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCountAggregateFunctionExpressionNode Count(this SqlExpressionNode node)
    {
        return SqlNode.AggregateFunctions.Count( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMinAggregateFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the minimum value for.</param>
    /// <returns>New <see cref="SqlMinAggregateFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMinAggregateFunctionExpressionNode Min(this SqlExpressionNode node)
    {
        return SqlNode.AggregateFunctions.Min( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMinFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First expression to calculate the minimum value from.</param>
    /// <param name="other">Collection of following expressions to calculate the minimum value from.</param>
    /// <returns>New <see cref="SqlMinFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMinFunctionExpressionNode Min(this SqlExpressionNode node, params SqlExpressionNode[] other)
    {
        var nodes = new SqlExpressionNode[other.Length + 1];
        nodes[0] = node;
        other.CopyTo( nodes, index: 1 );
        return SqlNode.Functions.Min( nodes );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMaxAggregateFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the maximum value for.</param>
    /// <returns>New <see cref="SqlMaxAggregateFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMaxAggregateFunctionExpressionNode Max(this SqlExpressionNode node)
    {
        return SqlNode.AggregateFunctions.Max( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMaxFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">First expression to calculate the maximum value from.</param>
    /// <param name="other">Collection of following expressions to calculate the maximum value from.</param>
    /// <returns>New <see cref="SqlMaxFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMaxFunctionExpressionNode Max(this SqlExpressionNode node, params SqlExpressionNode[] other)
    {
        var nodes = new SqlExpressionNode[other.Length + 1];
        nodes[0] = node;
        other.CopyTo( nodes, index: 1 );
        return SqlNode.Functions.Max( nodes );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSumAggregateFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the sum value for.</param>
    /// <returns>New <see cref="SqlSumAggregateFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSumAggregateFunctionExpressionNode Sum(this SqlExpressionNode node)
    {
        return SqlNode.AggregateFunctions.Sum( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlAverageAggregateFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the average value for.</param>
    /// <returns>New <see cref="SqlAverageAggregateFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAverageAggregateFunctionExpressionNode Average(this SqlExpressionNode node)
    {
        return SqlNode.AggregateFunctions.Average( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlStringConcatAggregateFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the concatenated string for.</param>
    /// <param name="separator">Optional separator of concatenated strings. Equal to null by default.</param>
    /// <returns>New <see cref="SqlStringConcatAggregateFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlStringConcatAggregateFunctionExpressionNode StringConcat(
        this SqlExpressionNode node,
        SqlExpressionNode? separator = null)
    {
        return SqlNode.AggregateFunctions.StringConcat( node, separator );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNTileWindowFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Number of groups.</param>
    /// <returns>New <see cref="SqlNTileWindowFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNTileWindowFunctionExpressionNode NTile(this SqlExpressionNode node)
    {
        return SqlNode.WindowFunctions.NTile( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLagWindowFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the lag for.</param>
    /// <param name="offset">Optional offset. Equal to SQL literal that represents <b>1</b> by default.</param>
    /// <param name="default">Optional default value. Equal to null by default.</param>
    /// <returns>New <see cref="SqlLagWindowFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLagWindowFunctionExpressionNode Lag(
        this SqlExpressionNode node,
        SqlExpressionNode? offset = null,
        SqlExpressionNode? @default = null)
    {
        return SqlNode.WindowFunctions.Lag( node, offset, @default );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLeadWindowFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the lead for.</param>
    /// <param name="offset">Optional offset. Equal to SQL literal that represents <b>1</b> by default.</param>
    /// <param name="default">Optional default value. Equal to null by default.</param>
    /// <returns>New <see cref="SqlLeadWindowFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLeadWindowFunctionExpressionNode Lead(
        this SqlExpressionNode node,
        SqlExpressionNode? offset = null,
        SqlExpressionNode? @default = null)
    {
        return SqlNode.WindowFunctions.Lead( node, offset, @default );
    }

    /// <summary>
    /// Creates a new <see cref="SqlFirstValueWindowFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the first value for.</param>
    /// <returns>New <see cref="SqlFirstValueWindowFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlFirstValueWindowFunctionExpressionNode FirstValue(this SqlExpressionNode node)
    {
        return SqlNode.WindowFunctions.FirstValue( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLastValueWindowFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the last value for.</param>
    /// <returns>New <see cref="SqlLastValueWindowFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLastValueWindowFunctionExpressionNode LastValue(this SqlExpressionNode node)
    {
        return SqlNode.WindowFunctions.LastValue( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNthValueWindowFunctionExpressionNode"/> instance.
    /// </summary>
    /// <param name="node">Expression to calculate the n-th value for.</param>
    /// <param name="n">Row's position.</param>
    /// <returns>New <see cref="SqlNthValueWindowFunctionExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNthValueWindowFunctionExpressionNode NthValue(this SqlExpressionNode node, SqlExpressionNode n)
    {
        return SqlNode.WindowFunctions.NthValue( node, n );
    }

    /// <summary>
    /// Decorates the provided SQL aggregate function node with an <see cref="SqlDistinctTraitNode"/>.
    /// </summary>
    /// <param name="node">Aggregate function node to decorate.</param>
    /// <typeparam name="TAggregateFunctionNode">SQL aggregate function node type.</typeparam>
    /// <returns>Decorated SQL aggregate function node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TAggregateFunctionNode Distinct<TAggregateFunctionNode>(this TAggregateFunctionNode node)
        where TAggregateFunctionNode : SqlAggregateFunctionExpressionNode
    {
        return ( TAggregateFunctionNode )node.AddTrait( SqlNode.DistinctTrait() );
    }

    /// <summary>
    /// Decorates the provided SQL aggregate function node with an <see cref="SqlFilterTraitNode"/>
    /// with <see cref="SqlFilterTraitNode.IsConjunction"/> set to <b>true</b>.
    /// </summary>
    /// <param name="node">Aggregate function node to decorate.</param>
    /// <param name="filter">Underlying predicate.</param>
    /// <typeparam name="TAggregateFunctionNode">SQL aggregate function node type.</typeparam>
    /// <returns>Decorated SQL aggregate function node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TAggregateFunctionNode AndWhere<TAggregateFunctionNode>(this TAggregateFunctionNode node, SqlConditionNode filter)
        where TAggregateFunctionNode : SqlAggregateFunctionExpressionNode
    {
        return ( TAggregateFunctionNode )node.AddTrait( SqlNode.FilterTrait( filter, isConjunction: true ) );
    }

    /// <summary>
    /// Decorates the provided SQL aggregate function node with an <see cref="SqlFilterTraitNode"/>
    /// with <see cref="SqlFilterTraitNode.IsConjunction"/> set to <b>false</b>.
    /// </summary>
    /// <param name="node">Aggregate function node to decorate.</param>
    /// <param name="filter">Underlying predicate.</param>
    /// <typeparam name="TAggregateFunctionNode">SQL aggregate function node type.</typeparam>
    /// <returns>Decorated SQL aggregate function node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TAggregateFunctionNode OrWhere<TAggregateFunctionNode>(this TAggregateFunctionNode node, SqlConditionNode filter)
        where TAggregateFunctionNode : SqlAggregateFunctionExpressionNode
    {
        return ( TAggregateFunctionNode )node.AddTrait( SqlNode.FilterTrait( filter, isConjunction: false ) );
    }

    /// <summary>
    /// Decorates the provided SQL aggregate function node with an <see cref="SqlSortTraitNode"/>.
    /// </summary>
    /// <param name="node">Aggregate function node to decorate.</param>
    /// <param name="ordering">Collection of ordering definitions.</param>
    /// <typeparam name="TAggregateFunctionNode">SQL aggregate function node type.</typeparam>
    /// <returns>Decorated SQL aggregate function node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TAggregateFunctionNode OrderBy<TAggregateFunctionNode>(
        this TAggregateFunctionNode node,
        IEnumerable<SqlOrderByNode> ordering)
        where TAggregateFunctionNode : SqlAggregateFunctionExpressionNode
    {
        return node.OrderBy( ordering.ToArray() );
    }

    /// <summary>
    /// Decorates the provided SQL aggregate function node with an <see cref="SqlSortTraitNode"/>.
    /// </summary>
    /// <param name="node">Aggregate function node to decorate.</param>
    /// <param name="ordering">Collection of ordering definitions.</param>
    /// <typeparam name="TAggregateFunctionNode">SQL aggregate function node type.</typeparam>
    /// <returns>Decorated SQL aggregate function node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TAggregateFunctionNode OrderBy<TAggregateFunctionNode>(this TAggregateFunctionNode node, params SqlOrderByNode[] ordering)
        where TAggregateFunctionNode : SqlAggregateFunctionExpressionNode
    {
        return ordering.Length == 0 ? node : ( TAggregateFunctionNode )node.AddTrait( SqlNode.SortTrait( ordering ) );
    }

    /// <summary>
    /// Decorates the provided SQL aggregate function node with an <see cref="SqlWindowTraitNode"/>.
    /// </summary>
    /// <param name="node">Aggregate function node to decorate.</param>
    /// <param name="window">Underlying window definition.</param>
    /// <typeparam name="TAggregateFunctionNode">SQL aggregate function node type.</typeparam>
    /// <returns>Decorated SQL aggregate function node.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TAggregateFunctionNode Over<TAggregateFunctionNode>(this TAggregateFunctionNode node, SqlWindowDefinitionNode window)
        where TAggregateFunctionNode : SqlAggregateFunctionExpressionNode
    {
        return ( TAggregateFunctionNode )node.AddTrait( SqlNode.WindowTrait( window ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpdateNode"/> instance by adding more <see cref="SqlUpdateNode.Assignments"/>.
    /// </summary>
    /// <param name="node">Source update node.</param>
    /// <param name="assignments">Collection of value assignments to add.</param>
    /// <returns>New <see cref="SqlUpdateNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpdateNode AndSet(this SqlUpdateNode node, Func<SqlUpdateNode, IEnumerable<SqlValueAssignmentNode>> assignments)
    {
        return node.AndSet( assignments( node ).ToArray() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpdateNode"/> instance by adding more <see cref="SqlUpdateNode.Assignments"/>.
    /// </summary>
    /// <param name="node">Source update node.</param>
    /// <param name="assignments">Collection of value assignments to add.</param>
    /// <returns>New <see cref="SqlUpdateNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpdateNode AndSet(this SqlUpdateNode node, params SqlValueAssignmentNode[] assignments)
    {
        if ( assignments.Length == 0 )
            return node;

        var newAssignments = new SqlValueAssignmentNode[node.Assignments.Count + assignments.Length];
        node.Assignments.AsSpan().CopyTo( newAssignments );
        assignments.CopyTo( newAssignments, node.Assignments.Count );
        return new SqlUpdateNode( node.DataSource, newAssignments );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDropTableNode"/> instance.
    /// </summary>
    /// <param name="node">Source table.</param>
    /// <param name="ifExists">
    /// Specifies whether or not the removal attempt should only be made if this table exists in DB. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New <see cref="SqlDropTableNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDropTableNode ToDropTable(this SqlCreateTableNode node, bool ifExists = false)
    {
        return SqlNode.DropTable( node.Info, ifExists );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDropViewNode"/> instance.
    /// </summary>
    /// <param name="node">Source view.</param>
    /// <param name="ifExists">
    /// Specifies whether or not the removal attempt should only be made if this view exists in DB. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New <see cref="SqlDropViewNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDropViewNode ToDropView(this SqlCreateViewNode node, bool ifExists = false)
    {
        return SqlNode.DropView( node.Info, ifExists );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDropIndexNode"/> instance.
    /// </summary>
    /// <param name="node">Source index.</param>
    /// <param name="ifExists">
    /// Specifies whether or not the removal attempt should only be made if this index exists in DB. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New <see cref="SqlDropIndexNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDropIndexNode ToDropIndex(this SqlCreateIndexNode node, bool ifExists = false)
    {
        return SqlNode.DropIndex( node.Table.Info, node.Name, ifExists );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTruncateNode"/> instance.
    /// </summary>
    /// <param name="node">Table to truncate.</param>
    /// <returns>New <see cref="SqlTruncateNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTruncateNode ToTruncate(this SqlRecordSetNode node)
    {
        return SqlNode.Truncate( node );
    }

    /// <summary>
    /// Creates a new <see cref="SqlInsertIntoNode"/> instance.
    /// </summary>
    /// <param name="node"><see cref="SqlValuesNode"/> source of records to be inserted.</param>
    /// <param name="recordSet">Table to insert into.</param>
    /// <param name="dataFields">Provider of collection of record set data fields that this insertion refers to.</param>
    /// <returns>New <see cref="SqlInsertIntoNode"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="SqlInsertIntoNode"/> instance.
    /// </summary>
    /// <param name="node"><see cref="SqlValuesNode"/> source of records to be inserted.</param>
    /// <param name="recordSet">Table to insert into.</param>
    /// <param name="dataFields">Collection of record set data fields that this insertion refers to.</param>
    /// <returns>New <see cref="SqlInsertIntoNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlInsertIntoNode ToInsertInto(this SqlValuesNode node, SqlRecordSetNode recordSet, params SqlDataFieldNode[] dataFields)
    {
        return SqlNode.InsertInto( node, recordSet, dataFields );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpsertNode"/> instance.
    /// </summary>
    /// <param name="node"><see cref="SqlValuesNode"/> source of records to be inserted or updated.</param>
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
            conflictTarget?.Invoke( recordSet ).ToArray() ?? ( ReadOnlyArray<SqlDataFieldNode>? )null );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpsertNode"/> instance.
    /// </summary>
    /// <param name="node"><see cref="SqlValuesNode"/> source of records to be inserted or updated.</param>
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
        this SqlValuesNode node,
        SqlRecordSetNode recordSet,
        ReadOnlyArray<SqlDataFieldNode> insertDataFields,
        Func<SqlRecordSetNode, SqlInternalRecordSetNode, IEnumerable<SqlValueAssignmentNode>> updateAssignments,
        ReadOnlyArray<SqlDataFieldNode>? conflictTarget = null)
    {
        return SqlNode.Upsert( node, recordSet, insertDataFields, updateAssignments, conflictTarget );
    }
}
