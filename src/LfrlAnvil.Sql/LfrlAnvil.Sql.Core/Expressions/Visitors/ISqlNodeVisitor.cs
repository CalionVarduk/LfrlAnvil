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

using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <summary>
/// Represents an object capable of recursive traversal over an SQL syntax tree.
/// </summary>
public interface ISqlNodeVisitor
{
    /// <summary>
    /// Visits an <see cref="SqlRawExpressionNode"/>.
    /// </summary>
    void VisitRawExpression(SqlRawExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlRawDataFieldNode"/>.
    /// </summary>
    void VisitRawDataField(SqlRawDataFieldNode node);

    /// <summary>
    /// Visits an <see cref="SqlNullNode"/>.
    /// </summary>
    void VisitNull(SqlNullNode node);

    /// <summary>
    /// Visits an <see cref="SqlLiteralNode"/>.
    /// </summary>
    void VisitLiteral(SqlLiteralNode node);

    /// <summary>
    /// Visits an <see cref="SqlParameterNode"/>.
    /// </summary>
    void VisitParameter(SqlParameterNode node);

    /// <summary>
    /// Visits an <see cref="SqlColumnNode"/>.
    /// </summary>
    void VisitColumn(SqlColumnNode node);

    /// <summary>
    /// Visits an <see cref="SqlColumnBuilderNode"/>.
    /// </summary>
    void VisitColumnBuilder(SqlColumnBuilderNode node);

    /// <summary>
    /// Visits an <see cref="SqlQueryDataFieldNode"/>.
    /// </summary>
    void VisitQueryDataField(SqlQueryDataFieldNode node);

    /// <summary>
    /// Visits an <see cref="SqlViewDataFieldNode"/>.
    /// </summary>
    void VisitViewDataField(SqlViewDataFieldNode node);

    /// <summary>
    /// Visits an <see cref="SqlNegateExpressionNode"/>.
    /// </summary>
    void VisitNegate(SqlNegateExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlAddExpressionNode"/>.
    /// </summary>
    void VisitAdd(SqlAddExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlConcatExpressionNode"/>.
    /// </summary>
    void VisitConcat(SqlConcatExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlSubtractExpressionNode"/>.
    /// </summary>
    void VisitSubtract(SqlSubtractExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlMultiplyExpressionNode"/>.
    /// </summary>
    void VisitMultiply(SqlMultiplyExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlDivideExpressionNode"/>.
    /// </summary>
    void VisitDivide(SqlDivideExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlModuloExpressionNode"/>.
    /// </summary>
    void VisitModulo(SqlModuloExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlBitwiseNotExpressionNode"/>.
    /// </summary>
    void VisitBitwiseNot(SqlBitwiseNotExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlBitwiseAndExpressionNode"/>.
    /// </summary>
    void VisitBitwiseAnd(SqlBitwiseAndExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlBitwiseOrExpressionNode"/>.
    /// </summary>
    void VisitBitwiseOr(SqlBitwiseOrExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlBitwiseXorExpressionNode"/>.
    /// </summary>
    void VisitBitwiseXor(SqlBitwiseXorExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlBitwiseLeftShiftExpressionNode"/>.
    /// </summary>
    void VisitBitwiseLeftShift(SqlBitwiseLeftShiftExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlBitwiseRightShiftExpressionNode"/>.
    /// </summary>
    void VisitBitwiseRightShift(SqlBitwiseRightShiftExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlSwitchCaseNode"/>.
    /// </summary>
    void VisitSwitchCase(SqlSwitchCaseNode node);

    /// <summary>
    /// Visits an <see cref="SqlSwitchExpressionNode"/>.
    /// </summary>
    void VisitSwitch(SqlSwitchExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlNamedFunctionExpressionNode"/>.
    /// </summary>
    void VisitNamedFunction(SqlNamedFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlCoalesceFunctionExpressionNode"/>.
    /// </summary>
    void VisitCoalesceFunction(SqlCoalesceFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlCurrentDateFunctionExpressionNode"/>.
    /// </summary>
    void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlCurrentTimeFunctionExpressionNode"/>.
    /// </summary>
    void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlCurrentDateTimeFunctionExpressionNode"/>.
    /// </summary>
    void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlCurrentUtcDateTimeFunctionExpressionNode"/>.
    /// </summary>
    void VisitCurrentUtcDateTimeFunction(SqlCurrentUtcDateTimeFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlCurrentTimestampFunctionExpressionNode"/>.
    /// </summary>
    void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlExtractDateFunctionExpressionNode"/>.
    /// </summary>
    void VisitExtractDateFunction(SqlExtractDateFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlExtractTimeOfDayFunctionExpressionNode"/>.
    /// </summary>
    void VisitExtractTimeOfDayFunction(SqlExtractTimeOfDayFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlExtractDayFunctionExpressionNode"/>.
    /// </summary>
    void VisitExtractDayFunction(SqlExtractDayFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlExtractTemporalUnitFunctionExpressionNode"/>.
    /// </summary>
    void VisitExtractTemporalUnitFunction(SqlExtractTemporalUnitFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlTemporalAddFunctionExpressionNode"/>.
    /// </summary>
    void VisitTemporalAddFunction(SqlTemporalAddFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlTemporalDiffFunctionExpressionNode"/>.
    /// </summary>
    void VisitTemporalDiffFunction(SqlTemporalDiffFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlNewGuidFunctionExpressionNode"/>.
    /// </summary>
    void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlLengthFunctionExpressionNode"/>.
    /// </summary>
    void VisitLengthFunction(SqlLengthFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlByteLengthFunctionExpressionNode"/>.
    /// </summary>
    void VisitByteLengthFunction(SqlByteLengthFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlToLowerFunctionExpressionNode"/>.
    /// </summary>
    void VisitToLowerFunction(SqlToLowerFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlToUpperFunctionExpressionNode"/>.
    /// </summary>
    void VisitToUpperFunction(SqlToUpperFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlTrimStartFunctionExpressionNode"/>.
    /// </summary>
    void VisitTrimStartFunction(SqlTrimStartFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlTrimEndFunctionExpressionNode"/>.
    /// </summary>
    void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlTrimFunctionExpressionNode"/>.
    /// </summary>
    void VisitTrimFunction(SqlTrimFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlSubstringFunctionExpressionNode"/>.
    /// </summary>
    void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlReplaceFunctionExpressionNode"/>.
    /// </summary>
    void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlReverseFunctionExpressionNode"/>.
    /// </summary>
    void VisitReverseFunction(SqlReverseFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlIndexOfFunctionExpressionNode"/>.
    /// </summary>
    void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlLastIndexOfFunctionExpressionNode"/>.
    /// </summary>
    void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlSignFunctionExpressionNode"/>.
    /// </summary>
    void VisitSignFunction(SqlSignFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlAbsFunctionExpressionNode"/>.
    /// </summary>
    void VisitAbsFunction(SqlAbsFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlCeilingFunctionExpressionNode"/>.
    /// </summary>
    void VisitCeilingFunction(SqlCeilingFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlFloorFunctionExpressionNode"/>.
    /// </summary>
    void VisitFloorFunction(SqlFloorFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlTruncateFunctionExpressionNode"/>.
    /// </summary>
    void VisitTruncateFunction(SqlTruncateFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlRoundFunctionExpressionNode"/>.
    /// </summary>
    void VisitRoundFunction(SqlRoundFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlPowerFunctionExpressionNode"/>.
    /// </summary>
    void VisitPowerFunction(SqlPowerFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlSquareRootFunctionExpressionNode"/>.
    /// </summary>
    void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlMinFunctionExpressionNode"/>.
    /// </summary>
    void VisitMinFunction(SqlMinFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlMaxFunctionExpressionNode"/>.
    /// </summary>
    void VisitMaxFunction(SqlMaxFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlFunctionExpressionNode"/> with <see cref="SqlFunctionType.Custom"/> type.
    /// </summary>
    void VisitCustomFunction(SqlFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlNamedAggregateFunctionExpressionNode"/>.
    /// </summary>
    void VisitNamedAggregateFunction(SqlNamedAggregateFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlMinAggregateFunctionExpressionNode"/>.
    /// </summary>
    void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlMaxAggregateFunctionExpressionNode"/>.
    /// </summary>
    void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlAverageAggregateFunctionExpressionNode"/>.
    /// </summary>
    void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlSumAggregateFunctionExpressionNode"/>.
    /// </summary>
    void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlCountAggregateFunctionExpressionNode"/>.
    /// </summary>
    void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlStringConcatAggregateFunctionExpressionNode"/>.
    /// </summary>
    void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlRowNumberWindowFunctionExpressionNode"/>.
    /// </summary>
    void VisitRowNumberWindowFunction(SqlRowNumberWindowFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlRankWindowFunctionExpressionNode"/>.
    /// </summary>
    void VisitRankWindowFunction(SqlRankWindowFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlDenseRankWindowFunctionExpressionNode"/>.
    /// </summary>
    void VisitDenseRankWindowFunction(SqlDenseRankWindowFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlCumulativeDistributionWindowFunctionExpressionNode"/>.
    /// </summary>
    void VisitCumulativeDistributionWindowFunction(SqlCumulativeDistributionWindowFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlNTileWindowFunctionExpressionNode"/>.
    /// </summary>
    void VisitNTileWindowFunction(SqlNTileWindowFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlLagWindowFunctionExpressionNode"/>.
    /// </summary>
    void VisitLagWindowFunction(SqlLagWindowFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlLeadWindowFunctionExpressionNode"/>.
    /// </summary>
    void VisitLeadWindowFunction(SqlLeadWindowFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlFirstValueWindowFunctionExpressionNode"/>.
    /// </summary>
    void VisitFirstValueWindowFunction(SqlFirstValueWindowFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlLastValueWindowFunctionExpressionNode"/>.
    /// </summary>
    void VisitLastValueWindowFunction(SqlLastValueWindowFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlNthValueWindowFunctionExpressionNode"/>.
    /// </summary>
    void VisitNthValueWindowFunction(SqlNthValueWindowFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlAggregateFunctionExpressionNode"/> with <see cref="SqlFunctionType.Custom"/> type.
    /// </summary>
    void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlRawConditionNode"/>.
    /// </summary>
    void VisitRawCondition(SqlRawConditionNode node);

    /// <summary>
    /// Visits an <see cref="SqlTrueNode"/>.
    /// </summary>
    void VisitTrue(SqlTrueNode node);

    /// <summary>
    /// Visits an <see cref="SqlFalseNode"/>.
    /// </summary>
    void VisitFalse(SqlFalseNode node);

    /// <summary>
    /// Visits an <see cref="SqlEqualToConditionNode"/>.
    /// </summary>
    void VisitEqualTo(SqlEqualToConditionNode node);

    /// <summary>
    /// Visits an <see cref="SqlNotEqualToConditionNode"/>.
    /// </summary>
    void VisitNotEqualTo(SqlNotEqualToConditionNode node);

    /// <summary>
    /// Visits an <see cref="SqlGreaterThanConditionNode"/>.
    /// </summary>
    void VisitGreaterThan(SqlGreaterThanConditionNode node);

    /// <summary>
    /// Visits an <see cref="SqlLessThanConditionNode"/>.
    /// </summary>
    void VisitLessThan(SqlLessThanConditionNode node);

    /// <summary>
    /// Visits an <see cref="SqlGreaterThanOrEqualToConditionNode"/>.
    /// </summary>
    void VisitGreaterThanOrEqualTo(SqlGreaterThanOrEqualToConditionNode node);

    /// <summary>
    /// Visits an <see cref="SqlLessThanOrEqualToConditionNode"/>.
    /// </summary>
    void VisitLessThanOrEqualTo(SqlLessThanOrEqualToConditionNode node);

    /// <summary>
    /// Visits an <see cref="SqlAndConditionNode"/>.
    /// </summary>
    void VisitAnd(SqlAndConditionNode node);

    /// <summary>
    /// Visits an <see cref="SqlOrConditionNode"/>.
    /// </summary>
    void VisitOr(SqlOrConditionNode node);

    /// <summary>
    /// Visits an <see cref="SqlConditionValueNode"/>.
    /// </summary>
    void VisitConditionValue(SqlConditionValueNode node);

    /// <summary>
    /// Visits an <see cref="SqlBetweenConditionNode"/>.
    /// </summary>
    void VisitBetween(SqlBetweenConditionNode node);

    /// <summary>
    /// Visits an <see cref="SqlExistsConditionNode"/>.
    /// </summary>
    void VisitExists(SqlExistsConditionNode node);

    /// <summary>
    /// Visits an <see cref="SqlLikeConditionNode"/>.
    /// </summary>
    void VisitLike(SqlLikeConditionNode node);

    /// <summary>
    /// Visits an <see cref="SqlInConditionNode"/>.
    /// </summary>
    void VisitIn(SqlInConditionNode node);

    /// <summary>
    /// Visits an <see cref="SqlInQueryConditionNode"/>.
    /// </summary>
    void VisitInQuery(SqlInQueryConditionNode node);

    /// <summary>
    /// Visits an <see cref="SqlRawRecordSetNode"/>.
    /// </summary>
    void VisitRawRecordSet(SqlRawRecordSetNode node);

    /// <summary>
    /// Visits an <see cref="SqlNamedFunctionRecordSetNode"/>.
    /// </summary>
    void VisitNamedFunctionRecordSet(SqlNamedFunctionRecordSetNode node);

    /// <summary>
    /// Visits an <see cref="SqlTableNode"/>.
    /// </summary>
    void VisitTable(SqlTableNode node);

    /// <summary>
    /// Visits an <see cref="SqlTableBuilderNode"/>.
    /// </summary>
    void VisitTableBuilder(SqlTableBuilderNode node);

    /// <summary>
    /// Visits an <see cref="SqlViewNode"/>.
    /// </summary>
    void VisitView(SqlViewNode node);

    /// <summary>
    /// Visits an <see cref="SqlViewBuilderNode"/>.
    /// </summary>
    void VisitViewBuilder(SqlViewBuilderNode node);

    /// <summary>
    /// Visits an <see cref="SqlQueryRecordSetNode"/>.
    /// </summary>
    void VisitQueryRecordSet(SqlQueryRecordSetNode node);

    /// <summary>
    /// Visits an <see cref="SqlCommonTableExpressionRecordSetNode"/>.
    /// </summary>
    void VisitCommonTableExpressionRecordSet(SqlCommonTableExpressionRecordSetNode node);

    /// <summary>
    /// Visits an <see cref="SqlNewTableNode"/>.
    /// </summary>
    void VisitNewTable(SqlNewTableNode node);

    /// <summary>
    /// Visits an <see cref="SqlNewViewNode"/>.
    /// </summary>
    void VisitNewView(SqlNewViewNode node);

    /// <summary>
    /// Visits an <see cref="SqlDataSourceJoinOnNode"/>.
    /// </summary>
    void VisitJoinOn(SqlDataSourceJoinOnNode node);

    /// <summary>
    /// Visits an <see cref="SqlDataSourceNode"/>.
    /// </summary>
    void VisitDataSource(SqlDataSourceNode node);

    /// <summary>
    /// Visits an <see cref="SqlSelectFieldNode"/>.
    /// </summary>
    void VisitSelectField(SqlSelectFieldNode node);

    /// <summary>
    /// Visits an <see cref="SqlSelectCompoundFieldNode"/>.
    /// </summary>
    void VisitSelectCompoundField(SqlSelectCompoundFieldNode node);

    /// <summary>
    /// Visits an <see cref="SqlSelectRecordSetNode"/>.
    /// </summary>
    void VisitSelectRecordSet(SqlSelectRecordSetNode node);

    /// <summary>
    /// Visits an <see cref="SqlSelectAllNode"/>.
    /// </summary>
    void VisitSelectAll(SqlSelectAllNode node);

    /// <summary>
    /// Visits an <see cref="SqlSelectExpressionNode"/>.
    /// </summary>
    void VisitSelectExpression(SqlSelectExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlRawQueryExpressionNode"/>.
    /// </summary>
    void VisitRawQuery(SqlRawQueryExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlDataSourceQueryExpressionNode"/>.
    /// </summary>
    void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlCompoundQueryExpressionNode"/>.
    /// </summary>
    void VisitCompoundQuery(SqlCompoundQueryExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlCompoundQueryComponentNode"/>.
    /// </summary>
    void VisitCompoundQueryComponent(SqlCompoundQueryComponentNode node);

    /// <summary>
    /// Visits an <see cref="SqlDistinctTraitNode"/>.
    /// </summary>
    void VisitDistinctTrait(SqlDistinctTraitNode node);

    /// <summary>
    /// Visits an <see cref="SqlFilterTraitNode"/>.
    /// </summary>
    void VisitFilterTrait(SqlFilterTraitNode node);

    /// <summary>
    /// Visits an <see cref="SqlAggregationTraitNode"/>.
    /// </summary>
    void VisitAggregationTrait(SqlAggregationTraitNode node);

    /// <summary>
    /// Visits an <see cref="SqlAggregationFilterTraitNode"/>.
    /// </summary>
    void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node);

    /// <summary>
    /// Visits an <see cref="SqlSortTraitNode"/>.
    /// </summary>
    void VisitSortTrait(SqlSortTraitNode node);

    /// <summary>
    /// Visits an <see cref="SqlLimitTraitNode"/>.
    /// </summary>
    void VisitLimitTrait(SqlLimitTraitNode node);

    /// <summary>
    /// Visits an <see cref="SqlOffsetTraitNode"/>.
    /// </summary>
    void VisitOffsetTrait(SqlOffsetTraitNode node);

    /// <summary>
    /// Visits an <see cref="SqlCommonTableExpressionTraitNode"/>.
    /// </summary>
    void VisitCommonTableExpressionTrait(SqlCommonTableExpressionTraitNode node);

    /// <summary>
    /// Visits an <see cref="SqlWindowDefinitionTraitNode"/>.
    /// </summary>
    void VisitWindowDefinitionTrait(SqlWindowDefinitionTraitNode node);

    /// <summary>
    /// Visits an <see cref="SqlWindowTraitNode"/>.
    /// </summary>
    void VisitWindowTrait(SqlWindowTraitNode node);

    /// <summary>
    /// Visits an <see cref="SqlOrderByNode"/>.
    /// </summary>
    void VisitOrderBy(SqlOrderByNode node);

    /// <summary>
    /// Visits an <see cref="SqlCommonTableExpressionNode"/>.
    /// </summary>
    void VisitCommonTableExpression(SqlCommonTableExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlWindowDefinitionNode"/>.
    /// </summary>
    void VisitWindowDefinition(SqlWindowDefinitionNode node);

    /// <summary>
    /// Visits an <see cref="SqlWindowFrameNode"/>.
    /// </summary>
    void VisitWindowFrame(SqlWindowFrameNode node);

    /// <summary>
    /// Visits an <see cref="SqlTypeCastExpressionNode"/>.
    /// </summary>
    void VisitTypeCast(SqlTypeCastExpressionNode node);

    /// <summary>
    /// Visits an <see cref="SqlValuesNode"/>.
    /// </summary>
    void VisitValues(SqlValuesNode node);

    /// <summary>
    /// Visits an <see cref="SqlRawStatementNode"/>.
    /// </summary>
    void VisitRawStatement(SqlRawStatementNode node);

    /// <summary>
    /// Visits an <see cref="SqlInsertIntoNode"/>.
    /// </summary>
    void VisitInsertInto(SqlInsertIntoNode node);

    /// <summary>
    /// Visits an <see cref="SqlUpdateNode"/>.
    /// </summary>
    void VisitUpdate(SqlUpdateNode node);

    /// <summary>
    /// Visits an <see cref="SqlUpsertNode"/>.
    /// </summary>
    void VisitUpsert(SqlUpsertNode node);

    /// <summary>
    /// Visits an <see cref="SqlValueAssignmentNode"/>.
    /// </summary>
    void VisitValueAssignment(SqlValueAssignmentNode node);

    /// <summary>
    /// Visits an <see cref="SqlDeleteFromNode"/>.
    /// </summary>
    void VisitDeleteFrom(SqlDeleteFromNode node);

    /// <summary>
    /// Visits an <see cref="SqlTruncateNode"/>.
    /// </summary>
    void VisitTruncate(SqlTruncateNode node);

    /// <summary>
    /// Visits an <see cref="SqlColumnDefinitionNode"/>.
    /// </summary>
    void VisitColumnDefinition(SqlColumnDefinitionNode node);

    /// <summary>
    /// Visits an <see cref="SqlPrimaryKeyDefinitionNode"/>.
    /// </summary>
    void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node);

    /// <summary>
    /// Visits an <see cref="SqlForeignKeyDefinitionNode"/>.
    /// </summary>
    void VisitForeignKeyDefinition(SqlForeignKeyDefinitionNode node);

    /// <summary>
    /// Visits an <see cref="SqlCheckDefinitionNode"/>.
    /// </summary>
    void VisitCheckDefinition(SqlCheckDefinitionNode node);

    /// <summary>
    /// Visits an <see cref="SqlCreateTableNode"/>.
    /// </summary>
    void VisitCreateTable(SqlCreateTableNode node);

    /// <summary>
    /// Visits an <see cref="SqlCreateViewNode"/>.
    /// </summary>
    void VisitCreateView(SqlCreateViewNode node);

    /// <summary>
    /// Visits an <see cref="SqlCreateIndexNode"/>.
    /// </summary>
    void VisitCreateIndex(SqlCreateIndexNode node);

    /// <summary>
    /// Visits an <see cref="SqlRenameTableNode"/>.
    /// </summary>
    void VisitRenameTable(SqlRenameTableNode node);

    /// <summary>
    /// Visits an <see cref="SqlRenameColumnNode"/>.
    /// </summary>
    void VisitRenameColumn(SqlRenameColumnNode node);

    /// <summary>
    /// Visits an <see cref="SqlAddColumnNode"/>.
    /// </summary>
    void VisitAddColumn(SqlAddColumnNode node);

    /// <summary>
    /// Visits an <see cref="SqlDropColumnNode"/>.
    /// </summary>
    void VisitDropColumn(SqlDropColumnNode node);

    /// <summary>
    /// Visits an <see cref="SqlDropTableNode"/>.
    /// </summary>
    void VisitDropTable(SqlDropTableNode node);

    /// <summary>
    /// Visits an <see cref="SqlDropViewNode"/>.
    /// </summary>
    void VisitDropView(SqlDropViewNode node);

    /// <summary>
    /// Visits an <see cref="SqlDropIndexNode"/>.
    /// </summary>
    void VisitDropIndex(SqlDropIndexNode node);

    /// <summary>
    /// Visits an <see cref="SqlStatementBatchNode"/>.
    /// </summary>
    void VisitStatementBatch(SqlStatementBatchNode node);

    /// <summary>
    /// Visits an <see cref="SqlBeginTransactionNode"/>.
    /// </summary>
    void VisitBeginTransaction(SqlBeginTransactionNode node);

    /// <summary>
    /// Visits an <see cref="SqlCommitTransactionNode"/>.
    /// </summary>
    void VisitCommitTransaction(SqlCommitTransactionNode node);

    /// <summary>
    /// Visits an <see cref="SqlRollbackTransactionNode"/>.
    /// </summary>
    void VisitRollbackTransaction(SqlRollbackTransactionNode node);

    /// <summary>
    /// Visits an <see cref="SqlNodeBase"/> with <see cref="SqlNodeType.Unknown"/> type.
    /// </summary>
    void VisitCustom(SqlNodeBase node);
}
