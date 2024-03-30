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

public static class SqlNodeExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTableNode ToRecordSet(this ISqlTable table, string? alias = null)
    {
        return alias is null ? table.Node : SqlNode.Table( table, alias );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTableBuilderNode ToRecordSet(this ISqlTableBuilder table, string? alias = null)
    {
        return alias is null ? table.Node : SqlNode.Table( table, alias );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlViewNode ToRecordSet(this ISqlView view, string? alias = null)
    {
        return alias is null ? view.Node : SqlNode.View( view, alias );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlViewBuilderNode ToRecordSet(this ISqlViewBuilder view, string? alias = null)
    {
        return alias is null ? view.Node : SqlNode.View( view, alias );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNewTableNode AsSet(this SqlCreateTableNode node, string? alias = null)
    {
        return new SqlNewTableNode( node, alias, isOptional: false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNewViewNode AsSet(this SqlCreateViewNode node, string? alias = null)
    {
        return new SqlNewViewNode( node, alias, isOptional: false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNamedFunctionRecordSetNode AsSet(this SqlNamedFunctionExpressionNode node, string alias)
    {
        return SqlNode.NamedFunctionRecordSet( node, alias );
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
    public static SqlSelectFieldNode As(this SqlConditionNode node, string alias)
    {
        return node.ToValue().As( alias );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDataSourceQueryExpressionNode<SqlDummyDataSourceNode> ToQuery(this SqlSelectFieldNode node)
    {
        return SqlNode.DummyDataSource().Select( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlValueAssignmentNode Assign(this SqlDataFieldNode node, SqlExpressionNode value)
    {
        return SqlNode.ValueAssignment( node, value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSelectExpressionNode ToExpression(this SqlSelectNode node)
    {
        return SqlNode.SelectExpression( node );
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
    public static SqlRawDataFieldNode GetRawField(this SqlRecordSetNode node, string name, TypeNullability? type)
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
    public static SqlTypeCastExpressionNode CastTo(this SqlExpressionNode node, ISqlColumnTypeDefinition typeDefinition)
    {
        return SqlNode.TypeCast( node, typeDefinition );
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
    public static SqlSwitchCaseNode Then(this SqlConditionNode node, SqlExpressionNode value)
    {
        return SqlNode.SwitchCase( node, value );
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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrderByNode Asc(this SqlSelectNode node)
    {
        return SqlNode.OrderByAsc( node.ToExpression() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrderByNode Desc(this SqlSelectNode node)
    {
        return SqlNode.OrderByDesc( node.ToExpression() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCoalesceFunctionExpressionNode Coalesce(this SqlExpressionNode node, params SqlExpressionNode[] other)
    {
        var nodes = new SqlExpressionNode[other.Length + 1];
        nodes[0] = node;
        other.CopyTo( nodes, index: 1 );
        return SqlNode.Functions.Coalesce( nodes );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExtractDateFunctionExpressionNode ExtractDate(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ExtractDate( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExtractTimeOfDayFunctionExpressionNode ExtractTimeOfDay(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ExtractTimeOfDay( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExtractDayFunctionExpressionNode ExtractDayOfYear(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ExtractDayOfYear( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExtractDayFunctionExpressionNode ExtractDayOfMonth(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ExtractDayOfMonth( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExtractDayFunctionExpressionNode ExtractDayOfWeek(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ExtractDayOfWeek( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlExtractTemporalUnitFunctionExpressionNode ExtractTemporalUnit(this SqlExpressionNode node, SqlTemporalUnit unit)
    {
        return SqlNode.Functions.ExtractTemporalUnit( node, unit );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTemporalAddFunctionExpressionNode TemporalAdd(
        this SqlExpressionNode node,
        SqlExpressionNode value,
        SqlTemporalUnit unit)
    {
        return SqlNode.Functions.TemporalAdd( node, value, unit );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTemporalDiffFunctionExpressionNode TemporalDiff(
        this SqlExpressionNode node,
        SqlExpressionNode end,
        SqlTemporalUnit unit)
    {
        return SqlNode.Functions.TemporalDiff( node, end, unit );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLengthFunctionExpressionNode Length(this SqlExpressionNode node)
    {
        return SqlNode.Functions.Length( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlByteLengthFunctionExpressionNode ByteLength(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ByteLength( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlToLowerFunctionExpressionNode ToLower(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ToLower( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlToUpperFunctionExpressionNode ToUpper(this SqlExpressionNode node)
    {
        return SqlNode.Functions.ToUpper( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTrimStartFunctionExpressionNode TrimStart(this SqlExpressionNode node, SqlExpressionNode? characters = null)
    {
        return SqlNode.Functions.TrimStart( node, characters );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTrimEndFunctionExpressionNode TrimEnd(this SqlExpressionNode node, SqlExpressionNode? characters = null)
    {
        return SqlNode.Functions.TrimEnd( node, characters );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTrimFunctionExpressionNode Trim(this SqlExpressionNode node, SqlExpressionNode? characters = null)
    {
        return SqlNode.Functions.Trim( node, characters );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSubstringFunctionExpressionNode Substring(
        this SqlExpressionNode node,
        SqlExpressionNode startIndex,
        SqlExpressionNode? length = null)
    {
        return SqlNode.Functions.Substring( node, startIndex, length );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlReplaceFunctionExpressionNode Replace(
        this SqlExpressionNode node,
        SqlExpressionNode oldValue,
        SqlExpressionNode newValue)
    {
        return SqlNode.Functions.Replace( node, oldValue, newValue );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlReverseFunctionExpressionNode Reverse(this SqlExpressionNode node)
    {
        return SqlNode.Functions.Reverse( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexOfFunctionExpressionNode IndexOf(this SqlExpressionNode node, SqlExpressionNode value)
    {
        return SqlNode.Functions.IndexOf( node, value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLastIndexOfFunctionExpressionNode LastIndexOf(this SqlExpressionNode node, SqlExpressionNode value)
    {
        return SqlNode.Functions.LastIndexOf( node, value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSignFunctionExpressionNode Sign(this SqlExpressionNode node)
    {
        return SqlNode.Functions.Sign( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAbsFunctionExpressionNode Abs(this SqlExpressionNode node)
    {
        return SqlNode.Functions.Abs( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlFloorFunctionExpressionNode Floor(this SqlExpressionNode node)
    {
        return SqlNode.Functions.Floor( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCeilingFunctionExpressionNode Ceiling(this SqlExpressionNode node)
    {
        return SqlNode.Functions.Ceiling( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTruncateFunctionExpressionNode Truncate(this SqlExpressionNode node, SqlExpressionNode? precision = null)
    {
        return SqlNode.Functions.Truncate( node, precision );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRoundFunctionExpressionNode Round(this SqlExpressionNode node, SqlExpressionNode precision)
    {
        return SqlNode.Functions.Round( node, precision );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlPowerFunctionExpressionNode Power(this SqlExpressionNode node, SqlExpressionNode power)
    {
        return SqlNode.Functions.Power( node, power );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSquareRootFunctionExpressionNode SquareRoot(this SqlExpressionNode node)
    {
        return SqlNode.Functions.SquareRoot( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlCountAggregateFunctionExpressionNode Count(this SqlExpressionNode node)
    {
        return SqlNode.AggregateFunctions.Count( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMinAggregateFunctionExpressionNode Min(this SqlExpressionNode node)
    {
        return SqlNode.AggregateFunctions.Min( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMinFunctionExpressionNode Min(this SqlExpressionNode node, params SqlExpressionNode[] other)
    {
        var nodes = new SqlExpressionNode[other.Length + 1];
        nodes[0] = node;
        other.CopyTo( nodes, index: 1 );
        return SqlNode.Functions.Min( nodes );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMaxAggregateFunctionExpressionNode Max(this SqlExpressionNode node)
    {
        return SqlNode.AggregateFunctions.Max( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMaxFunctionExpressionNode Max(this SqlExpressionNode node, params SqlExpressionNode[] other)
    {
        var nodes = new SqlExpressionNode[other.Length + 1];
        nodes[0] = node;
        other.CopyTo( nodes, index: 1 );
        return SqlNode.Functions.Max( nodes );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlSumAggregateFunctionExpressionNode Sum(this SqlExpressionNode node)
    {
        return SqlNode.AggregateFunctions.Sum( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAverageAggregateFunctionExpressionNode Average(this SqlExpressionNode node)
    {
        return SqlNode.AggregateFunctions.Average( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlStringConcatAggregateFunctionExpressionNode StringConcat(
        this SqlExpressionNode node,
        SqlExpressionNode? separator = null)
    {
        return SqlNode.AggregateFunctions.StringConcat( node, separator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNTileWindowFunctionExpressionNode NTile(this SqlExpressionNode node)
    {
        return SqlNode.WindowFunctions.NTile( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLagWindowFunctionExpressionNode Lag(
        this SqlExpressionNode node,
        SqlExpressionNode? offset = null,
        SqlExpressionNode? @default = null)
    {
        return SqlNode.WindowFunctions.Lag( node, offset, @default );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLeadWindowFunctionExpressionNode Lead(
        this SqlExpressionNode node,
        SqlExpressionNode? offset = null,
        SqlExpressionNode? @default = null)
    {
        return SqlNode.WindowFunctions.Lead( node, offset, @default );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlFirstValueWindowFunctionExpressionNode FirstValue(this SqlExpressionNode node)
    {
        return SqlNode.WindowFunctions.FirstValue( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlLastValueWindowFunctionExpressionNode LastValue(this SqlExpressionNode node)
    {
        return SqlNode.WindowFunctions.LastValue( node );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNthValueWindowFunctionExpressionNode NthValue(this SqlExpressionNode node, SqlExpressionNode n)
    {
        return SqlNode.WindowFunctions.NthValue( node, n );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TAggregateFunctionNode Distinct<TAggregateFunctionNode>(this TAggregateFunctionNode node)
        where TAggregateFunctionNode : SqlAggregateFunctionExpressionNode
    {
        return (TAggregateFunctionNode)node.AddTrait( SqlNode.DistinctTrait() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TAggregateFunctionNode AndWhere<TAggregateFunctionNode>(this TAggregateFunctionNode node, SqlConditionNode filter)
        where TAggregateFunctionNode : SqlAggregateFunctionExpressionNode
    {
        return (TAggregateFunctionNode)node.AddTrait( SqlNode.FilterTrait( filter, isConjunction: true ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TAggregateFunctionNode OrWhere<TAggregateFunctionNode>(this TAggregateFunctionNode node, SqlConditionNode filter)
        where TAggregateFunctionNode : SqlAggregateFunctionExpressionNode
    {
        return (TAggregateFunctionNode)node.AddTrait( SqlNode.FilterTrait( filter, isConjunction: false ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TAggregateFunctionNode Over<TAggregateFunctionNode>(this TAggregateFunctionNode node, SqlWindowDefinitionNode window)
        where TAggregateFunctionNode : SqlAggregateFunctionExpressionNode
    {
        return (TAggregateFunctionNode)node.AddTrait( SqlNode.WindowTrait( window ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlUpdateNode AndSet(this SqlUpdateNode node, Func<SqlUpdateNode, IEnumerable<SqlValueAssignmentNode>> assignments)
    {
        return node.AndSet( assignments( node ).ToArray() );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDropTableNode ToDropTable(this SqlCreateTableNode node, bool ifExists = false)
    {
        return SqlNode.DropTable( node.Info, ifExists );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDropViewNode ToDropView(this SqlCreateViewNode node, bool ifExists = false)
    {
        return SqlNode.DropView( node.Info, ifExists );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDropIndexNode ToDropIndex(this SqlCreateIndexNode node, bool ifExists = false)
    {
        return SqlNode.DropIndex( node.Table.Info, node.Name, ifExists );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlTruncateNode ToTruncate(this SqlRecordSetNode node)
    {
        return SqlNode.Truncate( node );
    }
}
