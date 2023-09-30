using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sqlite.Internal;

internal abstract class SqliteSourceNodeValidator : ISqlNodeVisitor
{
    protected List<SqlNodeBase>? ForbiddenNodes;

    public virtual void VisitRawExpression(SqlRawExpressionNode node)
    {
        foreach ( var parameter in node.Parameters.Span )
            VisitParameter( parameter );
    }

    public virtual void VisitRawDataField(SqlRawDataFieldNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitNull(SqlNullNode node) { }

    public virtual void VisitLiteral(SqlLiteralNode node) { }

    public virtual void VisitParameter(SqlParameterNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitColumn(SqlColumnNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitViewDataField(SqlViewDataFieldNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitNegate(SqlNegateExpressionNode node)
    {
        this.Visit( node.Value );
    }

    public virtual void VisitAdd(SqlAddExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitConcat(SqlConcatExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitSubtract(SqlSubtractExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitMultiply(SqlMultiplyExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitDivide(SqlDivideExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitModulo(SqlModuloExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitBitwiseNot(SqlBitwiseNotExpressionNode node)
    {
        this.Visit( node.Value );
    }

    public virtual void VisitBitwiseAnd(SqlBitwiseAndExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitBitwiseOr(SqlBitwiseOrExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitBitwiseXor(SqlBitwiseXorExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitBitwiseLeftShift(SqlBitwiseLeftShiftExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitBitwiseRightShift(SqlBitwiseRightShiftExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitSwitchCase(SqlSwitchCaseNode node)
    {
        this.Visit( node.Condition );
        this.Visit( node.Expression );
    }

    public virtual void VisitSwitch(SqlSwitchExpressionNode node)
    {
        foreach ( var @case in node.Cases.Span )
            VisitSwitchCase( @case );

        this.Visit( node.Default );
    }

    public virtual void VisitRecordsAffectedFunction(SqlRecordsAffectedFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitCoalesceFunction(SqlCoalesceFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitLengthFunction(SqlLengthFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitToLowerFunction(SqlToLowerFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitToUpperFunction(SqlToUpperFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitTrimStartFunction(SqlTrimStartFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitTrimFunction(SqlTrimFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitSignFunction(SqlSignFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitAbsFunction(SqlAbsFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitCeilingFunction(SqlCeilingFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitFloorFunction(SqlFloorFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitTruncateFunction(SqlTruncateFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitPowerFunction(SqlPowerFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitMinFunction(SqlMinFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitMaxFunction(SqlMaxFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitCustomFunction(SqlFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public virtual void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node)
    {
        VisitAggregateFunction( node );
    }

    public virtual void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node)
    {
        VisitAggregateFunction( node );
    }

    public virtual void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node)
    {
        VisitAggregateFunction( node );
    }

    public virtual void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node)
    {
        VisitAggregateFunction( node );
    }

    public virtual void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node)
    {
        VisitAggregateFunction( node );
    }

    public virtual void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node)
    {
        VisitAggregateFunction( node );
    }

    public virtual void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        VisitAggregateFunction( node );
    }

    public virtual void VisitRawCondition(SqlRawConditionNode node)
    {
        foreach ( var parameter in node.Parameters.Span )
            VisitParameter( parameter );
    }

    public virtual void VisitTrue(SqlTrueNode node) { }
    public virtual void VisitFalse(SqlFalseNode node) { }

    public virtual void VisitEqualTo(SqlEqualToConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitNotEqualTo(SqlNotEqualToConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitGreaterThan(SqlGreaterThanConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitLessThan(SqlLessThanConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitGreaterThanOrEqualTo(SqlGreaterThanOrEqualToConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitLessThanOrEqualTo(SqlLessThanOrEqualToConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitAnd(SqlAndConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitOr(SqlOrConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public virtual void VisitConditionValue(SqlConditionValueNode node)
    {
        this.Visit( node.Condition );
    }

    public virtual void VisitBetween(SqlBetweenConditionNode node)
    {
        this.Visit( node.Value );
        this.Visit( node.Min );
        this.Visit( node.Max );
    }

    public virtual void VisitExists(SqlExistsConditionNode node)
    {
        this.Visit( node.Query );
    }

    public virtual void VisitLike(SqlLikeConditionNode node)
    {
        this.Visit( node.Value );
        this.Visit( node.Pattern );
        if ( node.Escape is not null )
            this.Visit( node.Escape );
    }

    public virtual void VisitIn(SqlInConditionNode node)
    {
        this.Visit( node.Value );
        foreach ( var expression in node.Expressions.Span )
            this.Visit( expression );
    }

    public virtual void VisitInQuery(SqlInQueryConditionNode node)
    {
        this.Visit( node.Value );
        this.Visit( node.Query );
    }

    public virtual void VisitRawRecordSet(SqlRawRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitTableRecordSet(SqlTableRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitTableBuilderRecordSet(SqlTableBuilderRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitViewRecordSet(SqlViewRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitViewBuilderRecordSet(SqlViewBuilderRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitQueryRecordSet(SqlQueryRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitCommonTableExpressionRecordSet(SqlCommonTableExpressionRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitTemporaryTableRecordSet(SqlTemporaryTableRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitJoinOn(SqlDataSourceJoinOnNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitDataSource(SqlDataSourceNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitSelectField(SqlSelectFieldNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitSelectCompoundField(SqlSelectCompoundFieldNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitSelectRecordSet(SqlSelectRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitSelectAll(SqlSelectAllNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitSelectExpression(SqlSelectExpressionNode node)
    {
        this.Visit( node.Selection );
    }

    public virtual void VisitRawQuery(SqlRawQueryExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitCompoundQuery(SqlCompoundQueryExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitCompoundQueryComponent(SqlCompoundQueryComponentNode node)
    {
        this.Visit( node.Query );
    }

    public virtual void VisitDistinctTrait(SqlDistinctTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitFilterTrait(SqlFilterTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitAggregationTrait(SqlAggregationTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitSortTrait(SqlSortTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitLimitTrait(SqlLimitTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitOffsetTrait(SqlOffsetTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitCommonTableExpressionTrait(SqlCommonTableExpressionTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitOrderBy(SqlOrderByNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitCommonTableExpression(SqlCommonTableExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitTypeCast(SqlTypeCastExpressionNode node)
    {
        this.Visit( node.Value );
    }

    public virtual void VisitValues(SqlValuesNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitInsertInto(SqlInsertIntoNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitUpdate(SqlUpdateNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitValueAssignment(SqlValueAssignmentNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitDeleteFrom(SqlDeleteFromNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitCreateTemporaryTable(SqlCreateTemporaryTableNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitDropTemporaryTable(SqlDropTemporaryTableNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitStatementBatch(SqlStatementBatchNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitBeginTransaction(SqlBeginTransactionNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitCommitTransaction(SqlCommitTransactionNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitRollbackTransaction(SqlRollbackTransactionNode node)
    {
        AddForbiddenNode( node );
    }

    public virtual void VisitCustom(SqlNodeBase node) { }

    protected void AddForbiddenNode(SqlNodeBase node)
    {
        if ( ForbiddenNodes is null )
        {
            ForbiddenNodes = new List<SqlNodeBase> { node };
            return;
        }

        if ( ! ForbiddenNodes.Contains( node ) )
            ForbiddenNodes.Add( node );
    }

    private void VisitBinaryOperator(SqlNodeBase left, SqlNodeBase right)
    {
        this.Visit( left );
        this.Visit( right );
    }

    private void VisitFunction(SqlFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments.Span )
            this.Visit( arg );
    }

    private void VisitAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments.Span )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }
}
