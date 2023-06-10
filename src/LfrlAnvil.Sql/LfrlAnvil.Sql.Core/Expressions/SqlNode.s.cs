using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Decorators;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Expressions;

public static class SqlNode
{
    private static SqlNullNode? _null;
    private static SqlTrueNode? _true;
    private static SqlFalseNode? _false;

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
    public static SqlRawExpressionNode RawExpression(string sql, SqlExpressionType? type, IEnumerable<SqlParameterNode> parameters)
    {
        return RawExpression( sql, type, parameters.ToArray() );
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
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConditionNode In(SqlExpressionNode value, IEnumerable<SqlExpressionNode> expressions)
    {
        return In( value, expressions.ToArray() );
    }

    [Pure]
    public static SqlConditionNode In(SqlExpressionNode value, params SqlExpressionNode[] expressions)
    {
        return expressions.Length == 0 ? False() : new SqlInConditionNode( value, expressions, isNegated: false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConditionNode NotIn(SqlExpressionNode value, IEnumerable<SqlExpressionNode> expressions)
    {
        return NotIn( value, expressions.ToArray() );
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
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRawConditionNode RawCondition(string sql, IEnumerable<SqlParameterNode> parameters)
    {
        return RawCondition( sql, parameters.ToArray() );
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
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRawQueryExpressionNode RawQuery(string sql, IEnumerable<SqlParameterNode> parameters)
    {
        return RawQuery( sql, parameters.ToArray() );
    }

    [Pure]
    public static SqlRawQueryExpressionNode RawQuery(string sql, params SqlParameterNode[] parameters)
    {
        return new SqlRawQueryExpressionNode( sql, parameters );
    }

    [Pure]
    public static SqlFilterDataSourceDecoratorNode<TDataSourceNode> Filtered<TDataSourceNode>(
        TDataSourceNode dataSource,
        SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlFilterDataSourceDecoratorNode<TDataSourceNode>( dataSource, filter );
    }

    [Pure]
    public static SqlFilterDataSourceDecoratorNode<TDataSourceNode> AndFiltered<TDataSourceNode>(
        SqlDataSourceDecoratorNode<TDataSourceNode> @base,
        SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlFilterDataSourceDecoratorNode<TDataSourceNode>( @base, filter, isConjunction: true );
    }

    [Pure]
    public static SqlFilterDataSourceDecoratorNode<TDataSourceNode> OrFiltered<TDataSourceNode>(
        SqlDataSourceDecoratorNode<TDataSourceNode> @base,
        SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlFilterDataSourceDecoratorNode<TDataSourceNode>( @base, filter, isConjunction: false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSortDataSourceDecoratorNode<TDataSourceNode> Ordered<TDataSourceNode>(
        TDataSourceNode dataSource,
        IEnumerable<SqlOrderByNode> ordering)
        where TDataSourceNode : SqlDataSourceNode
    {
        return Ordered( dataSource, ordering.ToArray() );
    }

    [Pure]
    public static SqlSortDataSourceDecoratorNode<TDataSourceNode> Ordered<TDataSourceNode>(
        TDataSourceNode dataSource,
        params SqlOrderByNode[] ordering)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlSortDataSourceDecoratorNode<TDataSourceNode>( dataSource, ordering );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSortDataSourceDecoratorNode<TDataSourceNode> Ordered<TDataSourceNode>(
        SqlDataSourceDecoratorNode<TDataSourceNode> @base,
        IEnumerable<SqlOrderByNode> ordering)
        where TDataSourceNode : SqlDataSourceNode
    {
        return Ordered( @base, ordering.ToArray() );
    }

    [Pure]
    public static SqlSortDataSourceDecoratorNode<TDataSourceNode> Ordered<TDataSourceNode>(
        SqlDataSourceDecoratorNode<TDataSourceNode> @base,
        params SqlOrderByNode[] ordering)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlSortDataSourceDecoratorNode<TDataSourceNode>( @base, ordering );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAggregationDataSourceDecoratorNode<TDataSourceNode> Aggregated<TDataSourceNode>(
        TDataSourceNode dataSource,
        IEnumerable<SqlExpressionNode> expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return Aggregated( dataSource, expressions.ToArray() );
    }

    [Pure]
    public static SqlAggregationDataSourceDecoratorNode<TDataSourceNode> Aggregated<TDataSourceNode>(
        TDataSourceNode dataSource,
        params SqlExpressionNode[] expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlAggregationDataSourceDecoratorNode<TDataSourceNode>( dataSource, expressions );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAggregationDataSourceDecoratorNode<TDataSourceNode> Aggregated<TDataSourceNode>(
        SqlDataSourceDecoratorNode<TDataSourceNode> @base,
        IEnumerable<SqlExpressionNode> expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return Aggregated( @base, expressions.ToArray() );
    }

    [Pure]
    public static SqlAggregationDataSourceDecoratorNode<TDataSourceNode> Aggregated<TDataSourceNode>(
        SqlDataSourceDecoratorNode<TDataSourceNode> @base,
        params SqlExpressionNode[] expressions)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlAggregationDataSourceDecoratorNode<TDataSourceNode>( @base, expressions );
    }

    [Pure]
    public static SqlAggregationFilterDataSourceDecoratorNode<TDataSourceNode> AggregationFiltered<TDataSourceNode>(
        TDataSourceNode dataSource,
        SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlAggregationFilterDataSourceDecoratorNode<TDataSourceNode>( dataSource, filter );
    }

    [Pure]
    public static SqlAggregationFilterDataSourceDecoratorNode<TDataSourceNode> AndAggregationFiltered<TDataSourceNode>(
        SqlDataSourceDecoratorNode<TDataSourceNode> @base,
        SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlAggregationFilterDataSourceDecoratorNode<TDataSourceNode>( @base, filter, isConjunction: true );
    }

    [Pure]
    public static SqlAggregationFilterDataSourceDecoratorNode<TDataSourceNode> OrAggregationFiltered<TDataSourceNode>(
        SqlDataSourceDecoratorNode<TDataSourceNode> @base,
        SqlConditionNode filter)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlAggregationFilterDataSourceDecoratorNode<TDataSourceNode>( @base, filter, isConjunction: false );
    }

    [Pure]
    public static SqlDistinctDataSourceDecoratorNode<TDataSourceNode> Distinct<TDataSourceNode>(TDataSourceNode dataSource)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlDistinctDataSourceDecoratorNode<TDataSourceNode>( dataSource );
    }

    [Pure]
    public static SqlDistinctDataSourceDecoratorNode<TDataSourceNode> Distinct<TDataSourceNode>(
        SqlDataSourceDecoratorNode<TDataSourceNode> @base)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlDistinctDataSourceDecoratorNode<TDataSourceNode>( @base );
    }

    [Pure]
    public static SqlLimitDataSourceDecoratorNode<TDataSourceNode> Limit<TDataSourceNode>(
        TDataSourceNode dataSource,
        SqlExpressionNode value)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlLimitDataSourceDecoratorNode<TDataSourceNode>( dataSource, value );
    }

    [Pure]
    public static SqlLimitDataSourceDecoratorNode<TDataSourceNode> Limit<TDataSourceNode>(
        SqlDataSourceDecoratorNode<TDataSourceNode> @base,
        SqlExpressionNode value)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlLimitDataSourceDecoratorNode<TDataSourceNode>( @base, value );
    }

    [Pure]
    public static SqlOffsetDataSourceDecoratorNode<TDataSourceNode> Offset<TDataSourceNode>(
        TDataSourceNode dataSource,
        SqlExpressionNode value)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlOffsetDataSourceDecoratorNode<TDataSourceNode>( dataSource, value );
    }

    [Pure]
    public static SqlOffsetDataSourceDecoratorNode<TDataSourceNode> Offset<TDataSourceNode>(
        SqlDataSourceDecoratorNode<TDataSourceNode> @base,
        SqlExpressionNode value)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlOffsetDataSourceDecoratorNode<TDataSourceNode>( @base, value );
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
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryExpressionNode Query(SqlDataSourceNode dataSource, IEnumerable<SqlSelectNode> selection)
    {
        return Query( dataSource, selection.ToArray() );
    }

    [Pure]
    public static SqlQueryExpressionNode Query(SqlDataSourceNode dataSource, params SqlSelectNode[] selection)
    {
        return new SqlQueryExpressionNode( dataSource, selection );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryExpressionNode Query(SqlDataSourceDecoratorNode decorator, IEnumerable<SqlSelectNode> selection)
    {
        return Query( decorator, selection.ToArray() );
    }

    [Pure]
    public static SqlQueryExpressionNode Query(SqlDataSourceDecoratorNode decorator, params SqlSelectNode[] selection)
    {
        return new SqlQueryExpressionNode( decorator, selection );
    }

    [Pure]
    public static SqlQueryRecordSetNode QueryRecordSet(SqlQueryExpressionNode query, string alias)
    {
        return new SqlQueryRecordSetNode( query, alias, isOptional: false );
    }

    [Pure]
    public static SqlRawQueryRecordSetNode RawQueryRecordSet(SqlRawQueryExpressionNode query, string alias)
    {
        return new SqlRawQueryRecordSetNode( query, alias, isOptional: false );
    }

    [Pure]
    public static SqlSingleDataSourceNode<TRecordSetNode> SingleDataSource<TRecordSetNode>(TRecordSetNode from)
        where TRecordSetNode : SqlRecordSetNode
    {
        return new SqlSingleDataSourceNode<TRecordSetNode>( from );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(SqlRecordSetNode from, IEnumerable<SqlDataSourceJoinOnNode> joins)
    {
        return Join( from, joins.ToArray() );
    }

    [Pure]
    public static SqlMultiDataSourceNode Join(SqlRecordSetNode from, params SqlDataSourceJoinOnNode[] joins)
    {
        return new SqlMultiDataSourceNode( from, joins );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(SqlRecordSetNode from, IEnumerable<SqlJoinDefinition> definitions)
    {
        return Join( from, definitions.ToArray() );
    }

    [Pure]
    public static SqlMultiDataSourceNode Join(SqlRecordSetNode from, params SqlJoinDefinition[] definitions)
    {
        return new SqlMultiDataSourceNode( from, definitions );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(SqlDataSourceNode from, IEnumerable<SqlDataSourceJoinOnNode> joins)
    {
        return Join( from, joins.ToArray() );
    }

    [Pure]
    public static SqlMultiDataSourceNode Join(SqlDataSourceNode from, params SqlDataSourceJoinOnNode[] joins)
    {
        return new SqlMultiDataSourceNode( from, joins );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(SqlDataSourceNode from, IEnumerable<SqlJoinDefinition> definitions)
    {
        return Join( from, definitions.ToArray() );
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
}
