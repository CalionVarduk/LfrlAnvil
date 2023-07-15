using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Expressions;

public static partial class SqlNode
{
    private static SqlNullNode? _null;
    private static SqlTrueNode? _true;
    private static SqlFalseNode? _false;
    private static SqlDistinctDataSourceTraitNode? _distinct;
    private static SqlDummyDataSourceNode? _dummyDataSource;

    [Pure]
    public static SqlExpressionNode Literal<T>(T? value)
        where T : notnull
    {
        return Generic<T>.IsNull( value ) ? Null() : new SqlLiteralNode<T>( value );
    }

    [Pure]
    public static SqlExpressionNode Literal<T>(T? value)
        where T : struct
    {
        return value is null ? Null() : new SqlLiteralNode<T>( value.Value );
    }

    [Pure]
    public static SqlNullNode Null()
    {
        return _null ??= new SqlNullNode();
    }

    [Pure]
    public static SqlParameterNode Parameter<T>(string name, bool isNullable = false)
    {
        return Parameter( name, SqlExpressionType.Create( typeof( T ), isNullable ) );
    }

    [Pure]
    public static SqlParameterNode Parameter(string name, SqlExpressionType? type = null)
    {
        return new SqlParameterNode( name, type );
    }

    [Pure]
    public static SqlTypeCastExpressionNode TypeCast(SqlExpressionNode expression, Type type)
    {
        return new SqlTypeCastExpressionNode( expression, type );
    }

    [Pure]
    public static SqlNegateExpressionNode Negate(SqlExpressionNode value)
    {
        return new SqlNegateExpressionNode( value );
    }

    [Pure]
    public static SqlBitwiseNotExpressionNode BitwiseNot(SqlExpressionNode value)
    {
        return new SqlBitwiseNotExpressionNode( value );
    }

    [Pure]
    public static SqlAddExpressionNode Add(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlAddExpressionNode( left, right );
    }

    [Pure]
    public static SqlConcatExpressionNode Concat(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlConcatExpressionNode( left, right );
    }

    [Pure]
    public static SqlSubtractExpressionNode Subtract(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlSubtractExpressionNode( left, right );
    }

    [Pure]
    public static SqlMultiplyExpressionNode Multiply(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlMultiplyExpressionNode( left, right );
    }

    [Pure]
    public static SqlDivideExpressionNode Divide(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlDivideExpressionNode( left, right );
    }

    [Pure]
    public static SqlModuloExpressionNode Modulo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlModuloExpressionNode( left, right );
    }

    [Pure]
    public static SqlBitwiseAndExpressionNode BitwiseAnd(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseAndExpressionNode( left, right );
    }

    [Pure]
    public static SqlBitwiseOrExpressionNode BitwiseOr(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseOrExpressionNode( left, right );
    }

    [Pure]
    public static SqlBitwiseXorExpressionNode BitwiseXor(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseXorExpressionNode( left, right );
    }

    [Pure]
    public static SqlBitwiseLeftShiftExpressionNode BitwiseLeftShift(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseLeftShiftExpressionNode( left, right );
    }

    [Pure]
    public static SqlBitwiseRightShiftExpressionNode BitwiseRightShift(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseRightShiftExpressionNode( left, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRawExpressionNode RawExpression(string sql, params SqlParameterNode[] parameters)
    {
        return RawExpression( sql, type: null, parameters );
    }

    [Pure]
    public static SqlRawExpressionNode RawExpression(string sql, SqlExpressionType? type, params SqlParameterNode[] parameters)
    {
        return new SqlRawExpressionNode( sql, type, parameters );
    }

    [Pure]
    public static SqlEqualToConditionNode EqualTo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlEqualToConditionNode( left, right );
    }

    [Pure]
    public static SqlNotEqualToConditionNode NotEqualTo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlNotEqualToConditionNode( left, right );
    }

    [Pure]
    public static SqlGreaterThanConditionNode GreaterThan(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlGreaterThanConditionNode( left, right );
    }

    [Pure]
    public static SqlLessThanConditionNode LessThan(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlLessThanConditionNode( left, right );
    }

    [Pure]
    public static SqlGreaterThanOrEqualToConditionNode GreaterThanOrEqualTo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlGreaterThanOrEqualToConditionNode( left, right );
    }

    [Pure]
    public static SqlLessThanOrEqualToConditionNode LessThanOrEqualTo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlLessThanOrEqualToConditionNode( left, right );
    }

    [Pure]
    public static SqlBetweenConditionNode Between(SqlExpressionNode left, SqlExpressionNode min, SqlExpressionNode max)
    {
        return new SqlBetweenConditionNode( left, min, max, isNegated: false );
    }

    [Pure]
    public static SqlBetweenConditionNode NotBetween(SqlExpressionNode left, SqlExpressionNode min, SqlExpressionNode max)
    {
        return new SqlBetweenConditionNode( left, min, max, isNegated: true );
    }

    [Pure]
    public static SqlLikeConditionNode Like(SqlExpressionNode value, SqlExpressionNode pattern, SqlExpressionNode? escape = null)
    {
        return new SqlLikeConditionNode( value, pattern, escape, isNegated: false );
    }

    [Pure]
    public static SqlLikeConditionNode NotLike(SqlExpressionNode value, SqlExpressionNode pattern, SqlExpressionNode? escape = null)
    {
        return new SqlLikeConditionNode( value, pattern, escape, isNegated: true );
    }

    [Pure]
    public static SqlExistsConditionNode Exists(SqlQueryExpressionNode query)
    {
        return new SqlExistsConditionNode( query, isNegated: false );
    }

    [Pure]
    public static SqlExistsConditionNode NotExists(SqlQueryExpressionNode query)
    {
        return new SqlExistsConditionNode( query, isNegated: true );
    }

    [Pure]
    public static SqlConditionNode In(SqlExpressionNode value, params SqlExpressionNode[] expressions)
    {
        return expressions.Length == 0 ? False() : new SqlInConditionNode( value, expressions, isNegated: false );
    }

    [Pure]
    public static SqlConditionNode NotIn(SqlExpressionNode value, params SqlExpressionNode[] expressions)
    {
        return expressions.Length == 0 ? True() : new SqlInConditionNode( value, expressions, isNegated: true );
    }

    [Pure]
    public static SqlInQueryConditionNode InQuery(SqlExpressionNode value, SqlQueryExpressionNode query)
    {
        return new SqlInQueryConditionNode( value, query, isNegated: false );
    }

    [Pure]
    public static SqlInQueryConditionNode NotInQuery(SqlExpressionNode value, SqlQueryExpressionNode query)
    {
        return new SqlInQueryConditionNode( value, query, isNegated: true );
    }

    [Pure]
    public static SqlTrueNode True()
    {
        return _true ??= new SqlTrueNode();
    }

    [Pure]
    public static SqlFalseNode False()
    {
        return _false ??= new SqlFalseNode();
    }

    [Pure]
    public static SqlRawConditionNode RawCondition(string sql, params SqlParameterNode[] parameters)
    {
        return new SqlRawConditionNode( sql, parameters );
    }

    [Pure]
    public static SqlConditionValueNode Value(SqlConditionNode condition)
    {
        return new SqlConditionValueNode( condition );
    }

    [Pure]
    public static SqlAndConditionNode And(SqlConditionNode left, SqlConditionNode right)
    {
        return new SqlAndConditionNode( left, right );
    }

    [Pure]
    public static SqlOrConditionNode Or(SqlConditionNode left, SqlConditionNode right)
    {
        return new SqlOrConditionNode( left, right );
    }

    [Pure]
    public static SqlTableRecordSetNode Table(ISqlTable value, string? alias = null)
    {
        return new SqlTableRecordSetNode( value, alias, isOptional: false );
    }

    [Pure]
    public static SqlRawRecordSetNode RawRecordSet(string name, string? alias = null)
    {
        return new SqlRawRecordSetNode( name, alias, isOptional: false );
    }

    [Pure]
    public static SqlRawDataFieldNode RawDataField(SqlRecordSetNode recordSet, string name, SqlExpressionType? type = null)
    {
        return new SqlRawDataFieldNode( recordSet, name, type );
    }

    [Pure]
    public static SqlRawQueryExpressionNode RawQuery(string sql, params SqlParameterNode[] parameters)
    {
        return new SqlRawQueryExpressionNode( sql, parameters );
    }

    [Pure]
    public static SqlFilterDataSourceTraitNode FilterTrait(SqlConditionNode filter, bool isConjunction)
    {
        return new SqlFilterDataSourceTraitNode( filter, isConjunction );
    }

    [Pure]
    public static SqlAggregationDataSourceTraitNode AggregationTrait(params SqlExpressionNode[] expressions)
    {
        return new SqlAggregationDataSourceTraitNode( expressions );
    }

    [Pure]
    public static SqlAggregationFilterDataSourceTraitNode AggregationFilterTrait(SqlConditionNode filter, bool isConjunction)
    {
        return new SqlAggregationFilterDataSourceTraitNode( filter, isConjunction );
    }

    [Pure]
    public static SqlDistinctDataSourceTraitNode DistinctTrait()
    {
        return _distinct ??= new SqlDistinctDataSourceTraitNode();
    }

    [Pure]
    public static SqlSortQueryTraitNode SortTrait(params SqlOrderByNode[] ordering)
    {
        return new SqlSortQueryTraitNode( ordering );
    }

    [Pure]
    public static SqlLimitQueryTraitNode LimitTrait(SqlExpressionNode value)
    {
        return new SqlLimitQueryTraitNode( value );
    }

    [Pure]
    public static SqlOffsetQueryTraitNode OffsetTrait(SqlExpressionNode value)
    {
        return new SqlOffsetQueryTraitNode( value );
    }

    [Pure]
    public static SqlCommonTableExpressionQueryTraitNode CommonTableExpressionTrait(
        params SqlCommonTableExpressionNode[] commonTableExpressions)
    {
        return new SqlCommonTableExpressionQueryTraitNode( commonTableExpressions );
    }

    [Pure]
    public static SqlSelectFieldNode Select(SqlExpressionNode expression, string alias)
    {
        return new SqlSelectFieldNode( expression, alias );
    }

    [Pure]
    public static SqlSelectFieldNode Select(SqlDataFieldNode dataField, string? alias = null)
    {
        return new SqlSelectFieldNode( dataField, alias );
    }

    [Pure]
    public static SqlSelectRecordSetNode SelectAll(SqlRecordSetNode recordSet)
    {
        return new SqlSelectRecordSetNode( recordSet );
    }

    [Pure]
    public static SqlSelectAllNode SelectAll(SqlDataSourceNode dataSource)
    {
        return new SqlSelectAllNode( dataSource );
    }

    [Pure]
    public static SqlSelectExpressionNode SelectExpression(SqlSelectNode selectNode)
    {
        return new SqlSelectExpressionNode( selectNode );
    }

    [Pure]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Query<TDataSourceNode>(
        TDataSourceNode dataSource,
        params SqlSelectNode[] selection)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlDataSourceQueryExpressionNode<TDataSourceNode>( dataSource, selection );
    }

    [Pure]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Query<TDataSourceNode>(
        TDataSourceNode dataSource,
        SqlQueryTraitNode trait)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlDataSourceQueryExpressionNode<TDataSourceNode>( dataSource, trait );
    }

    [Pure]
    public static SqlCompoundQueryExpressionNode CompoundQuery(
        SqlQueryExpressionNode firstQuery,
        params SqlCompoundQueryComponentNode[] followingQueries)
    {
        return new SqlCompoundQueryExpressionNode( firstQuery, followingQueries );
    }

    [Pure]
    public static SqlQueryRecordSetNode QueryRecordSet(SqlQueryExpressionNode query, string alias)
    {
        return new SqlQueryRecordSetNode( query, alias, isOptional: false );
    }

    [Pure]
    public static SqlOrdinalCommonTableExpressionNode OrdinalCommonTableExpression(SqlQueryExpressionNode query, string name)
    {
        return new SqlOrdinalCommonTableExpressionNode( query, name );
    }

    [Pure]
    public static SqlRecursiveCommonTableExpressionNode RecursiveCommonTableExpression(SqlCompoundQueryExpressionNode query, string name)
    {
        return new SqlRecursiveCommonTableExpressionNode( query, name );
    }

    [Pure]
    public static SqlSingleDataSourceNode<TRecordSetNode> SingleDataSource<TRecordSetNode>(TRecordSetNode from)
        where TRecordSetNode : SqlRecordSetNode
    {
        return new SqlSingleDataSourceNode<TRecordSetNode>( from );
    }

    [Pure]
    public static SqlDummyDataSourceNode DummyDataSource()
    {
        return _dummyDataSource ??= new SqlDummyDataSourceNode( Chain<SqlDataSourceTraitNode>.Empty );
    }

    [Pure]
    public static SqlMultiDataSourceNode Join(SqlRecordSetNode from, params SqlDataSourceJoinOnNode[] joins)
    {
        return new SqlMultiDataSourceNode( from, joins );
    }

    [Pure]
    public static SqlMultiDataSourceNode Join(SqlRecordSetNode from, params SqlJoinDefinition[] definitions)
    {
        return new SqlMultiDataSourceNode( from, definitions );
    }

    [Pure]
    public static SqlMultiDataSourceNode Join(SqlDataSourceNode from, params SqlDataSourceJoinOnNode[] joins)
    {
        return new SqlMultiDataSourceNode( from, joins );
    }

    [Pure]
    public static SqlMultiDataSourceNode Join(SqlDataSourceNode from, params SqlJoinDefinition[] definitions)
    {
        return new SqlMultiDataSourceNode( from, definitions );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrderByNode OrderByAsc(SqlExpressionNode expression)
    {
        return OrderBy( expression, Sql.OrderBy.Asc );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrderByNode OrderByDesc(SqlExpressionNode expression)
    {
        return OrderBy( expression, Sql.OrderBy.Desc );
    }

    [Pure]
    public static SqlOrderByNode OrderBy(SqlExpressionNode expression, OrderBy ordering)
    {
        return new SqlOrderByNode( expression, ordering );
    }

    [Pure]
    public static SqlDataSourceJoinOnNode InnerJoinOn(SqlRecordSetNode innerRecordSet, SqlConditionNode onExpression)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Inner, innerRecordSet, onExpression );
    }

    [Pure]
    public static SqlDataSourceJoinOnNode LeftJoinOn(SqlRecordSetNode innerRecordSet, SqlConditionNode onExpression)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Left, innerRecordSet, onExpression );
    }

    [Pure]
    public static SqlDataSourceJoinOnNode RightJoinOn(SqlRecordSetNode innerRecordSet, SqlConditionNode onExpression)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Right, innerRecordSet, onExpression );
    }

    [Pure]
    public static SqlDataSourceJoinOnNode FullJoinOn(SqlRecordSetNode innerRecordSet, SqlConditionNode onExpression)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Full, innerRecordSet, onExpression );
    }

    [Pure]
    public static SqlDataSourceJoinOnNode CrossJoin(SqlRecordSetNode innerRecordSet)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Cross, innerRecordSet, True() );
    }

    [Pure]
    public static SqlCompoundQueryComponentNode UnionWith(SqlQueryExpressionNode query)
    {
        return new SqlCompoundQueryComponentNode( query, SqlCompoundQueryOperator.Union );
    }

    [Pure]
    public static SqlCompoundQueryComponentNode UnionAllWith(SqlQueryExpressionNode query)
    {
        return new SqlCompoundQueryComponentNode( query, SqlCompoundQueryOperator.UnionAll );
    }

    [Pure]
    public static SqlCompoundQueryComponentNode IntersectWith(SqlQueryExpressionNode query)
    {
        return new SqlCompoundQueryComponentNode( query, SqlCompoundQueryOperator.Intersect );
    }

    [Pure]
    public static SqlCompoundQueryComponentNode ExceptWith(SqlQueryExpressionNode query)
    {
        return new SqlCompoundQueryComponentNode( query, SqlCompoundQueryOperator.Except );
    }

    [Pure]
    public static SqlCompoundQueryComponentNode CompoundWith(SqlCompoundQueryOperator @operator, SqlQueryExpressionNode query)
    {
        Ensure.IsDefined( @operator, nameof( @operator ) );
        return new SqlCompoundQueryComponentNode( query, @operator );
    }

    [Pure]
    public static SqlSwitchExpressionNode Iif(SqlConditionNode condition, SqlExpressionNode whenTrue, SqlExpressionNode whenFalse)
    {
        return Switch( new[] { SwitchCase( condition, whenTrue ) }, whenFalse );
    }

    [Pure]
    public static SqlSwitchExpressionNode Switch(IEnumerable<SqlSwitchCaseNode> cases, SqlExpressionNode defaultExpression)
    {
        return new SqlSwitchExpressionNode( cases.ToArray(), defaultExpression );
    }

    [Pure]
    public static SqlSwitchCaseNode SwitchCase(SqlConditionNode condition, SqlExpressionNode expression)
    {
        return new SqlSwitchCaseNode( condition, expression );
    }

    [Pure]
    public static SqlValuesNode Values(SqlExpressionNode[,] expressions)
    {
        return new SqlValuesNode( expressions );
    }

    [Pure]
    public static SqlValuesNode Values(params SqlExpressionNode[] expressions)
    {
        return new SqlValuesNode( expressions );
    }

    [Pure]
    public static SqlDeleteFromNode DeleteFrom(SqlDataSourceNode dataSource, SqlRecordSetNode recordSet)
    {
        return new SqlDeleteFromNode( dataSource, recordSet );
    }

    [Pure]
    public static SqlValueAssignmentNode ValueAssignment(SqlDataFieldNode dataField, SqlExpressionNode value)
    {
        return new SqlValueAssignmentNode( dataField, value );
    }

    [Pure]
    public static SqlUpdateNode Update(
        SqlDataSourceNode dataSource,
        SqlRecordSetNode recordSet,
        params SqlValueAssignmentNode[] assignments)
    {
        return new SqlUpdateNode( dataSource, recordSet, assignments );
    }

    [Pure]
    public static SqlInsertIntoNode InsertInto(
        SqlQueryExpressionNode query,
        SqlRecordSetNode recordSet,
        params SqlDataFieldNode[] dataFields)
    {
        return new SqlInsertIntoNode( query, recordSet, dataFields );
    }

    [Pure]
    public static SqlInsertIntoNode InsertInto(SqlValuesNode values, SqlRecordSetNode recordSet, params SqlDataFieldNode[] dataFields)
    {
        return new SqlInsertIntoNode( values, recordSet, dataFields );
    }
}
