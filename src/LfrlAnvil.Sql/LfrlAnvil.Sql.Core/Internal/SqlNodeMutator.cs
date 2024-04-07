using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

internal sealed class SqlNodeMutator : ISqlNodeVisitor
{
    private readonly SqlNodeAncestors _ancestors = new SqlNodeAncestors( new List<SqlNodeBase>() );
    private SqlNodeBase? _result;

    internal SqlNodeMutator(SqlNodeMutatorContext context)
    {
        Context = context;
    }

    internal SqlNodeMutatorContext Context { get; }

    public void VisitRawExpression(SqlRawExpressionNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitRawDataField(SqlRawDataFieldNode node)
    {
        VisitDataField( node );
    }

    public void VisitNull(SqlNullNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitLiteral(SqlLiteralNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitParameter(SqlParameterNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitColumn(SqlColumnNode node)
    {
        VisitDataField( node );
    }

    public void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        VisitDataField( node );
    }

    public void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        VisitDataField( node );
    }

    public void VisitViewDataField(SqlViewDataFieldNode node)
    {
        VisitDataField( node );
    }

    public void VisitNegate(SqlNegateExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var value = Handle( node.Value, node );
        if ( ! ReferenceEquals( value, node.Value ) )
            next = -value;

        Push( next );
    }

    public void VisitAdd(SqlAddExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left + right;

        Push( next );
    }

    public void VisitConcat(SqlConcatExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left.Concat( right );

        Push( next );
    }

    public void VisitSubtract(SqlSubtractExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left - right;

        Push( next );
    }

    public void VisitMultiply(SqlMultiplyExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left * right;

        Push( next );
    }

    public void VisitDivide(SqlDivideExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left / right;

        Push( next );
    }

    public void VisitModulo(SqlModuloExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left % right;

        Push( next );
    }

    public void VisitBitwiseNot(SqlBitwiseNotExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var value = Handle( node.Value, node );
        if ( ! ReferenceEquals( value, node.Value ) )
            next = ~value;

        Push( next );
    }

    public void VisitBitwiseAnd(SqlBitwiseAndExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left & right;

        Push( next );
    }

    public void VisitBitwiseOr(SqlBitwiseOrExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left | right;

        Push( next );
    }

    public void VisitBitwiseXor(SqlBitwiseXorExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left ^ right;

        Push( next );
    }

    public void VisitBitwiseLeftShift(SqlBitwiseLeftShiftExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left.BitwiseLeftShift( right );

        Push( next );
    }

    public void VisitBitwiseRightShift(SqlBitwiseRightShiftExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left.BitwiseRightShift( right );

        Push( next );
    }

    public void VisitSwitchCase(SqlSwitchCaseNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var condition = Handle( node.Condition, node );
        var expression = Handle( node.Expression, node );
        if ( ! ReferenceEquals( condition, node.Condition ) || ! ReferenceEquals( expression, node.Expression ) )
            next = SqlNode.SwitchCase( condition, expression );

        Push( next );
    }

    public void VisitSwitch(SqlSwitchExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var cases = HandleCollection( node.Cases, node );
        var @default = Handle( node.Default, node );
        if ( cases is not null || ! ReferenceEquals( @default, node.Default ) )
            next = SqlNode.Switch( cases ?? node.Cases.GetUnderlyingArray(), @default );

        Push( next );
    }

    public void VisitNamedFunction(SqlNamedFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Named( node.Name, args );

        Push( next );
    }

    public void VisitCoalesceFunction(SqlCoalesceFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Coalesce( args );

        Push( next );
    }

    public void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitCurrentUtcDateTimeFunction(SqlCurrentUtcDateTimeFunctionExpressionNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitExtractDateFunction(SqlExtractDateFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.ExtractDate( args[0] );

        Push( next );
    }

    public void VisitExtractTimeOfDayFunction(SqlExtractTimeOfDayFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.ExtractTimeOfDay( args[0] );

        Push( next );
    }

    public void VisitExtractDayFunction(SqlExtractDayFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = new SqlExtractDayFunctionExpressionNode( args[0], node.Unit );

        Push( next );
    }

    public void VisitExtractTemporalUnitFunction(SqlExtractTemporalUnitFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.ExtractTemporalUnit( args[0], node.Unit );

        Push( next );
    }

    public void VisitTemporalAddFunction(SqlTemporalAddFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.TemporalAdd( args[0], args[1], node.Unit );

        Push( next );
    }

    public void VisitTemporalDiffFunction(SqlTemporalDiffFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.TemporalDiff( args[0], args[1], node.Unit );

        Push( next );
    }

    public void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitLengthFunction(SqlLengthFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Length( args[0] );

        Push( next );
    }

    public void VisitByteLengthFunction(SqlByteLengthFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.ByteLength( args[0] );

        Push( next );
    }

    public void VisitToLowerFunction(SqlToLowerFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.ToLower( args[0] );

        Push( next );
    }

    public void VisitToUpperFunction(SqlToUpperFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.ToUpper( args[0] );

        Push( next );
    }

    public void VisitTrimStartFunction(SqlTrimStartFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.TrimStart( args[0], args.Length > 1 ? args[1] : null );

        Push( next );
    }

    public void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.TrimEnd( args[0], args.Length > 1 ? args[1] : null );

        Push( next );
    }

    public void VisitTrimFunction(SqlTrimFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Trim( args[0], args.Length > 1 ? args[1] : null );

        Push( next );
    }

    public void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Substring( args[0], args[1], args.Length > 2 ? args[2] : null );

        Push( next );
    }

    public void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Replace( args[0], args[1], args[2] );

        Push( next );
    }

    public void VisitReverseFunction(SqlReverseFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Reverse( args[0] );

        Push( next );
    }

    public void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.IndexOf( args[0], args[1] );

        Push( next );
    }

    public void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.LastIndexOf( args[0], args[1] );

        Push( next );
    }

    public void VisitSignFunction(SqlSignFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Sign( args[0] );

        Push( next );
    }

    public void VisitAbsFunction(SqlAbsFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Abs( args[0] );

        Push( next );
    }

    public void VisitCeilingFunction(SqlCeilingFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Ceiling( args[0] );

        Push( next );
    }

    public void VisitFloorFunction(SqlFloorFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Floor( args[0] );

        Push( next );
    }

    public void VisitTruncateFunction(SqlTruncateFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Truncate( args[0], args.Length > 1 ? args[1] : null );

        Push( next );
    }

    public void VisitRoundFunction(SqlRoundFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Round( args[0], args[1] );

        Push( next );
    }

    public void VisitPowerFunction(SqlPowerFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Power( args[0], args[1] );

        Push( next );
    }

    public void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.SquareRoot( args[0] );

        Push( next );
    }

    public void VisitMinFunction(SqlMinFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Min( args );

        Push( next );
    }

    public void VisitMaxFunction(SqlMaxFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        if ( args is not null )
            next = SqlNode.Functions.Max( args );

        Push( next );
    }

    public void VisitCustomFunction(SqlFunctionExpressionNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitNamedAggregateFunction(SqlNamedAggregateFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        var traits = HandleTraits( node.Traits, node );
        if ( args is not null || traits is not null )
            next = SqlNode.AggregateFunctions.Named( node.Name, args ?? node.Arguments.GetUnderlyingArray().ToArray() )
                .SetTraits( traits ?? node.Traits );

        Push( next );
    }

    public void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        var traits = HandleTraits( node.Traits, node );
        if ( args is not null || traits is not null )
            next = SqlNode.AggregateFunctions.Min( args is null ? node.Arguments[0] : args[0] ).SetTraits( traits ?? node.Traits );

        Push( next );
    }

    public void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        var traits = HandleTraits( node.Traits, node );
        if ( args is not null || traits is not null )
            next = SqlNode.AggregateFunctions.Max( args is null ? node.Arguments[0] : args[0] ).SetTraits( traits ?? node.Traits );

        Push( next );
    }

    public void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        var traits = HandleTraits( node.Traits, node );
        if ( args is not null || traits is not null )
            next = SqlNode.AggregateFunctions.Average( args is null ? node.Arguments[0] : args[0] ).SetTraits( traits ?? node.Traits );

        Push( next );
    }

    public void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        var traits = HandleTraits( node.Traits, node );
        if ( args is not null || traits is not null )
            next = SqlNode.AggregateFunctions.Sum( args is null ? node.Arguments[0] : args[0] ).SetTraits( traits ?? node.Traits );

        Push( next );
    }

    public void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        var traits = HandleTraits( node.Traits, node );
        if ( args is not null || traits is not null )
            next = SqlNode.AggregateFunctions.Count( args is null ? node.Arguments[0] : args[0] ).SetTraits( traits ?? node.Traits );

        Push( next );
    }

    public void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        var traits = HandleTraits( node.Traits, node );
        if ( args is not null || traits is not null )
        {
            var finalArgs = args ?? node.Arguments;
            next = SqlNode.AggregateFunctions.StringConcat( finalArgs[0], finalArgs.Count > 1 ? finalArgs[1] : null )
                .SetTraits( traits ?? node.Traits );
        }

        Push( next );
    }

    public void VisitRowNumberWindowFunction(SqlRowNumberWindowFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var traits = HandleTraits( node.Traits, node );
        if ( traits is not null )
            next = SqlNode.WindowFunctions.RowNumber().SetTraits( traits.Value );

        Push( next );
    }

    public void VisitRankWindowFunction(SqlRankWindowFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var traits = HandleTraits( node.Traits, node );
        if ( traits is not null )
            next = SqlNode.WindowFunctions.Rank().SetTraits( traits.Value );

        Push( next );
    }

    public void VisitDenseRankWindowFunction(SqlDenseRankWindowFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var traits = HandleTraits( node.Traits, node );
        if ( traits is not null )
            next = SqlNode.WindowFunctions.DenseRank().SetTraits( traits.Value );

        Push( next );
    }

    public void VisitCumulativeDistributionWindowFunction(SqlCumulativeDistributionWindowFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var traits = HandleTraits( node.Traits, node );
        if ( traits is not null )
            next = SqlNode.WindowFunctions.CumulativeDistribution().SetTraits( traits.Value );

        Push( next );
    }

    public void VisitNTileWindowFunction(SqlNTileWindowFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        var traits = HandleTraits( node.Traits, node );
        if ( args is not null || traits is not null )
            next = SqlNode.WindowFunctions.NTile( args is null ? node.Arguments[0] : args[0] ).SetTraits( traits ?? node.Traits );

        Push( next );
    }

    public void VisitLagWindowFunction(SqlLagWindowFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        var traits = HandleTraits( node.Traits, node );
        if ( args is not null || traits is not null )
        {
            var finalArgs = args ?? node.Arguments;
            next = SqlNode.WindowFunctions.Lag( finalArgs[0], finalArgs[1], finalArgs.Count > 2 ? finalArgs[2] : null )
                .SetTraits( traits ?? node.Traits );
        }

        Push( next );
    }

    public void VisitLeadWindowFunction(SqlLeadWindowFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        var traits = HandleTraits( node.Traits, node );
        if ( args is not null || traits is not null )
        {
            var finalArgs = args ?? node.Arguments;
            next = SqlNode.WindowFunctions.Lead( finalArgs[0], finalArgs[1], finalArgs.Count > 2 ? finalArgs[2] : null )
                .SetTraits( traits ?? node.Traits );
        }

        Push( next );
    }

    public void VisitFirstValueWindowFunction(SqlFirstValueWindowFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        var traits = HandleTraits( node.Traits, node );
        if ( args is not null || traits is not null )
            next = SqlNode.WindowFunctions.FirstValue( args is null ? node.Arguments[0] : args[0] ).SetTraits( traits ?? node.Traits );

        Push( next );
    }

    public void VisitLastValueWindowFunction(SqlLastValueWindowFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        var traits = HandleTraits( node.Traits, node );
        if ( args is not null || traits is not null )
            next = SqlNode.WindowFunctions.LastValue( args is null ? node.Arguments[0] : args[0] ).SetTraits( traits ?? node.Traits );

        Push( next );
    }

    public void VisitNthValueWindowFunction(SqlNthValueWindowFunctionExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var args = HandleCollection( node.Arguments, node );
        var traits = HandleTraits( node.Traits, node );
        if ( args is not null || traits is not null )
        {
            var finalArgs = args ?? node.Arguments;
            next = SqlNode.WindowFunctions.NthValue( finalArgs[0], finalArgs[1] ).SetTraits( traits ?? node.Traits );
        }

        Push( next );
    }

    public void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitRawCondition(SqlRawConditionNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitTrue(SqlTrueNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitFalse(SqlFalseNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitEqualTo(SqlEqualToConditionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left == right;

        Push( next );
    }

    public void VisitNotEqualTo(SqlNotEqualToConditionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left != right;

        Push( next );
    }

    public void VisitGreaterThan(SqlGreaterThanConditionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left > right;

        Push( next );
    }

    public void VisitLessThan(SqlLessThanConditionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left < right;

        Push( next );
    }

    public void VisitGreaterThanOrEqualTo(SqlGreaterThanOrEqualToConditionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left >= right;

        Push( next );
    }

    public void VisitLessThanOrEqualTo(SqlLessThanOrEqualToConditionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left <= right;

        Push( next );
    }

    public void VisitAnd(SqlAndConditionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left.And( right );

        Push( next );
    }

    public void VisitOr(SqlOrConditionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var left = Handle( node.Left, node );
        var right = Handle( node.Right, node );
        if ( ! ReferenceEquals( left, node.Left ) || ! ReferenceEquals( right, node.Right ) )
            next = left.Or( right );

        Push( next );
    }

    public void VisitConditionValue(SqlConditionValueNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var condition = Handle( node.Condition, node );
        if ( ! ReferenceEquals( condition, node.Condition ) )
            next = condition.ToValue();

        Push( next );
    }

    public void VisitBetween(SqlBetweenConditionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var value = Handle( node.Value, node );
        var min = Handle( node.Min, node );
        var max = Handle( node.Max, node );

        if ( ! ReferenceEquals( value, node.Value ) || ! ReferenceEquals( min, node.Min ) || ! ReferenceEquals( max, node.Max ) )
            next = node.IsNegated ? value.IsNotBetween( min, max ) : value.IsBetween( min, max );

        Push( next );
    }

    public void VisitExists(SqlExistsConditionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var query = Handle( node.Query, node );
        if ( ! ReferenceEquals( query, node.Query ) )
            next = node.IsNegated ? query.NotExists() : query.Exists();

        Push( next );
    }

    public void VisitLike(SqlLikeConditionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var value = Handle( node.Value, node );
        var pattern = Handle( node.Pattern, node );
        var escape = node.Escape is null ? null : Handle( node.Escape, node );

        if ( ! ReferenceEquals( value, node.Value )
            || ! ReferenceEquals( pattern, node.Pattern )
            || ! ReferenceEquals( escape, node.Escape ) )
            next = node.IsNegated ? value.NotLike( pattern, escape ) : value.Like( pattern, escape );

        Push( next );
    }

    public void VisitIn(SqlInConditionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var value = Handle( node.Value, node );
        var expressions = HandleCollection( node.Expressions, node );

        if ( ! ReferenceEquals( value, node.Value ) || expressions is not null )
        {
            var finalExpressions = expressions ?? node.Expressions.GetUnderlyingArray().ToArray();
            next = node.IsNegated ? value.NotIn( finalExpressions ) : value.In( finalExpressions );
        }

        Push( next );
    }

    public void VisitInQuery(SqlInQueryConditionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var value = Handle( node.Value, node );
        var query = Handle( node.Query, node );
        if ( ! ReferenceEquals( value, node.Value ) || ! ReferenceEquals( query, node.Query ) )
            next = node.IsNegated ? value.NotInQuery( query ) : value.InQuery( query );

        Push( next );
    }

    public void VisitRawRecordSet(SqlRawRecordSetNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitNamedFunctionRecordSet(SqlNamedFunctionRecordSetNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitTable(SqlTableNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitTableBuilder(SqlTableBuilderNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitView(SqlViewNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitViewBuilder(SqlViewBuilderNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitQueryRecordSet(SqlQueryRecordSetNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitCommonTableExpressionRecordSet(SqlCommonTableExpressionRecordSetNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitNewTable(SqlNewTableNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitNewView(SqlNewViewNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitJoinOn(SqlDataSourceJoinOnNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var innerRecordSet = Handle( node.InnerRecordSet, node );
        var onExpression = node.JoinType != SqlJoinType.Cross ? Handle( node.OnExpression, node ) : node.OnExpression;
        if ( ! ReferenceEquals( innerRecordSet, node.InnerRecordSet ) || ! ReferenceEquals( onExpression, node.OnExpression ) )
            next = new SqlDataSourceJoinOnNode( node.JoinType, innerRecordSet, onExpression );

        Push( next );
    }

    public void VisitDataSource(SqlDataSourceNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var traits = HandleTraits( node.Traits, node );
        if ( node is SqlDummyDataSourceNode )
        {
            if ( traits is not null )
                next = SqlNode.DummyDataSource().SetTraits( traits.Value );
        }
        else
        {
            var from = Handle( node.From, node );
            var joins = HandleCollection( node.Joins, node );

            if ( ! ReferenceEquals( from, node.From ) || joins is not null || traits is not null )
                next = node.Joins.Count == 0
                    ? from.ToDataSource().SetTraits( traits ?? node.Traits )
                    : from.Join( joins ?? node.Joins.GetUnderlyingArray() ).SetTraits( traits ?? node.Traits );
        }

        Push( next );
    }

    public void VisitSelectField(SqlSelectFieldNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var expression = Handle( node.Expression, node );
        if ( ! ReferenceEquals( expression, node.Expression ) )
            next = node.Alias is null
                ? CastOrThrow<SqlDataFieldNode>( expression, node.Expression, node ).AsSelf()
                : expression.As( node.Alias );

        Push( next );
    }

    public void VisitSelectCompoundField(SqlSelectCompoundFieldNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitSelectRecordSet(SqlSelectRecordSetNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var recordSet = Handle( node.RecordSet, node );
        if ( ! ReferenceEquals( recordSet, node.RecordSet ) )
            next = recordSet.GetAll();

        Push( next );
    }

    public void VisitSelectAll(SqlSelectAllNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var dataSource = Handle( node.DataSource, node );
        if ( ! ReferenceEquals( dataSource, node.DataSource ) )
            next = dataSource.GetAll();

        Push( next );
    }

    public void VisitSelectExpression(SqlSelectExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var selection = Handle( node.Selection, node );
        if ( ! ReferenceEquals( selection, node.Selection ) )
            next = selection.ToExpression();

        Push( next );
    }

    public void VisitRawQuery(SqlRawQueryExpressionNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var dataSource = Handle( node.DataSource, node );
        var selection = HandleCollection( node.Selection, node );
        var traits = HandleTraits( node.Traits, node );

        if ( ! ReferenceEquals( dataSource, node.DataSource ) || selection is not null || traits is not null )
            next = dataSource.Select( selection ?? node.Selection.GetUnderlyingArray() ).SetTraits( traits ?? node.Traits );

        Push( next );
    }

    public void VisitCompoundQuery(SqlCompoundQueryExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var firstQuery = Handle( node.FirstQuery, node );
        var followingQueries = HandleCollection( node.FollowingQueries, node );
        var traits = HandleTraits( node.Traits, node );

        if ( ! ReferenceEquals( firstQuery, node.FirstQuery ) || followingQueries is not null || traits is not null )
            next = firstQuery.CompoundWith( followingQueries ?? node.FollowingQueries.GetUnderlyingArray() )
                .SetTraits( traits ?? node.Traits );

        Push( next );
    }

    public void VisitCompoundQueryComponent(SqlCompoundQueryComponentNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var query = Handle( node.Query, node );
        if ( ! ReferenceEquals( query, node.Query ) )
            next = SqlNode.CompoundWith( node.Operator, query );

        Push( next );
    }

    public void VisitDistinctTrait(SqlDistinctTraitNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitFilterTrait(SqlFilterTraitNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var filter = Handle( node.Filter, node );
        if ( ! ReferenceEquals( filter, node.Filter ) )
            next = SqlNode.FilterTrait( filter, node.IsConjunction );

        Push( next );
    }

    public void VisitAggregationTrait(SqlAggregationTraitNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var expressions = HandleCollection( node.Expressions, node );
        if ( expressions is not null )
            next = SqlNode.AggregationTrait( expressions );

        Push( next );
    }

    public void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var filter = Handle( node.Filter, node );
        if ( ! ReferenceEquals( filter, node.Filter ) )
            next = SqlNode.AggregationFilterTrait( filter, node.IsConjunction );

        Push( next );
    }

    public void VisitSortTrait(SqlSortTraitNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var ordering = HandleCollection( node.Ordering, node );
        if ( ordering is not null )
            next = SqlNode.SortTrait( ordering );

        Push( next );
    }

    public void VisitLimitTrait(SqlLimitTraitNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var value = Handle( node.Value, node );
        if ( ! ReferenceEquals( value, node.Value ) )
            next = SqlNode.LimitTrait( value );

        Push( next );
    }

    public void VisitOffsetTrait(SqlOffsetTraitNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var value = Handle( node.Value, node );
        if ( ! ReferenceEquals( value, node.Value ) )
            next = SqlNode.OffsetTrait( value );

        Push( next );
    }

    public void VisitCommonTableExpressionTrait(SqlCommonTableExpressionTraitNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var commonTableExpressions = HandleCollection( node.CommonTableExpressions, node );
        if ( commonTableExpressions is not null )
            next = SqlNode.CommonTableExpressionTrait( commonTableExpressions );

        Push( next );
    }

    public void VisitWindowDefinitionTrait(SqlWindowDefinitionTraitNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var windows = HandleCollection( node.Windows, node );
        if ( windows is not null )
            next = SqlNode.WindowDefinitionTrait( windows );

        Push( next );
    }

    public void VisitWindowTrait(SqlWindowTraitNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var definition = Handle( node.Definition, node );
        if ( ! ReferenceEquals( definition, node.Definition ) )
            next = SqlNode.WindowTrait( definition );

        Push( next );
    }

    public void VisitOrderBy(SqlOrderByNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var expression = Handle( node.Expression, node );
        if ( ! ReferenceEquals( expression, node.Expression ) )
            next = SqlNode.OrderBy( expression, node.Ordering );

        Push( next );
    }

    public void VisitCommonTableExpression(SqlCommonTableExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var query = Handle( node.Query, node );
        if ( ! ReferenceEquals( query, node.Query ) )
            next = node.IsRecursive
                ? SqlNode.RecursiveCommonTableExpression(
                    CastOrThrow<SqlCompoundQueryExpressionNode>( query, node.Query, node ),
                    node.Name )
                : SqlNode.OrdinalCommonTableExpression( query, node.Name );

        Push( next );
    }

    public void VisitWindowDefinition(SqlWindowDefinitionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var partitioning = HandleCollection( node.Partitioning, node );
        var ordering = HandleCollection( node.Ordering, node );
        var frame = node.Frame is null ? null : Handle( node.Frame, node );

        if ( partitioning is not null || ordering is not null || ! ReferenceEquals( frame, node.Frame ) )
            next = SqlNode.WindowDefinition(
                node.Name,
                partitioning ?? node.Partitioning.GetUnderlyingArray().ToArray(),
                ordering ?? node.Ordering.GetUnderlyingArray().ToArray(),
                frame );

        Push( next );
    }

    public void VisitWindowFrame(SqlWindowFrameNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitTypeCast(SqlTypeCastExpressionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var value = Handle( node.Value, node );
        if ( ! ReferenceEquals( value, node.Value ) )
            next = node.TargetTypeDefinition is null
                ? SqlNode.TypeCast( value, node.TargetType )
                : SqlNode.TypeCast( value, node.TargetTypeDefinition );

        Push( next );
    }

    public void VisitValues(SqlValuesNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        Array? expressions = null;
        if ( node.RowCount == 1 )
            expressions = HandleCollection( node[0], node );
        else
        {
            for ( var r = 0; r < node.RowCount; ++r )
            {
                var row = node[r];
                for ( var i = 0; i < row.Length; ++i )
                {
                    var cell = row[i];
                    var nextCell = Handle( cell, node );

                    if ( ! ReferenceEquals( nextCell, cell ) && expressions is null )
                    {
                        expressions = new SqlExpressionNode[row.Length, node.ColumnCount];
                        for ( var e = 0; e < r; ++e )
                        {
                            var originalRow = node[e];
                            for ( var j = 0; j < originalRow.Length; ++j )
                                ReinterpretCast.To<SqlExpressionNode[,]>( expressions )[e, j] = originalRow[j];
                        }

                        for ( var j = 0; j < i; ++j )
                            ReinterpretCast.To<SqlExpressionNode[,]>( expressions )[r, j] = row[j];
                    }

                    if ( expressions is not null )
                        ReinterpretCast.To<SqlExpressionNode[,]>( expressions )[r, i] = nextCell;
                }
            }
        }

        if ( expressions is not null )
            next = node.RowCount == 1
                ? SqlNode.Values( ReinterpretCast.To<SqlExpressionNode[]>( expressions ) )
                : SqlNode.Values( ReinterpretCast.To<SqlExpressionNode[,]>( expressions ) );

        Push( next );
    }

    public void VisitRawStatement(SqlRawStatementNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitInsertInto(SqlInsertIntoNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var source = Handle( node.Source, node );
        var recordSet = Handle( node.RecordSet, node );
        var dataFields = HandleCollection( node.DataFields, node );
        if ( ! ReferenceEquals( source, node.Source ) || ! ReferenceEquals( recordSet, node.RecordSet ) || dataFields is not null )
            next = source is SqlValuesNode values
                ? values.ToInsertInto( recordSet, dataFields ?? node.DataFields.GetUnderlyingArray().ToArray() )
                : CastOrThrow<SqlQueryExpressionNode>( source, node.Source, node )
                    .ToInsertInto( recordSet, dataFields ?? node.DataFields.GetUnderlyingArray().ToArray() );

        Push( next );
    }

    public void VisitUpdate(SqlUpdateNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var dataSource = Handle( node.DataSource, node );
        var assignments = HandleCollection( node.Assignments, node );
        if ( ! ReferenceEquals( dataSource, node.DataSource ) || assignments is not null )
            next = dataSource.ToUpdate( assignments ?? node.Assignments.GetUnderlyingArray().ToArray() );

        Push( next );
    }

    public void VisitUpsert(SqlUpsertNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var source = Handle( node.Source, node );
        var recordSet = Handle( node.RecordSet, node );
        var insertDataFields = HandleCollection( node.InsertDataFields, node );
        var conflictTarget = HandleCollection( node.ConflictTarget, node );
        var updateAssignments = HandleCollection( node.UpdateAssignments, node );

        if ( ! ReferenceEquals( source, node.Source )
            || ! ReferenceEquals( recordSet, node.RecordSet )
            || insertDataFields is not null
            || conflictTarget is not null
            || updateAssignments is not null )
        {
            insertDataFields ??= node.InsertDataFields.GetUnderlyingArray().ToArray();
            conflictTarget ??= node.ConflictTarget.GetUnderlyingArray().ToArray();
            updateAssignments ??= node.UpdateAssignments.GetUnderlyingArray().ToArray();
            var updateAssignmentsProvider = (SqlRecordSetNode r, SqlInternalRecordSetNode i) =>
            {
                var mutator = new SqlNodeMutator( new SqlRecordSetReplacerContext( node.UpdateSource, i ) );
                for ( var j = 0; j < updateAssignments.Length; ++j )
                {
                    var assignment = updateAssignments[j];
                    mutator.VisitValueAssignment( assignment );
                    updateAssignments[j] = CastOrThrow<SqlValueAssignmentNode>( mutator.GetResult(), assignment, node );
                }

                return updateAssignments.AsEnumerable();
            };

            next = source is SqlValuesNode values
                ? values.ToUpsert( recordSet, insertDataFields, updateAssignmentsProvider, conflictTarget )
                : CastOrThrow<SqlQueryExpressionNode>( source, node.Source, node )
                    .ToUpsert( recordSet, insertDataFields, updateAssignmentsProvider, conflictTarget );
        }

        Push( next );
    }

    public void VisitValueAssignment(SqlValueAssignmentNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var dataField = Handle( node.DataField, node );
        var value = Handle( node.Value, node );
        if ( ! ReferenceEquals( value, node.Value ) || ! ReferenceEquals( dataField, node.DataField ) )
            next = dataField.Assign( value );

        Push( next );
    }

    public void VisitDeleteFrom(SqlDeleteFromNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var dataSource = Handle( node.DataSource, node );
        if ( ! ReferenceEquals( dataSource, node.DataSource ) )
            next = dataSource.ToDeleteFrom();

        Push( next );
    }

    public void VisitTruncate(SqlTruncateNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var table = Handle( node.Table, node );
        if ( ! ReferenceEquals( table, node.Table ) )
            next = table.ToTruncate();

        Push( next );
    }

    public void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var defaultValue = node.DefaultValue is null ? null : Handle( node.DefaultValue, node );
        var computationExpression = node.Computation is null ? null : Handle( node.Computation.Value.Expression, node );

        if ( ! ReferenceEquals( defaultValue, node.DefaultValue )
            || ! ReferenceEquals( computationExpression, node.Computation?.Expression ) )
        {
            var computation = computationExpression is null
                ? ( SqlColumnComputation? )null
                : new SqlColumnComputation( computationExpression, node.Computation!.Value.Storage );

            next = node.TypeDefinition is null
                ? SqlNode.Column( node.Name, node.Type, defaultValue, computation )
                : SqlNode.Column( node.Name, node.TypeDefinition, node.Type.IsNullable, defaultValue, computation );
        }

        Push( next );
    }

    public void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var ordering = HandleCollection( node.Columns, node );
        if ( ordering is not null )
            next = SqlNode.PrimaryKey( node.Name, ordering );

        Push( next );
    }

    public void VisitForeignKeyDefinition(SqlForeignKeyDefinitionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var columns = HandleCollection( node.Columns, node );
        var referencedTable = Handle( node.ReferencedTable, node );
        var referencedColumns = HandleCollection( node.ReferencedColumns, node );

        if ( columns is not null || referencedColumns is not null || ! ReferenceEquals( referencedTable, node.ReferencedTable ) )
            next = SqlNode.ForeignKey(
                node.Name,
                columns ?? node.Columns.GetUnderlyingArray().ToArray(),
                referencedTable,
                referencedColumns ?? node.ReferencedColumns.GetUnderlyingArray().ToArray(),
                node.OnDeleteBehavior,
                node.OnUpdateBehavior );

        Push( next );
    }

    public void VisitCheckDefinition(SqlCheckDefinitionNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var condition = Handle( node.Condition, node );
        if ( ! ReferenceEquals( condition, node.Condition ) )
            next = SqlNode.Check( node.Name, condition );

        Push( next );
    }

    public void VisitCreateTable(SqlCreateTableNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var columns = HandleCollection( node.Columns, node );
        var primaryKey = node.PrimaryKey is null ? null : Handle( node.PrimaryKey, node );
        var foreignKeys = HandleCollection( node.ForeignKeys, node );
        var checks = HandleCollection( node.Checks, node );

        if ( columns is not null || ! ReferenceEquals( primaryKey, node.PrimaryKey ) || foreignKeys is not null || checks is not null )
        {
            foreignKeys ??= node.ForeignKeys.GetUnderlyingArray().ToArray();
            checks ??= node.Checks.GetUnderlyingArray().ToArray();

            Func<SqlNewTableNode, SqlCreateTableConstraints>? constraintsProvider =
                primaryKey is not null || foreignKeys.Length > 0 || checks.Length > 0
                    ? t =>
                    {
                        var result = SqlCreateTableConstraints.Empty;
                        var mutator = new SqlNodeMutator( new SqlRecordSetReplacerContext( node.RecordSet, t ) );
                        if ( primaryKey is not null )
                        {
                            mutator.VisitPrimaryKeyDefinition( primaryKey );
                            result = result.WithPrimaryKey(
                                CastOrThrow<SqlPrimaryKeyDefinitionNode>( mutator.GetResult(), primaryKey, node ) );
                        }

                        for ( var i = 0; i < foreignKeys.Length; ++i )
                        {
                            var foreignKey = foreignKeys[i];
                            mutator.VisitForeignKeyDefinition( foreignKey );
                            foreignKeys[i] = CastOrThrow<SqlForeignKeyDefinitionNode>( mutator.GetResult(), foreignKey, node );
                        }

                        for ( var i = 0; i < checks.Length; ++i )
                        {
                            var check = checks[i];
                            mutator.VisitCheckDefinition( check );
                            checks[i] = CastOrThrow<SqlCheckDefinitionNode>( mutator.GetResult(), check, node );
                        }

                        return result.WithForeignKeys( foreignKeys ).WithChecks( checks );
                    }
                    : null;

            next = SqlNode.CreateTable(
                node.Info,
                columns ?? node.Columns.GetUnderlyingArray().ToArray(),
                node.IfNotExists,
                constraintsProvider );
        }

        Push( next );
    }

    public void VisitCreateView(SqlCreateViewNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var source = Handle( node.Source, node );
        if ( ! ReferenceEquals( source, node.Source ) )
            next = SqlNode.CreateView( node.Info, source, node.ReplaceIfExists );

        Push( next );
    }

    public void VisitCreateIndex(SqlCreateIndexNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var table = Handle( node.Table, node );
        var columns = HandleCollection( node.Columns, node );
        var filter = node.Filter is null ? null : Handle( node.Filter, node );

        if ( ! ReferenceEquals( table, node.Table ) || columns is not null || ! ReferenceEquals( filter, node.Filter ) )
            next = SqlNode.CreateIndex( node.Name, node.IsUnique, table, columns ?? node.Columns, node.ReplaceIfExists, filter );

        Push( next );
    }

    public void VisitRenameTable(SqlRenameTableNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitRenameColumn(SqlRenameColumnNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitAddColumn(SqlAddColumnNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var definition = Handle( node.Definition, node );
        if ( ! ReferenceEquals( definition, node.Definition ) )
            next = SqlNode.AddColumn( node.Table, definition );

        Push( next );
    }

    public void VisitDropColumn(SqlDropColumnNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitDropTable(SqlDropTableNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitDropView(SqlDropViewNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitDropIndex(SqlDropIndexNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitStatementBatch(SqlStatementBatchNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        ISqlStatementNode[]? statements = null;
        for ( var i = 0; i < node.Statements.Count; ++i )
        {
            var statement = node.Statements[i];
            var nextStatement = CastOrThrow<ISqlStatementNode>( Handle( statement.Node, node ), statement.Node, node );

            if ( ! ReferenceEquals( nextStatement, statement ) && statements is null )
            {
                statements = new ISqlStatementNode[node.Statements.Count];
                node.Statements.AsSpan().Slice( 0, i ).CopyTo( statements );
            }

            if ( statements is not null )
                statements[i] = nextStatement;
        }

        if ( statements is not null )
            next = SqlNode.Batch( statements );

        Push( next );
    }

    public void VisitBeginTransaction(SqlBeginTransactionNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitCommitTransaction(SqlCommitTransactionNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitRollbackTransaction(SqlRollbackTransactionNode node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    public void VisitCustom(SqlNodeBase node)
    {
        var mutation = Context.Mutate( node, _ancestors );
        HandleLeafNode( node, mutation.Value, mutation.IsNode );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal SqlNodeBase GetResult()
    {
        Assume.Equals( _ancestors.Count, 0 );
        return GetHandleResult();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private SqlNodeBase GetHandleResult()
    {
        Assume.IsNotNull( _result );
        var result = _result;
        _result = null;
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void HandleLeafNode(SqlNodeBase original, SqlNodeBase next, bool visit)
    {
        if ( ! visit || ReferenceEquals( next, original ) )
            Push( next );
        else
            this.Visit( next );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool TryHandleVisitable(SqlNodeBase original, SqlNodeBase next, bool visit)
    {
        if ( ! visit )
        {
            Push( next );
            return true;
        }

        if ( ReferenceEquals( next, original ) )
            return false;

        this.Visit( next );
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void VisitDataField(SqlDataFieldNode node)
    {
        var (next, visit) = Context.Mutate( node, _ancestors );
        if ( TryHandleVisitable( node, next, visit ) )
            return;

        var recordSet = Handle( node.RecordSet, node );
        if ( ! ReferenceEquals( recordSet, node.RecordSet ) )
            next = node.ReplaceRecordSet( recordSet );

        Push( next );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private T[]? HandleCollection<T>(
        ReadOnlyArray<T> collection,
        SqlNodeBase parent,
        [CallerArgumentExpression( "collection" )]
        string description = "")
        where T : SqlNodeBase
    {
        return HandleCollection( collection.AsSpan(), parent, description );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private T[]? HandleCollection<T>(
        ReadOnlySpan<T> collection,
        SqlNodeBase parent,
        [CallerArgumentExpression( "collection" )]
        string description = "")
        where T : SqlNodeBase
    {
        T[]? result = null;
        for ( var i = 0; i < collection.Length; ++i )
        {
            var element = collection[i];
            var nextElement = Handle( element, parent, description );
            if ( ! ReferenceEquals( nextElement, element ) && result is null )
            {
                result = new T[collection.Length];
                collection.Slice( 0, i ).CopyTo( result );
            }

            if ( result is not null )
                result[i] = nextElement;
        }

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<SqlTraitNode>? HandleTraits(
        Chain<SqlTraitNode> traits,
        SqlNodeBase parent,
        [CallerArgumentExpression( "traits" )] string description = "")
    {
        var i = 0;
        Chain<SqlTraitNode>? result = null;
        foreach ( var trait in traits )
        {
            var nextTrait = Handle( trait, parent, description );
            if ( ! ReferenceEquals( nextTrait, trait ) && result is null )
            {
                var j = 0;
                result = Chain<SqlTraitNode>.Empty;
                foreach ( var t in traits )
                {
                    if ( j >= i )
                        break;

                    result = result.Value.Extend( t );
                    ++j;
                }
            }

            result = result?.Extend( nextTrait );
            ++i;
        }

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void Push(SqlNodeBase? node)
    {
        Assume.IsNotNull( node );
        Assume.IsNull( _result );
        _result = node;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private T Handle<T>(T node, SqlNodeBase parent, [CallerArgumentExpression( "node" )] string description = "")
        where T : SqlNodeBase
    {
        using ( _ancestors.Push( parent ) )
            this.Visit( node );

        return CastOrThrow<T>( GetHandleResult(), node, parent, description );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static T CastOrThrow<T>(
        SqlNodeBase next,
        SqlNodeBase node,
        SqlNodeBase parent,
        [CallerArgumentExpression( "node" )] string description = "")
        where T : class
    {
        if ( next is T result )
            return result;

        ExceptionThrower.Throw( new SqlNodeMutatorException( parent, node, next, typeof( T ), description ) );
        return default!;
    }
}
