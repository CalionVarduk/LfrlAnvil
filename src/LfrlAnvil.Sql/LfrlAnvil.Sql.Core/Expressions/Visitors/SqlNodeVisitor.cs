using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <inheritdoc />
public abstract class SqlNodeVisitor : ISqlNodeVisitor
{
    /// <inheritdoc />
    public virtual void VisitRawExpression(SqlRawExpressionNode node)
    {
        foreach ( var parameter in node.Parameters )
            VisitParameter( parameter );
    }

    /// <inheritdoc />
    public virtual void VisitRawDataField(SqlRawDataFieldNode node)
    {
        this.Visit( node.RecordSet );
    }

    /// <inheritdoc />
    public virtual void VisitNull(SqlNullNode node) { }

    /// <inheritdoc />
    public virtual void VisitLiteral(SqlLiteralNode node) { }

    /// <inheritdoc />
    public virtual void VisitParameter(SqlParameterNode node) { }

    /// <inheritdoc />
    public virtual void VisitColumn(SqlColumnNode node)
    {
        this.Visit( node.RecordSet );
    }

    /// <inheritdoc />
    public virtual void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        this.Visit( node.RecordSet );
    }

    /// <inheritdoc />
    public virtual void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        this.Visit( node.RecordSet );
    }

    /// <inheritdoc />
    public virtual void VisitViewDataField(SqlViewDataFieldNode node)
    {
        this.Visit( node.RecordSet );
    }

    /// <inheritdoc />
    public virtual void VisitNegate(SqlNegateExpressionNode node)
    {
        this.Visit( node.Value );
    }

    /// <inheritdoc />
    public virtual void VisitAdd(SqlAddExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitConcat(SqlConcatExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitSubtract(SqlSubtractExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitMultiply(SqlMultiplyExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitDivide(SqlDivideExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitModulo(SqlModuloExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitBitwiseNot(SqlBitwiseNotExpressionNode node)
    {
        this.Visit( node.Value );
    }

    /// <inheritdoc />
    public virtual void VisitBitwiseAnd(SqlBitwiseAndExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitBitwiseOr(SqlBitwiseOrExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitBitwiseXor(SqlBitwiseXorExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitBitwiseLeftShift(SqlBitwiseLeftShiftExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitBitwiseRightShift(SqlBitwiseRightShiftExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitSwitchCase(SqlSwitchCaseNode node)
    {
        this.Visit( node.Condition );
        this.Visit( node.Expression );
    }

    /// <inheritdoc />
    public virtual void VisitSwitch(SqlSwitchExpressionNode node)
    {
        foreach ( var @case in node.Cases )
            VisitSwitchCase( @case );

        this.Visit( node.Default );
    }

    /// <inheritdoc />
    public virtual void VisitNamedFunction(SqlNamedFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitCoalesceFunction(SqlCoalesceFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node) { }

    /// <inheritdoc />
    public virtual void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node) { }

    /// <inheritdoc />
    public virtual void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node) { }

    /// <inheritdoc />
    public virtual void VisitCurrentUtcDateTimeFunction(SqlCurrentUtcDateTimeFunctionExpressionNode node) { }

    /// <inheritdoc />
    public virtual void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node) { }

    /// <inheritdoc />
    public virtual void VisitExtractDateFunction(SqlExtractDateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitExtractTimeOfDayFunction(SqlExtractTimeOfDayFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitExtractDayFunction(SqlExtractDayFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitExtractTemporalUnitFunction(SqlExtractTemporalUnitFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitTemporalAddFunction(SqlTemporalAddFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitTemporalDiffFunction(SqlTemporalDiffFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node) { }

    /// <inheritdoc />
    public virtual void VisitLengthFunction(SqlLengthFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitByteLengthFunction(SqlByteLengthFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitToLowerFunction(SqlToLowerFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitToUpperFunction(SqlToUpperFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitTrimStartFunction(SqlTrimStartFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitTrimFunction(SqlTrimFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitReverseFunction(SqlReverseFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitSignFunction(SqlSignFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitAbsFunction(SqlAbsFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitCeilingFunction(SqlCeilingFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitFloorFunction(SqlFloorFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitTruncateFunction(SqlTruncateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitRoundFunction(SqlRoundFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitPowerFunction(SqlPowerFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitMinFunction(SqlMinFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitMaxFunction(SqlMaxFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitCustomFunction(SqlFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    /// <inheritdoc />
    public virtual void VisitNamedAggregateFunction(SqlNamedAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitRowNumberWindowFunction(SqlRowNumberWindowFunctionExpressionNode node)
    {
        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitRankWindowFunction(SqlRankWindowFunctionExpressionNode node)
    {
        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitDenseRankWindowFunction(SqlDenseRankWindowFunctionExpressionNode node)
    {
        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitCumulativeDistributionWindowFunction(SqlCumulativeDistributionWindowFunctionExpressionNode node)
    {
        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitNTileWindowFunction(SqlNTileWindowFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitLagWindowFunction(SqlLagWindowFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitLeadWindowFunction(SqlLeadWindowFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitFirstValueWindowFunction(SqlFirstValueWindowFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitLastValueWindowFunction(SqlLastValueWindowFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitNthValueWindowFunction(SqlNthValueWindowFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitRawCondition(SqlRawConditionNode node)
    {
        foreach ( var parameter in node.Parameters )
            VisitParameter( parameter );
    }

    /// <inheritdoc />
    public virtual void VisitTrue(SqlTrueNode node) { }

    /// <inheritdoc />
    public virtual void VisitFalse(SqlFalseNode node) { }

    /// <inheritdoc />
    public virtual void VisitEqualTo(SqlEqualToConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitNotEqualTo(SqlNotEqualToConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitGreaterThan(SqlGreaterThanConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitLessThan(SqlLessThanConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitGreaterThanOrEqualTo(SqlGreaterThanOrEqualToConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitLessThanOrEqualTo(SqlLessThanOrEqualToConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitAnd(SqlAndConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitOr(SqlOrConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    /// <inheritdoc />
    public virtual void VisitConditionValue(SqlConditionValueNode node)
    {
        this.Visit( node.Condition );
    }

    /// <inheritdoc />
    public virtual void VisitBetween(SqlBetweenConditionNode node)
    {
        this.Visit( node.Value );
        this.Visit( node.Min );
        this.Visit( node.Max );
    }

    /// <inheritdoc />
    public virtual void VisitExists(SqlExistsConditionNode node)
    {
        this.Visit( node.Query );
    }

    /// <inheritdoc />
    public virtual void VisitLike(SqlLikeConditionNode node)
    {
        this.Visit( node.Value );
        this.Visit( node.Pattern );
        if ( node.Escape is not null )
            this.Visit( node.Escape );
    }

    /// <inheritdoc />
    public virtual void VisitIn(SqlInConditionNode node)
    {
        this.Visit( node.Value );
        foreach ( var expression in node.Expressions )
            this.Visit( expression );
    }

    /// <inheritdoc />
    public virtual void VisitInQuery(SqlInQueryConditionNode node)
    {
        this.Visit( node.Value );
        this.Visit( node.Query );
    }

    /// <inheritdoc />
    public virtual void VisitRawRecordSet(SqlRawRecordSetNode node) { }

    /// <inheritdoc />
    public virtual void VisitNamedFunctionRecordSet(SqlNamedFunctionRecordSetNode node)
    {
        VisitNamedFunction( node.Function );
    }

    /// <inheritdoc />
    public virtual void VisitTable(SqlTableNode node) { }

    /// <inheritdoc />
    public virtual void VisitTableBuilder(SqlTableBuilderNode node) { }

    /// <inheritdoc />
    public virtual void VisitView(SqlViewNode node) { }

    /// <inheritdoc />
    public virtual void VisitViewBuilder(SqlViewBuilderNode node) { }

    /// <inheritdoc />
    public virtual void VisitQueryRecordSet(SqlQueryRecordSetNode node)
    {
        this.Visit( node.Query );
    }

    /// <inheritdoc />
    public virtual void VisitCommonTableExpressionRecordSet(SqlCommonTableExpressionRecordSetNode node) { }

    /// <inheritdoc />
    public virtual void VisitNewTable(SqlNewTableNode node) { }

    /// <inheritdoc />
    public virtual void VisitNewView(SqlNewViewNode node) { }

    /// <inheritdoc />
    public virtual void VisitJoinOn(SqlDataSourceJoinOnNode node)
    {
        this.Visit( node.InnerRecordSet );
        this.Visit( node.OnExpression );
    }

    /// <inheritdoc />
    public virtual void VisitDataSource(SqlDataSourceNode node)
    {
        if ( node is not SqlDummyDataSourceNode )
        {
            this.Visit( node.From );
            foreach ( var join in node.Joins )
                VisitJoinOn( join );
        }

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitSelectField(SqlSelectFieldNode node)
    {
        this.Visit( node.Expression );
    }

    /// <inheritdoc />
    public virtual void VisitSelectCompoundField(SqlSelectCompoundFieldNode node) { }

    /// <inheritdoc />
    public virtual void VisitSelectRecordSet(SqlSelectRecordSetNode node) { }

    /// <inheritdoc />
    public virtual void VisitSelectAll(SqlSelectAllNode node) { }

    /// <inheritdoc />
    public virtual void VisitSelectExpression(SqlSelectExpressionNode node)
    {
        this.Visit( node.Selection );
    }

    /// <inheritdoc />
    public virtual void VisitRawQuery(SqlRawQueryExpressionNode node)
    {
        foreach ( var parameter in node.Parameters )
            VisitParameter( parameter );
    }

    /// <inheritdoc />
    public virtual void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node)
    {
        foreach ( var selection in node.Selection )
            this.Visit( selection );

        this.Visit( node.DataSource );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitCompoundQuery(SqlCompoundQueryExpressionNode node)
    {
        foreach ( var selection in node.Selection )
            this.Visit( selection );

        this.Visit( node.FirstQuery );
        foreach ( var component in node.FollowingQueries )
            VisitCompoundQueryComponent( component );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    /// <inheritdoc />
    public virtual void VisitCompoundQueryComponent(SqlCompoundQueryComponentNode node)
    {
        this.Visit( node.Query );
    }

    /// <inheritdoc />
    public virtual void VisitDistinctTrait(SqlDistinctTraitNode node) { }

    /// <inheritdoc />
    public virtual void VisitFilterTrait(SqlFilterTraitNode node)
    {
        this.Visit( node.Filter );
    }

    /// <inheritdoc />
    public virtual void VisitAggregationTrait(SqlAggregationTraitNode node)
    {
        foreach ( var expression in node.Expressions )
            this.Visit( expression );
    }

    /// <inheritdoc />
    public virtual void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node)
    {
        this.Visit( node.Filter );
    }

    /// <inheritdoc />
    public virtual void VisitSortTrait(SqlSortTraitNode node)
    {
        foreach ( var orderBy in node.Ordering )
            VisitOrderBy( orderBy );
    }

    /// <inheritdoc />
    public virtual void VisitLimitTrait(SqlLimitTraitNode node)
    {
        this.Visit( node.Value );
    }

    /// <inheritdoc />
    public virtual void VisitOffsetTrait(SqlOffsetTraitNode node)
    {
        this.Visit( node.Value );
    }

    /// <inheritdoc />
    public virtual void VisitCommonTableExpressionTrait(SqlCommonTableExpressionTraitNode node)
    {
        foreach ( var cte in node.CommonTableExpressions )
            VisitCommonTableExpression( cte );
    }

    /// <inheritdoc />
    public virtual void VisitWindowDefinitionTrait(SqlWindowDefinitionTraitNode node)
    {
        foreach ( var definition in node.Windows )
            VisitWindowDefinition( definition );
    }

    /// <inheritdoc />
    public virtual void VisitWindowTrait(SqlWindowTraitNode node)
    {
        VisitWindowDefinition( node.Definition );
    }

    /// <inheritdoc />
    public virtual void VisitOrderBy(SqlOrderByNode node)
    {
        this.Visit( node.Expression );
    }

    /// <inheritdoc />
    public virtual void VisitCommonTableExpression(SqlCommonTableExpressionNode node)
    {
        this.Visit( node.Query );
    }

    /// <inheritdoc />
    public virtual void VisitWindowDefinition(SqlWindowDefinitionNode node)
    {
        foreach ( var partition in node.Partitioning )
            this.Visit( partition );

        foreach ( var orderBy in node.Ordering )
            VisitOrderBy( orderBy );

        if ( node.Frame is not null )
            VisitWindowFrame( node.Frame );
    }

    /// <inheritdoc />
    public virtual void VisitWindowFrame(SqlWindowFrameNode node) { }

    /// <inheritdoc />
    public virtual void VisitTypeCast(SqlTypeCastExpressionNode node)
    {
        this.Visit( node.Value );
    }

    /// <inheritdoc />
    public virtual void VisitValues(SqlValuesNode node)
    {
        for ( var i = 0; i < node.RowCount; ++i )
        {
            var row = node[i];
            foreach ( var expr in row )
                this.Visit( expr );
        }
    }

    /// <inheritdoc />
    public virtual void VisitRawStatement(SqlRawStatementNode node)
    {
        foreach ( var parameter in node.Parameters )
            VisitParameter( parameter );
    }

    /// <inheritdoc />
    public virtual void VisitInsertInto(SqlInsertIntoNode node)
    {
        this.Visit( node.RecordSet );
        foreach ( var dataField in node.DataFields )
            this.Visit( dataField );

        this.Visit( node.Source );
    }

    /// <inheritdoc />
    public virtual void VisitUpdate(SqlUpdateNode node)
    {
        foreach ( var assignment in node.Assignments )
            VisitValueAssignment( assignment );

        this.Visit( node.DataSource );
    }

    /// <inheritdoc />
    public virtual void VisitUpsert(SqlUpsertNode node)
    {
        this.Visit( node.RecordSet );
        foreach ( var dataField in node.InsertDataFields )
            this.Visit( dataField );

        foreach ( var assignment in node.UpdateAssignments )
            VisitValueAssignment( assignment );

        foreach ( var target in node.ConflictTarget )
            this.Visit( target );

        this.Visit( node.Source );
    }

    /// <inheritdoc />
    public virtual void VisitValueAssignment(SqlValueAssignmentNode node)
    {
        this.Visit( node.DataField );
        this.Visit( node.Value );
    }

    /// <inheritdoc />
    public virtual void VisitDeleteFrom(SqlDeleteFromNode node)
    {
        this.Visit( node.DataSource );
    }

    /// <inheritdoc />
    public virtual void VisitTruncate(SqlTruncateNode node)
    {
        this.Visit( node.Table );
    }

    /// <inheritdoc />
    public virtual void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        if ( node.DefaultValue is not null )
            this.Visit( node.DefaultValue );

        if ( node.Computation is not null )
            this.Visit( node.Computation.Value.Expression );
    }

    /// <inheritdoc />
    public virtual void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node)
    {
        foreach ( var column in node.Columns )
            VisitOrderBy( column );
    }

    /// <inheritdoc />
    public virtual void VisitForeignKeyDefinition(SqlForeignKeyDefinitionNode node)
    {
        foreach ( var column in node.Columns )
            this.Visit( column );

        this.Visit( node.ReferencedTable );
        foreach ( var column in node.ReferencedColumns )
            this.Visit( column );
    }

    /// <inheritdoc />
    public virtual void VisitCheckDefinition(SqlCheckDefinitionNode node)
    {
        this.Visit( node.Condition );
    }

    /// <inheritdoc />
    public virtual void VisitCreateTable(SqlCreateTableNode node)
    {
        foreach ( var column in node.Columns )
            VisitColumnDefinition( column );

        if ( node.PrimaryKey is not null )
            VisitPrimaryKeyDefinition( node.PrimaryKey );

        foreach ( var foreignKey in node.ForeignKeys )
            VisitForeignKeyDefinition( foreignKey );

        foreach ( var check in node.Checks )
            VisitCheckDefinition( check );
    }

    /// <inheritdoc />
    public virtual void VisitCreateView(SqlCreateViewNode node)
    {
        this.Visit( node.Source );
    }

    /// <inheritdoc />
    public virtual void VisitCreateIndex(SqlCreateIndexNode node)
    {
        this.Visit( node.Table );
        foreach ( var column in node.Columns )
            VisitOrderBy( column );

        if ( node.Filter is not null )
            this.Visit( node.Filter );
    }

    /// <inheritdoc />
    public virtual void VisitRenameTable(SqlRenameTableNode node) { }

    /// <inheritdoc />
    public virtual void VisitRenameColumn(SqlRenameColumnNode node) { }

    /// <inheritdoc />
    public virtual void VisitAddColumn(SqlAddColumnNode node)
    {
        VisitColumnDefinition( node.Definition );
    }

    /// <inheritdoc />
    public virtual void VisitDropColumn(SqlDropColumnNode node) { }

    /// <inheritdoc />
    public virtual void VisitDropTable(SqlDropTableNode node) { }

    /// <inheritdoc />
    public virtual void VisitDropView(SqlDropViewNode node) { }

    /// <inheritdoc />
    public virtual void VisitDropIndex(SqlDropIndexNode node) { }

    /// <inheritdoc />
    public virtual void VisitStatementBatch(SqlStatementBatchNode node)
    {
        foreach ( var statement in node.Statements )
            this.Visit( statement.Node );
    }

    /// <inheritdoc />
    public virtual void VisitBeginTransaction(SqlBeginTransactionNode node) { }

    /// <inheritdoc />
    public virtual void VisitCommitTransaction(SqlCommitTransactionNode node) { }

    /// <inheritdoc />
    public virtual void VisitRollbackTransaction(SqlRollbackTransactionNode node) { }

    /// <inheritdoc />
    public virtual void VisitCustom(SqlNodeBase node) { }
}
