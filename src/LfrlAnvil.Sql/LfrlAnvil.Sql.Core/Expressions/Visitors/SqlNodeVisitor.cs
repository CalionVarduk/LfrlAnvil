using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public abstract class SqlNodeVisitor : ISqlNodeVisitor
{
    public virtual void VisitRawExpression(SqlRawExpressionNode node)
    {
        foreach ( var parameter in node.Parameters )
            VisitParameter( parameter );
    }

    public virtual void VisitRawDataField(SqlRawDataFieldNode node)
    {
        this.Visit( node.RecordSet );
    }

    public virtual void VisitNull(SqlNullNode node) { }
    public virtual void VisitLiteral(SqlLiteralNode node) { }
    public virtual void VisitParameter(SqlParameterNode node) { }

    public virtual void VisitColumn(SqlColumnNode node)
    {
        this.Visit( node.RecordSet );
    }

    public virtual void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        this.Visit( node.RecordSet );
    }

    public virtual void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        this.Visit( node.RecordSet );
    }

    public virtual void VisitViewDataField(SqlViewDataFieldNode node)
    {
        this.Visit( node.RecordSet );
    }

    public virtual void VisitNegate(SqlNegateExpressionNode node)
    {
        this.Visit( node.Value );
    }

    public virtual void VisitAdd(SqlAddExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitConcat(SqlConcatExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitSubtract(SqlSubtractExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitMultiply(SqlMultiplyExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitDivide(SqlDivideExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitModulo(SqlModuloExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitBitwiseNot(SqlBitwiseNotExpressionNode node)
    {
        this.Visit( node.Value );
    }

    public virtual void VisitBitwiseAnd(SqlBitwiseAndExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitBitwiseOr(SqlBitwiseOrExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitBitwiseXor(SqlBitwiseXorExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitBitwiseLeftShift(SqlBitwiseLeftShiftExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitBitwiseRightShift(SqlBitwiseRightShiftExpressionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitSwitchCase(SqlSwitchCaseNode node)
    {
        this.Visit( node.Condition );
        this.Visit( node.Expression );
    }

    public virtual void VisitSwitch(SqlSwitchExpressionNode node)
    {
        foreach ( var @case in node.Cases )
            VisitSwitchCase( @case );

        this.Visit( node.Default );
    }

    public virtual void VisitRecordsAffectedFunction(SqlRecordsAffectedFunctionExpressionNode node) { }

    public virtual void VisitCoalesceFunction(SqlCoalesceFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node) { }
    public virtual void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node) { }
    public virtual void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node) { }
    public virtual void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node) { }
    public virtual void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node) { }

    public virtual void VisitLengthFunction(SqlLengthFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitToLowerFunction(SqlToLowerFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitToUpperFunction(SqlToUpperFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitTrimStartFunction(SqlTrimStartFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitTrimFunction(SqlTrimFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitSignFunction(SqlSignFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitAbsFunction(SqlAbsFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitCeilingFunction(SqlCeilingFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitFloorFunction(SqlFloorFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitTruncateFunction(SqlTruncateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitPowerFunction(SqlPowerFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitMinFunction(SqlMinFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitMaxFunction(SqlMaxFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitCustomFunction(SqlFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );
    }

    public virtual void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitRowNumberWindowFunction(SqlRowNumberWindowFunctionExpressionNode node)
    {
        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitRankWindowFunction(SqlRankWindowFunctionExpressionNode node)
    {
        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitDenseRankWindowFunction(SqlDenseRankWindowFunctionExpressionNode node)
    {
        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitCumulativeDistributionWindowFunction(SqlCumulativeDistributionWindowFunctionExpressionNode node)
    {
        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitNTileWindowFunction(SqlNTileWindowFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitLagWindowFunction(SqlLagWindowFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitLeadWindowFunction(SqlLeadWindowFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitFirstValueWindowFunction(SqlFirstValueWindowFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitLastValueWindowFunction(SqlLastValueWindowFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitNthValueWindowFunction(SqlNthValueWindowFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        foreach ( var arg in node.Arguments )
            this.Visit( arg );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public virtual void VisitRawCondition(SqlRawConditionNode node)
    {
        foreach ( var parameter in node.Parameters )
            VisitParameter( parameter );
    }

    public virtual void VisitTrue(SqlTrueNode node) { }
    public virtual void VisitFalse(SqlFalseNode node) { }

    public virtual void VisitEqualTo(SqlEqualToConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitNotEqualTo(SqlNotEqualToConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitGreaterThan(SqlGreaterThanConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitLessThan(SqlLessThanConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitGreaterThanOrEqualTo(SqlGreaterThanOrEqualToConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitLessThanOrEqualTo(SqlLessThanOrEqualToConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitAnd(SqlAndConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
    }

    public virtual void VisitOr(SqlOrConditionNode node)
    {
        this.Visit( node.Left );
        this.Visit( node.Right );
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
        foreach ( var expression in node.Expressions )
            this.Visit( expression );
    }

    public virtual void VisitInQuery(SqlInQueryConditionNode node)
    {
        this.Visit( node.Value );
        this.Visit( node.Query );
    }

    public virtual void VisitRawRecordSet(SqlRawRecordSetNode node) { }
    public virtual void VisitTable(SqlTableNode node) { }
    public virtual void VisitTableBuilder(SqlTableBuilderNode node) { }
    public virtual void VisitView(SqlViewNode node) { }
    public virtual void VisitViewBuilder(SqlViewBuilderNode node) { }

    public virtual void VisitQueryRecordSet(SqlQueryRecordSetNode node)
    {
        this.Visit( node.Query );
    }

    public virtual void VisitCommonTableExpressionRecordSet(SqlCommonTableExpressionRecordSetNode node) { }
    public virtual void VisitNewTable(SqlNewTableNode node) { }
    public virtual void VisitNewView(SqlNewViewNode node) { }

    public virtual void VisitJoinOn(SqlDataSourceJoinOnNode node)
    {
        this.Visit( node.InnerRecordSet );
        this.Visit( node.OnExpression );
    }

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

    public virtual void VisitSelectField(SqlSelectFieldNode node)
    {
        this.Visit( node.Expression );
    }

    public virtual void VisitSelectCompoundField(SqlSelectCompoundFieldNode node) { }
    public virtual void VisitSelectRecordSet(SqlSelectRecordSetNode node) { }
    public virtual void VisitSelectAll(SqlSelectAllNode node) { }

    public virtual void VisitSelectExpression(SqlSelectExpressionNode node)
    {
        this.Visit( node.Selection );
    }

    public virtual void VisitRawQuery(SqlRawQueryExpressionNode node)
    {
        foreach ( var parameter in node.Parameters )
            VisitParameter( parameter );
    }

    public virtual void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node)
    {
        foreach ( var selection in node.Selection )
            this.Visit( selection );

        this.Visit( node.DataSource );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

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

    public virtual void VisitCompoundQueryComponent(SqlCompoundQueryComponentNode node)
    {
        this.Visit( node.Query );
    }

    public virtual void VisitDistinctTrait(SqlDistinctTraitNode node) { }

    public virtual void VisitFilterTrait(SqlFilterTraitNode node)
    {
        this.Visit( node.Filter );
    }

    public virtual void VisitAggregationTrait(SqlAggregationTraitNode node)
    {
        foreach ( var expression in node.Expressions )
            this.Visit( expression );
    }

    public virtual void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node)
    {
        this.Visit( node.Filter );
    }

    public virtual void VisitSortTrait(SqlSortTraitNode node)
    {
        foreach ( var orderBy in node.Ordering )
            VisitOrderBy( orderBy );
    }

    public virtual void VisitLimitTrait(SqlLimitTraitNode node)
    {
        this.Visit( node.Value );
    }

    public virtual void VisitOffsetTrait(SqlOffsetTraitNode node)
    {
        this.Visit( node.Value );
    }

    public virtual void VisitCommonTableExpressionTrait(SqlCommonTableExpressionTraitNode node)
    {
        foreach ( var cte in node.CommonTableExpressions )
            VisitCommonTableExpression( cte );
    }

    public virtual void VisitWindowDefinitionTrait(SqlWindowDefinitionTraitNode node)
    {
        foreach ( var definition in node.Windows )
            VisitWindowDefinition( definition );
    }

    public virtual void VisitWindowTrait(SqlWindowTraitNode node)
    {
        VisitWindowDefinition( node.Definition );
    }

    public virtual void VisitOrderBy(SqlOrderByNode node)
    {
        this.Visit( node.Expression );
    }

    public virtual void VisitCommonTableExpression(SqlCommonTableExpressionNode node)
    {
        this.Visit( node.Query );
    }

    public virtual void VisitWindowDefinition(SqlWindowDefinitionNode node)
    {
        foreach ( var partition in node.Partitioning )
            this.Visit( partition );

        foreach ( var orderBy in node.Ordering )
            VisitOrderBy( orderBy );

        if ( node.Frame is not null )
            VisitWindowFrame( node.Frame );
    }

    public virtual void VisitWindowFrame(SqlWindowFrameNode node) { }

    public virtual void VisitTypeCast(SqlTypeCastExpressionNode node)
    {
        this.Visit( node.Value );
    }

    public virtual void VisitValues(SqlValuesNode node)
    {
        for ( var i = 0; i < node.RowCount; ++i )
        {
            var row = node[i];
            foreach ( var expr in row )
                this.Visit( expr );
        }
    }

    public virtual void VisitRawStatement(SqlRawStatementNode node)
    {
        foreach ( var parameter in node.Parameters )
            VisitParameter( parameter );
    }

    public virtual void VisitInsertInto(SqlInsertIntoNode node)
    {
        this.Visit( node.RecordSet );
        foreach ( var dataField in node.DataFields )
            this.Visit( dataField );

        this.Visit( node.Source );
    }

    public virtual void VisitUpdate(SqlUpdateNode node)
    {
        foreach ( var assignment in node.Assignments )
            VisitValueAssignment( assignment );

        this.Visit( node.DataSource );
    }

    public virtual void VisitValueAssignment(SqlValueAssignmentNode node)
    {
        this.Visit( node.DataField );
        this.Visit( node.Value );
    }

    public virtual void VisitDeleteFrom(SqlDeleteFromNode node)
    {
        this.Visit( node.DataSource );
    }

    public virtual void VisitTruncate(SqlTruncateNode node)
    {
        this.Visit( node.Table );
    }

    public virtual void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        if ( node.DefaultValue is not null )
            this.Visit( node.DefaultValue );
    }

    public virtual void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node)
    {
        foreach ( var column in node.Columns )
            VisitOrderBy( column );
    }

    public virtual void VisitForeignKeyDefinition(SqlForeignKeyDefinitionNode node)
    {
        foreach ( var column in node.Columns )
            this.Visit( column );

        this.Visit( node.ReferencedTable );
        foreach ( var column in node.ReferencedColumns )
            this.Visit( column );
    }

    public virtual void VisitCheckDefinition(SqlCheckDefinitionNode node)
    {
        this.Visit( node.Predicate );
    }

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

    public virtual void VisitCreateView(SqlCreateViewNode node)
    {
        this.Visit( node.Source );
    }

    public virtual void VisitCreateIndex(SqlCreateIndexNode node)
    {
        this.Visit( node.Table );
        foreach ( var column in node.Columns )
            VisitOrderBy( column );

        if ( node.Filter is not null )
            this.Visit( node.Filter );
    }

    public virtual void VisitRenameTable(SqlRenameTableNode node) { }
    public virtual void VisitRenameColumn(SqlRenameColumnNode node) { }

    public virtual void VisitAddColumn(SqlAddColumnNode node)
    {
        VisitColumnDefinition( node.Definition );
    }

    public virtual void VisitDropColumn(SqlDropColumnNode node) { }
    public virtual void VisitDropTable(SqlDropTableNode node) { }
    public virtual void VisitDropView(SqlDropViewNode node) { }
    public virtual void VisitDropIndex(SqlDropIndexNode node) { }

    public virtual void VisitStatementBatch(SqlStatementBatchNode node)
    {
        foreach ( var statement in node.Statements )
            this.Visit( statement.Node );
    }

    public virtual void VisitBeginTransaction(SqlBeginTransactionNode node) { }
    public virtual void VisitCommitTransaction(SqlCommitTransactionNode node) { }
    public virtual void VisitRollbackTransaction(SqlRollbackTransactionNode node) { }
    public virtual void VisitCustom(SqlNodeBase node) { }
}
