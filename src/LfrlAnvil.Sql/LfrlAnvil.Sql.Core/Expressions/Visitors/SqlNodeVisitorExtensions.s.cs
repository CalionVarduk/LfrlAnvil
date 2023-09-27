using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public static class SqlNodeVisitorExtensions
{
    public static void Visit(this ISqlNodeVisitor visitor, SqlNodeBase node)
    {
        switch ( node.NodeType )
        {
            case SqlNodeType.RawExpression:
                visitor.VisitRawExpression( ReinterpretCast.To<SqlRawExpressionNode>( node ) );
                break;

            case SqlNodeType.RawDataField:
                visitor.VisitRawDataField( ReinterpretCast.To<SqlRawDataFieldNode>( node ) );
                break;

            case SqlNodeType.Null:
                visitor.VisitNull( ReinterpretCast.To<SqlNullNode>( node ) );
                break;

            case SqlNodeType.Literal:
                visitor.VisitLiteral( ReinterpretCast.To<SqlLiteralNode>( node ) );
                break;

            case SqlNodeType.Parameter:
                visitor.VisitParameter( ReinterpretCast.To<SqlParameterNode>( node ) );
                break;

            case SqlNodeType.Column:
                visitor.VisitColumn( ReinterpretCast.To<SqlColumnNode>( node ) );
                break;

            case SqlNodeType.ColumnBuilder:
                visitor.VisitColumnBuilder( ReinterpretCast.To<SqlColumnBuilderNode>( node ) );
                break;

            case SqlNodeType.QueryDataField:
                visitor.VisitQueryDataField( ReinterpretCast.To<SqlQueryDataFieldNode>( node ) );
                break;

            case SqlNodeType.ViewDataField:
                visitor.VisitViewDataField( ReinterpretCast.To<SqlViewDataFieldNode>( node ) );
                break;

            case SqlNodeType.Negate:
                visitor.VisitNegate( ReinterpretCast.To<SqlNegateExpressionNode>( node ) );
                break;

            case SqlNodeType.Add:
                visitor.VisitAdd( ReinterpretCast.To<SqlAddExpressionNode>( node ) );
                break;

            case SqlNodeType.Concat:
                visitor.VisitConcat( ReinterpretCast.To<SqlConcatExpressionNode>( node ) );
                break;

            case SqlNodeType.Subtract:
                visitor.VisitSubtract( ReinterpretCast.To<SqlSubtractExpressionNode>( node ) );
                break;

            case SqlNodeType.Multiply:
                visitor.VisitMultiply( ReinterpretCast.To<SqlMultiplyExpressionNode>( node ) );
                break;

            case SqlNodeType.Divide:
                visitor.VisitDivide( ReinterpretCast.To<SqlDivideExpressionNode>( node ) );
                break;

            case SqlNodeType.Modulo:
                visitor.VisitModulo( ReinterpretCast.To<SqlModuloExpressionNode>( node ) );
                break;

            case SqlNodeType.BitwiseNot:
                visitor.VisitBitwiseNot( ReinterpretCast.To<SqlBitwiseNotExpressionNode>( node ) );
                break;

            case SqlNodeType.BitwiseAnd:
                visitor.VisitBitwiseAnd( ReinterpretCast.To<SqlBitwiseAndExpressionNode>( node ) );
                break;

            case SqlNodeType.BitwiseOr:
                visitor.VisitBitwiseOr( ReinterpretCast.To<SqlBitwiseOrExpressionNode>( node ) );
                break;

            case SqlNodeType.BitwiseXor:
                visitor.VisitBitwiseXor( ReinterpretCast.To<SqlBitwiseXorExpressionNode>( node ) );
                break;

            case SqlNodeType.BitwiseLeftShift:
                visitor.VisitBitwiseLeftShift( ReinterpretCast.To<SqlBitwiseLeftShiftExpressionNode>( node ) );
                break;

            case SqlNodeType.BitwiseRightShift:
                visitor.VisitBitwiseRightShift( ReinterpretCast.To<SqlBitwiseRightShiftExpressionNode>( node ) );
                break;

            case SqlNodeType.SwitchCase:
                visitor.VisitSwitchCase( ReinterpretCast.To<SqlSwitchCaseNode>( node ) );
                break;

            case SqlNodeType.Switch:
                visitor.VisitSwitch( ReinterpretCast.To<SqlSwitchExpressionNode>( node ) );
                break;

            case SqlNodeType.FunctionExpression:
                visitor.VisitFunction( ReinterpretCast.To<SqlFunctionExpressionNode>( node ) );
                break;

            case SqlNodeType.AggregateFunctionExpression:
                visitor.VisitAggregateFunction( ReinterpretCast.To<SqlAggregateFunctionExpressionNode>( node ) );
                break;

            case SqlNodeType.RawCondition:
                visitor.VisitRawCondition( ReinterpretCast.To<SqlRawConditionNode>( node ) );
                break;

            case SqlNodeType.True:
                visitor.VisitTrue( ReinterpretCast.To<SqlTrueNode>( node ) );
                break;

            case SqlNodeType.False:
                visitor.VisitFalse( ReinterpretCast.To<SqlFalseNode>( node ) );
                break;

            case SqlNodeType.EqualTo:
                visitor.VisitEqualTo( ReinterpretCast.To<SqlEqualToConditionNode>( node ) );
                break;

            case SqlNodeType.NotEqualTo:
                visitor.VisitNotEqualTo( ReinterpretCast.To<SqlNotEqualToConditionNode>( node ) );
                break;

            case SqlNodeType.GreaterThan:
                visitor.VisitGreaterThan( ReinterpretCast.To<SqlGreaterThanConditionNode>( node ) );
                break;

            case SqlNodeType.LessThan:
                visitor.VisitLessThan( ReinterpretCast.To<SqlLessThanConditionNode>( node ) );
                break;

            case SqlNodeType.GreaterThanOrEqualTo:
                visitor.VisitGreaterThanOrEqualTo( ReinterpretCast.To<SqlGreaterThanOrEqualToConditionNode>( node ) );
                break;

            case SqlNodeType.LessThanOrEqualTo:
                visitor.VisitLessThanOrEqualTo( ReinterpretCast.To<SqlLessThanOrEqualToConditionNode>( node ) );
                break;

            case SqlNodeType.And:
                visitor.VisitAnd( ReinterpretCast.To<SqlAndConditionNode>( node ) );
                break;

            case SqlNodeType.Or:
                visitor.VisitOr( ReinterpretCast.To<SqlOrConditionNode>( node ) );
                break;

            case SqlNodeType.ConditionValue:
                visitor.VisitConditionValue( ReinterpretCast.To<SqlConditionValueNode>( node ) );
                break;

            case SqlNodeType.Between:
                visitor.VisitBetween( ReinterpretCast.To<SqlBetweenConditionNode>( node ) );
                break;

            case SqlNodeType.Exists:
                visitor.VisitExists( ReinterpretCast.To<SqlExistsConditionNode>( node ) );
                break;

            case SqlNodeType.Like:
                visitor.VisitLike( ReinterpretCast.To<SqlLikeConditionNode>( node ) );
                break;

            case SqlNodeType.In:
                visitor.VisitIn( ReinterpretCast.To<SqlInConditionNode>( node ) );
                break;

            case SqlNodeType.InQuery:
                visitor.VisitInQuery( ReinterpretCast.To<SqlInQueryConditionNode>( node ) );
                break;

            case SqlNodeType.RawRecordSet:
                visitor.VisitRawRecordSet( ReinterpretCast.To<SqlRawRecordSetNode>( node ) );
                break;

            case SqlNodeType.TableRecordSet:
                visitor.VisitTableRecordSet( ReinterpretCast.To<SqlTableRecordSetNode>( node ) );
                break;

            case SqlNodeType.TableBuilderRecordSet:
                visitor.VisitTableBuilderRecordSet( ReinterpretCast.To<SqlTableBuilderRecordSetNode>( node ) );
                break;

            case SqlNodeType.ViewRecordSet:
                visitor.VisitViewRecordSet( ReinterpretCast.To<SqlViewRecordSetNode>( node ) );
                break;

            case SqlNodeType.ViewBuilderRecordSet:
                visitor.VisitViewBuilderRecordSet( ReinterpretCast.To<SqlViewBuilderRecordSetNode>( node ) );
                break;

            case SqlNodeType.QueryRecordSet:
                visitor.VisitQueryRecordSet( ReinterpretCast.To<SqlQueryRecordSetNode>( node ) );
                break;

            case SqlNodeType.CommonTableExpressionRecordSet:
                visitor.VisitCommonTableExpressionRecordSet( ReinterpretCast.To<SqlCommonTableExpressionRecordSetNode>( node ) );
                break;

            case SqlNodeType.TemporaryTableRecordSet:
                visitor.VisitTemporaryTableRecordSet( ReinterpretCast.To<SqlTemporaryTableRecordSetNode>( node ) );
                break;

            case SqlNodeType.JoinOn:
                visitor.VisitJoinOn( ReinterpretCast.To<SqlDataSourceJoinOnNode>( node ) );
                break;

            case SqlNodeType.DataSource:
                visitor.VisitDataSource( ReinterpretCast.To<SqlDataSourceNode>( node ) );
                break;

            case SqlNodeType.SelectField:
                visitor.VisitSelectField( ReinterpretCast.To<SqlSelectFieldNode>( node ) );
                break;

            case SqlNodeType.SelectCompoundField:
                visitor.VisitSelectCompoundField( ReinterpretCast.To<SqlSelectCompoundFieldNode>( node ) );
                break;

            case SqlNodeType.SelectRecordSet:
                visitor.VisitSelectRecordSet( ReinterpretCast.To<SqlSelectRecordSetNode>( node ) );
                break;

            case SqlNodeType.SelectAll:
                visitor.VisitSelectAll( ReinterpretCast.To<SqlSelectAllNode>( node ) );
                break;

            case SqlNodeType.SelectExpression:
                visitor.VisitSelectExpression( ReinterpretCast.To<SqlSelectExpressionNode>( node ) );
                break;

            case SqlNodeType.RawQuery:
                visitor.VisitRawQuery( ReinterpretCast.To<SqlRawQueryExpressionNode>( node ) );
                break;

            case SqlNodeType.DataSourceQuery:
                visitor.VisitDataSourceQuery( ReinterpretCast.To<SqlDataSourceQueryExpressionNode>( node ) );
                break;

            case SqlNodeType.CompoundQuery:
                visitor.VisitCompoundQuery( ReinterpretCast.To<SqlCompoundQueryExpressionNode>( node ) );
                break;

            case SqlNodeType.CompoundQueryComponent:
                visitor.VisitCompoundQueryComponent( ReinterpretCast.To<SqlCompoundQueryComponentNode>( node ) );
                break;

            case SqlNodeType.DistinctTrait:
                visitor.VisitDistinctTrait( ReinterpretCast.To<SqlDistinctTraitNode>( node ) );
                break;

            case SqlNodeType.FilterTrait:
                visitor.VisitFilterTrait( ReinterpretCast.To<SqlFilterTraitNode>( node ) );
                break;

            case SqlNodeType.AggregationTrait:
                visitor.VisitAggregationTrait( ReinterpretCast.To<SqlAggregationTraitNode>( node ) );
                break;

            case SqlNodeType.AggregationFilterTrait:
                visitor.VisitAggregationFilterTrait( ReinterpretCast.To<SqlAggregationFilterTraitNode>( node ) );
                break;

            case SqlNodeType.SortTrait:
                visitor.VisitSortTrait( ReinterpretCast.To<SqlSortTraitNode>( node ) );
                break;

            case SqlNodeType.LimitTrait:
                visitor.VisitLimitTrait( ReinterpretCast.To<SqlLimitTraitNode>( node ) );
                break;

            case SqlNodeType.OffsetTrait:
                visitor.VisitOffsetTrait( ReinterpretCast.To<SqlOffsetTraitNode>( node ) );
                break;

            case SqlNodeType.CommonTableExpressionTrait:
                visitor.VisitCommonTableExpressionTrait( ReinterpretCast.To<SqlCommonTableExpressionTraitNode>( node ) );
                break;

            case SqlNodeType.OrderBy:
                visitor.VisitOrderBy( ReinterpretCast.To<SqlOrderByNode>( node ) );
                break;

            case SqlNodeType.CommonTableExpression:
                visitor.VisitCommonTableExpression( ReinterpretCast.To<SqlCommonTableExpressionNode>( node ) );
                break;

            case SqlNodeType.TypeCast:
                visitor.VisitTypeCast( ReinterpretCast.To<SqlTypeCastExpressionNode>( node ) );
                break;

            case SqlNodeType.Values:
                visitor.VisitValues( ReinterpretCast.To<SqlValuesNode>( node ) );
                break;

            case SqlNodeType.InsertInto:
                visitor.VisitInsertInto( ReinterpretCast.To<SqlInsertIntoNode>( node ) );
                break;

            case SqlNodeType.Update:
                visitor.VisitUpdate( ReinterpretCast.To<SqlUpdateNode>( node ) );
                break;

            case SqlNodeType.ValueAssignment:
                visitor.VisitValueAssignment( ReinterpretCast.To<SqlValueAssignmentNode>( node ) );
                break;

            case SqlNodeType.DeleteFrom:
                visitor.VisitDeleteFrom( ReinterpretCast.To<SqlDeleteFromNode>( node ) );
                break;

            case SqlNodeType.ColumnDefinition:
                visitor.VisitColumnDefinition( ReinterpretCast.To<SqlColumnDefinitionNode>( node ) );
                break;

            case SqlNodeType.CreateTemporaryTable:
                visitor.VisitCreateTemporaryTable( ReinterpretCast.To<SqlCreateTemporaryTableNode>( node ) );
                break;

            case SqlNodeType.DropTemporaryTable:
                visitor.VisitDropTemporaryTable( ReinterpretCast.To<SqlDropTemporaryTableNode>( node ) );
                break;

            case SqlNodeType.StatementBatch:
                visitor.VisitStatementBatch( ReinterpretCast.To<SqlStatementBatchNode>( node ) );
                break;

            case SqlNodeType.BeginTransaction:
                visitor.VisitBeginTransaction( ReinterpretCast.To<SqlBeginTransactionNode>( node ) );
                break;

            case SqlNodeType.CommitTransaction:
                visitor.VisitCommitTransaction( ReinterpretCast.To<SqlCommitTransactionNode>( node ) );
                break;

            case SqlNodeType.RollbackTransaction:
                visitor.VisitRollbackTransaction( ReinterpretCast.To<SqlRollbackTransactionNode>( node ) );
                break;

            default:
                visitor.VisitCustom( node );
                break;
        }
    }

    public static void VisitFunction(this ISqlNodeVisitor visitor, SqlFunctionExpressionNode node)
    {
        switch ( node.FunctionType )
        {
            case SqlFunctionType.RecordsAffected:
                visitor.VisitRecordsAffectedFunction( ReinterpretCast.To<SqlRecordsAffectedFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Coalesce:
                visitor.VisitCoalesceFunction( ReinterpretCast.To<SqlCoalesceFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.CurrentDate:
                visitor.VisitCurrentDateFunction( ReinterpretCast.To<SqlCurrentDateFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.CurrentTime:
                visitor.VisitCurrentTimeFunction( ReinterpretCast.To<SqlCurrentTimeFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.CurrentDateTime:
                visitor.VisitCurrentDateTimeFunction( ReinterpretCast.To<SqlCurrentDateTimeFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.CurrentTimestamp:
                visitor.VisitCurrentTimestampFunction( ReinterpretCast.To<SqlCurrentTimestampFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Length:
                visitor.VisitLengthFunction( ReinterpretCast.To<SqlLengthFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.ToLower:
                visitor.VisitToLowerFunction( ReinterpretCast.To<SqlToLowerFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.ToUpper:
                visitor.VisitToUpperFunction( ReinterpretCast.To<SqlToUpperFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.TrimStart:
                visitor.VisitTrimStartFunction( ReinterpretCast.To<SqlTrimStartFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.TrimEnd:
                visitor.VisitTrimEndFunction( ReinterpretCast.To<SqlTrimEndFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Trim:
                visitor.VisitTrimFunction( ReinterpretCast.To<SqlTrimFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Substring:
                visitor.VisitSubstringFunction( ReinterpretCast.To<SqlSubstringFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Replace:
                visitor.VisitReplaceFunction( ReinterpretCast.To<SqlReplaceFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.IndexOf:
                visitor.VisitIndexOfFunction( ReinterpretCast.To<SqlIndexOfFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.LastIndexOf:
                visitor.VisitLastIndexOfFunction( ReinterpretCast.To<SqlLastIndexOfFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Sign:
                visitor.VisitSignFunction( ReinterpretCast.To<SqlSignFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Abs:
                visitor.VisitAbsFunction( ReinterpretCast.To<SqlAbsFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Ceiling:
                visitor.VisitCeilingFunction( ReinterpretCast.To<SqlCeilingFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Floor:
                visitor.VisitFloorFunction( ReinterpretCast.To<SqlFloorFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Truncate:
                visitor.VisitTruncateFunction( ReinterpretCast.To<SqlTruncateFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Power:
                visitor.VisitPowerFunction( ReinterpretCast.To<SqlPowerFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.SquareRoot:
                visitor.VisitSquareRootFunction( ReinterpretCast.To<SqlSquareRootFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Min:
                visitor.VisitMinFunction( ReinterpretCast.To<SqlMinFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Max:
                visitor.VisitMaxFunction( ReinterpretCast.To<SqlMaxFunctionExpressionNode>( node ) );
                break;

            default:
                visitor.VisitCustomFunction( node );
                break;
        }
    }

    public static void VisitAggregateFunction(this ISqlNodeVisitor visitor, SqlAggregateFunctionExpressionNode node)
    {
        switch ( node.FunctionType )
        {
            case SqlFunctionType.Min:
                visitor.VisitMinAggregateFunction( ReinterpretCast.To<SqlMinAggregateFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Max:
                visitor.VisitMaxAggregateFunction( ReinterpretCast.To<SqlMaxAggregateFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Average:
                visitor.VisitAverageAggregateFunction( ReinterpretCast.To<SqlAverageAggregateFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Sum:
                visitor.VisitSumAggregateFunction( ReinterpretCast.To<SqlSumAggregateFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.Count:
                visitor.VisitCountAggregateFunction( ReinterpretCast.To<SqlCountAggregateFunctionExpressionNode>( node ) );
                break;

            case SqlFunctionType.StringConcat:
                visitor.VisitStringConcatAggregateFunction( ReinterpretCast.To<SqlStringConcatAggregateFunctionExpressionNode>( node ) );
                break;

            default:
                visitor.VisitCustomAggregateFunction( node );
                break;
        }
    }

    public static SqlNodeInterpreterContext Interpret(this SqlNodeInterpreter interpreter, SqlNodeBase node)
    {
        interpreter.Visit( node );
        return interpreter.Context;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNodeInterpreter Create(this ISqlNodeInterpreterFactory factory)
    {
        return factory.Create( SqlNodeInterpreterContext.Create() );
    }
}
