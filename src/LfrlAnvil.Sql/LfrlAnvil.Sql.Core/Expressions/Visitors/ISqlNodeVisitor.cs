﻿using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public interface ISqlNodeVisitor
{
    void VisitRawExpression(SqlRawExpressionNode node);
    void VisitRawDataField(SqlRawDataFieldNode node);
    void VisitNull(SqlNullNode node);
    void VisitLiteral(SqlLiteralNode node);
    void VisitParameter(SqlParameterNode node);
    void VisitColumn(SqlColumnNode node);
    void VisitColumnBuilder(SqlColumnBuilderNode node);
    void VisitQueryDataField(SqlQueryDataFieldNode node);
    void VisitViewDataField(SqlViewDataFieldNode node);
    void VisitNegate(SqlNegateExpressionNode node);
    void VisitAdd(SqlAddExpressionNode node);
    void VisitConcat(SqlConcatExpressionNode node);
    void VisitSubtract(SqlSubtractExpressionNode node);
    void VisitMultiply(SqlMultiplyExpressionNode node);
    void VisitDivide(SqlDivideExpressionNode node);
    void VisitModulo(SqlModuloExpressionNode node);
    void VisitBitwiseNot(SqlBitwiseNotExpressionNode node);
    void VisitBitwiseAnd(SqlBitwiseAndExpressionNode node);
    void VisitBitwiseOr(SqlBitwiseOrExpressionNode node);
    void VisitBitwiseXor(SqlBitwiseXorExpressionNode node);
    void VisitBitwiseLeftShift(SqlBitwiseLeftShiftExpressionNode node);
    void VisitBitwiseRightShift(SqlBitwiseRightShiftExpressionNode node);
    void VisitSwitchCase(SqlSwitchCaseNode node);
    void VisitSwitch(SqlSwitchExpressionNode node);
    void VisitNamedFunction(SqlNamedFunctionExpressionNode node);
    void VisitRecordsAffectedFunction(SqlRecordsAffectedFunctionExpressionNode node);
    void VisitCoalesceFunction(SqlCoalesceFunctionExpressionNode node);
    void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node);
    void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node);
    void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node);
    void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node);
    void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node);
    void VisitLengthFunction(SqlLengthFunctionExpressionNode node);
    void VisitToLowerFunction(SqlToLowerFunctionExpressionNode node);
    void VisitToUpperFunction(SqlToUpperFunctionExpressionNode node);
    void VisitTrimStartFunction(SqlTrimStartFunctionExpressionNode node);
    void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node);
    void VisitTrimFunction(SqlTrimFunctionExpressionNode node);
    void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node);
    void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node);
    void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node);
    void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node);
    void VisitSignFunction(SqlSignFunctionExpressionNode node);
    void VisitAbsFunction(SqlAbsFunctionExpressionNode node);
    void VisitCeilingFunction(SqlCeilingFunctionExpressionNode node);
    void VisitFloorFunction(SqlFloorFunctionExpressionNode node);
    void VisitTruncateFunction(SqlTruncateFunctionExpressionNode node);
    void VisitPowerFunction(SqlPowerFunctionExpressionNode node);
    void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node);
    void VisitMinFunction(SqlMinFunctionExpressionNode node);
    void VisitMaxFunction(SqlMaxFunctionExpressionNode node);
    void VisitCustomFunction(SqlFunctionExpressionNode node);
    void VisitNamedAggregateFunction(SqlNamedAggregateFunctionExpressionNode node);
    void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node);
    void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node);
    void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node);
    void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node);
    void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node);
    void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node);
    void VisitRowNumberWindowFunction(SqlRowNumberWindowFunctionExpressionNode node);
    void VisitRankWindowFunction(SqlRankWindowFunctionExpressionNode node);
    void VisitDenseRankWindowFunction(SqlDenseRankWindowFunctionExpressionNode node);
    void VisitCumulativeDistributionWindowFunction(SqlCumulativeDistributionWindowFunctionExpressionNode node);
    void VisitNTileWindowFunction(SqlNTileWindowFunctionExpressionNode node);
    void VisitLagWindowFunction(SqlLagWindowFunctionExpressionNode node);
    void VisitLeadWindowFunction(SqlLeadWindowFunctionExpressionNode node);
    void VisitFirstValueWindowFunction(SqlFirstValueWindowFunctionExpressionNode node);
    void VisitLastValueWindowFunction(SqlLastValueWindowFunctionExpressionNode node);
    void VisitNthValueWindowFunction(SqlNthValueWindowFunctionExpressionNode node);
    void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node);
    void VisitRawCondition(SqlRawConditionNode node);
    void VisitTrue(SqlTrueNode node);
    void VisitFalse(SqlFalseNode node);
    void VisitEqualTo(SqlEqualToConditionNode node);
    void VisitNotEqualTo(SqlNotEqualToConditionNode node);
    void VisitGreaterThan(SqlGreaterThanConditionNode node);
    void VisitLessThan(SqlLessThanConditionNode node);
    void VisitGreaterThanOrEqualTo(SqlGreaterThanOrEqualToConditionNode node);
    void VisitLessThanOrEqualTo(SqlLessThanOrEqualToConditionNode node);
    void VisitAnd(SqlAndConditionNode node);
    void VisitOr(SqlOrConditionNode node);
    void VisitConditionValue(SqlConditionValueNode node);
    void VisitBetween(SqlBetweenConditionNode node);
    void VisitExists(SqlExistsConditionNode node);
    void VisitLike(SqlLikeConditionNode node);
    void VisitIn(SqlInConditionNode node);
    void VisitInQuery(SqlInQueryConditionNode node);
    void VisitRawRecordSet(SqlRawRecordSetNode node);
    void VisitNamedFunctionRecordSet(SqlNamedFunctionRecordSetNode node);
    void VisitTable(SqlTableNode node);
    void VisitTableBuilder(SqlTableBuilderNode node);
    void VisitView(SqlViewNode node);
    void VisitViewBuilder(SqlViewBuilderNode node);
    void VisitQueryRecordSet(SqlQueryRecordSetNode node);
    void VisitCommonTableExpressionRecordSet(SqlCommonTableExpressionRecordSetNode node);
    void VisitNewTable(SqlNewTableNode node);
    void VisitNewView(SqlNewViewNode node);
    void VisitJoinOn(SqlDataSourceJoinOnNode node);
    void VisitDataSource(SqlDataSourceNode node);
    void VisitSelectField(SqlSelectFieldNode node);
    void VisitSelectCompoundField(SqlSelectCompoundFieldNode node);
    void VisitSelectRecordSet(SqlSelectRecordSetNode node);
    void VisitSelectAll(SqlSelectAllNode node);
    void VisitSelectExpression(SqlSelectExpressionNode node);
    void VisitRawQuery(SqlRawQueryExpressionNode node);
    void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node);
    void VisitCompoundQuery(SqlCompoundQueryExpressionNode node);
    void VisitCompoundQueryComponent(SqlCompoundQueryComponentNode node);
    void VisitDistinctTrait(SqlDistinctTraitNode node);
    void VisitFilterTrait(SqlFilterTraitNode node);
    void VisitAggregationTrait(SqlAggregationTraitNode node);
    void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node);
    void VisitSortTrait(SqlSortTraitNode node);
    void VisitLimitTrait(SqlLimitTraitNode node);
    void VisitOffsetTrait(SqlOffsetTraitNode node);
    void VisitCommonTableExpressionTrait(SqlCommonTableExpressionTraitNode node);
    void VisitWindowDefinitionTrait(SqlWindowDefinitionTraitNode node);
    void VisitWindowTrait(SqlWindowTraitNode node);
    void VisitOrderBy(SqlOrderByNode node);
    void VisitCommonTableExpression(SqlCommonTableExpressionNode node);
    void VisitWindowDefinition(SqlWindowDefinitionNode node);
    void VisitWindowFrame(SqlWindowFrameNode node);
    void VisitTypeCast(SqlTypeCastExpressionNode node);
    void VisitValues(SqlValuesNode node);
    void VisitRawStatement(SqlRawStatementNode node);
    void VisitInsertInto(SqlInsertIntoNode node);
    void VisitUpdate(SqlUpdateNode node);
    void VisitValueAssignment(SqlValueAssignmentNode node);
    void VisitDeleteFrom(SqlDeleteFromNode node);
    void VisitTruncate(SqlTruncateNode node);
    void VisitColumnDefinition(SqlColumnDefinitionNode node);
    void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node);
    void VisitForeignKeyDefinition(SqlForeignKeyDefinitionNode node);
    void VisitCheckDefinition(SqlCheckDefinitionNode node);
    void VisitCreateTable(SqlCreateTableNode node);
    void VisitCreateView(SqlCreateViewNode node);
    void VisitCreateIndex(SqlCreateIndexNode node);
    void VisitRenameTable(SqlRenameTableNode node);
    void VisitRenameColumn(SqlRenameColumnNode node);
    void VisitAddColumn(SqlAddColumnNode node);
    void VisitDropColumn(SqlDropColumnNode node);
    void VisitDropTable(SqlDropTableNode node);
    void VisitDropView(SqlDropViewNode node);
    void VisitDropIndex(SqlDropIndexNode node);
    void VisitStatementBatch(SqlStatementBatchNode node);
    void VisitBeginTransaction(SqlBeginTransactionNode node);
    void VisitCommitTransaction(SqlCommitTransactionNode node);
    void VisitRollbackTransaction(SqlRollbackTransactionNode node);
    void VisitCustom(SqlNodeBase node);
}
