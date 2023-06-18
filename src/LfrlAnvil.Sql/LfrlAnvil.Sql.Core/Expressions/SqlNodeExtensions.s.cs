using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Decorators;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Expressions;

public static class SqlNodeExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTableRecordSetNode ToRecordSet(this ISqlTable table, string? alias = null)
    {
        return SqlNode.Table( table, alias );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSelectFieldNode As(this SqlExpressionNode node, string alias)
    {
        return SqlNode.Select( node, alias );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSelectFieldNode AsSelf(this SqlDataFieldNode node)
    {
        return SqlNode.Select( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSingleDataSourceNode<TRecordSetNode> ToDataSource<TRecordSetNode>(this TRecordSetNode node)
        where TRecordSetNode : SqlRecordSetNode
    {
        return SqlNode.SingleDataSource( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlRecordSetNode node, IEnumerable<SqlDataSourceJoinOnNode> joins)
    {
        return node.Join( joins.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlRecordSetNode node, params SqlDataSourceJoinOnNode[] joins)
    {
        return SqlNode.Join( node, joins );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlRecordSetNode node, IEnumerable<SqlJoinDefinition> definitions)
    {
        return node.Join( definitions.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlRecordSetNode node, params SqlJoinDefinition[] definitions)
    {
        return SqlNode.Join( node, definitions );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlDataSourceNode node, IEnumerable<SqlDataSourceJoinOnNode> joins)
    {
        return node.Join( joins.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlDataSourceNode node, params SqlDataSourceJoinOnNode[] joins)
    {
        return SqlNode.Join( node, joins );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlDataSourceNode node, IEnumerable<SqlJoinDefinition> definitions)
    {
        return node.Join( definitions.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataSourceNode Join(this SqlDataSourceNode node, params SqlJoinDefinition[] definitions)
    {
        return SqlNode.Join( node, definitions );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceJoinOnNode InnerOn(this SqlRecordSetNode node, SqlConditionNode onExpression)
    {
        return SqlNode.InnerJoinOn( node, onExpression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceJoinOnNode LeftOn(this SqlRecordSetNode node, SqlConditionNode onExpression)
    {
        return SqlNode.LeftJoinOn( node, onExpression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceJoinOnNode RightOn(this SqlRecordSetNode node, SqlConditionNode onExpression)
    {
        return SqlNode.RightJoinOn( node, onExpression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceJoinOnNode FullOn(this SqlRecordSetNode node, SqlConditionNode onExpression)
    {
        return SqlNode.FullJoinOn( node, onExpression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceJoinOnNode Cross(this SqlRecordSetNode node)
    {
        return SqlNode.CrossJoin( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSelectRecordSetNode GetAll(this SqlRecordSetNode node)
    {
        return SqlNode.SelectAll( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRawDataFieldNode GetRawField(this SqlRecordSetNode node, string name, SqlExpressionType? type)
    {
        return SqlNode.RawDataField( node, name, type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTypeCastExpressionNode CastTo<T>(this SqlExpressionNode node)
    {
        return node.CastTo( typeof( T ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTypeCastExpressionNode CastTo(this SqlExpressionNode node, Type type)
    {
        return SqlNode.TypeCast( node, type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConditionValueNode ToValue(this SqlConditionNode node)
    {
        return SqlNode.Value( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNegateExpressionNode Negate(this SqlExpressionNode node)
    {
        return SqlNode.Negate( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBitwiseNotExpressionNode BitwiseNot(this SqlExpressionNode node)
    {
        return SqlNode.BitwiseNot( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAddExpressionNode Add(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.Add( node, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConcatExpressionNode Concat(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.Concat( node, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSubtractExpressionNode Subtract(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.Subtract( node, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiplyExpressionNode Multiply(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.Multiply( node, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDivideExpressionNode Divide(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.Divide( node, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlModuloExpressionNode Modulo(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.Modulo( node, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBitwiseAndExpressionNode BitwiseAnd(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.BitwiseAnd( node, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBitwiseOrExpressionNode BitwiseOr(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.BitwiseOr( node, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBitwiseXorExpressionNode BitwiseXor(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.BitwiseXor( node, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBitwiseLeftShiftExpressionNode BitwiseLeftShift(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.BitwiseLeftShift( node, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBitwiseRightShiftExpressionNode BitwiseRightShift(this SqlExpressionNode node, SqlExpressionNode right)
    {
        return SqlNode.BitwiseRightShift( node, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlEqualToConditionNode IsEqualTo(this SqlExpressionNode? node, SqlExpressionNode? right)
    {
        return SqlNode.EqualTo( node ?? SqlNode.Null(), right ?? SqlNode.Null() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNotEqualToConditionNode IsNotEqualTo(this SqlExpressionNode? node, SqlExpressionNode? right)
    {
        return SqlNode.NotEqualTo( node ?? SqlNode.Null(), right ?? SqlNode.Null() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlGreaterThanConditionNode IsGreaterThan(this SqlExpressionNode? node, SqlExpressionNode? right)
    {
        return SqlNode.GreaterThan( node ?? SqlNode.Null(), right ?? SqlNode.Null() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLessThanConditionNode IsLessThan(this SqlExpressionNode? node, SqlExpressionNode? right)
    {
        return SqlNode.LessThan( node ?? SqlNode.Null(), right ?? SqlNode.Null() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlGreaterThanOrEqualToConditionNode IsGreaterThanOrEqualTo(this SqlExpressionNode? node, SqlExpressionNode? right)
    {
        return SqlNode.GreaterThanOrEqualTo( node ?? SqlNode.Null(), right ?? SqlNode.Null() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLessThanOrEqualToConditionNode IsLessThanOrEqualTo(this SqlExpressionNode? node, SqlExpressionNode? right)
    {
        return SqlNode.LessThanOrEqualTo( node ?? SqlNode.Null(), right ?? SqlNode.Null() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBetweenConditionNode IsBetween(this SqlExpressionNode? node, SqlExpressionNode? min, SqlExpressionNode? max)
    {
        return SqlNode.Between( node ?? SqlNode.Null(), min ?? SqlNode.Null(), max ?? SqlNode.Null() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlBetweenConditionNode IsNotBetween(this SqlExpressionNode? node, SqlExpressionNode? min, SqlExpressionNode? max)
    {
        return SqlNode.NotBetween( node ?? SqlNode.Null(), min ?? SqlNode.Null(), max ?? SqlNode.Null() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLikeConditionNode Like(this SqlExpressionNode node, SqlExpressionNode pattern, SqlExpressionNode? escape = null)
    {
        return SqlNode.Like( node, pattern, escape );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLikeConditionNode NotLike(this SqlExpressionNode node, SqlExpressionNode pattern, SqlExpressionNode? escape = null)
    {
        return SqlNode.NotLike( node, pattern, escape );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLikeConditionNode Escape(this SqlLikeConditionNode node, SqlExpressionNode escape)
    {
        return new SqlLikeConditionNode( node.Value, node.Pattern, escape, node.IsNegated );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExistsConditionNode Exists(this SqlRecordSetNode node)
    {
        return node.ToDataSource().Exists();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExistsConditionNode NotExists(this SqlRecordSetNode node)
    {
        return node.ToDataSource().NotExists();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConditionNode In(this SqlExpressionNode node, IEnumerable<SqlExpressionNode> expressions)
    {
        return node.In( expressions.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConditionNode In(this SqlExpressionNode node, params SqlExpressionNode[] expressions)
    {
        return SqlNode.In( node, expressions );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConditionNode NotIn(this SqlExpressionNode node, IEnumerable<SqlExpressionNode> expressions)
    {
        return node.NotIn( expressions.ToArray() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConditionNode NotIn(this SqlExpressionNode node, params SqlExpressionNode[] expressions)
    {
        return SqlNode.NotIn( node, expressions );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlInQueryConditionNode InQuery(this SqlExpressionNode node, SqlQueryExpressionNode query)
    {
        return SqlNode.InQuery( node, query );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlInQueryConditionNode NotInQuery(this SqlExpressionNode node, SqlQueryExpressionNode query)
    {
        return SqlNode.NotInQuery( node, query );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAndConditionNode And(this SqlConditionNode node, SqlConditionNode right)
    {
        return SqlNode.And( node, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrConditionNode Or(this SqlConditionNode node, SqlConditionNode right)
    {
        return SqlNode.Or( node, right );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrderByNode Asc(this SqlExpressionNode node)
    {
        return SqlNode.OrderByAsc( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrderByNode Desc(this SqlExpressionNode node)
    {
        return SqlNode.OrderByDesc( node );
    }
}
