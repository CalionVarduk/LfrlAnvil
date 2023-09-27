using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteViewSourceValidator : ISqlNodeVisitor
{
    private List<SqlNodeBase>? _forbiddenNodes;

    internal SqliteViewSourceValidator(SqliteDatabaseBuilder database)
    {
        _forbiddenNodes = null;
        Database = database;
        ReferencedObjects = new Dictionary<ulong, SqliteObjectBuilder>();
    }

    internal SqliteDatabaseBuilder Database { get; }
    internal Dictionary<ulong, SqliteObjectBuilder> ReferencedObjects { get; }

    public void VisitNonQueryRecordSet(SqlRecordSetNode node)
    {
        if ( node.NodeType != SqlNodeType.QueryRecordSet )
            this.Visit( node );
    }

    public void VisitRawExpression(SqlRawExpressionNode node)
    {
        foreach ( var parameter in node.Parameters.Span )
            VisitParameter( parameter );
    }

    public void VisitRawDataField(SqlRawDataFieldNode node)
    {
        VisitNonQueryRecordSet( node.RecordSet );
    }

    public void VisitNull(SqlNullNode node) { }
    public void VisitLiteral(SqlLiteralNode node) { }

    public void VisitParameter(SqlParameterNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitColumn(SqlColumnNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        if ( ! ReferenceEquals( node.Value.Database, Database ) )
        {
            AddForbiddenNode( node );
            return;
        }

        var column = ReinterpretCast.To<SqliteColumnBuilder>( node.Value );
        if ( column.IsRemoved )
            AddForbiddenNode( node );
        else
            ReferencedObjects.TryAdd( column.Id, column );

        VisitNonQueryRecordSet( node.RecordSet );
    }

    public void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        VisitNonQueryRecordSet( node.RecordSet );
    }

    public void VisitViewDataField(SqlViewDataFieldNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitNegate(SqlNegateExpressionNode node)
    {
        this.Visit( node.Value );
    }

    public void VisitAdd(SqlAddExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitConcat(SqlConcatExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitSubtract(SqlSubtractExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitMultiply(SqlMultiplyExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitDivide(SqlDivideExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitModulo(SqlModuloExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitBitwiseNot(SqlBitwiseNotExpressionNode node)
    {
        this.Visit( node.Value );
    }

    public void VisitBitwiseAnd(SqlBitwiseAndExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitBitwiseOr(SqlBitwiseOrExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitBitwiseXor(SqlBitwiseXorExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitBitwiseLeftShift(SqlBitwiseLeftShiftExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitBitwiseRightShift(SqlBitwiseRightShiftExpressionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitSwitchCase(SqlSwitchCaseNode node)
    {
        this.Visit( node.Condition );
        this.Visit( node.Expression );
    }

    public void VisitSwitch(SqlSwitchExpressionNode node)
    {
        foreach ( var @case in node.Cases.Span )
            VisitSwitchCase( @case );

        this.Visit( node.Default );
    }

    public void VisitRecordsAffectedFunction(SqlRecordsAffectedFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitCoalesceFunction(SqlCoalesceFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitLengthFunction(SqlLengthFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitToLowerFunction(SqlToLowerFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitToUpperFunction(SqlToUpperFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitTrimStartFunction(SqlTrimStartFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitTrimEndFunction(SqlTrimEndFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitTrimFunction(SqlTrimFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitSubstringFunction(SqlSubstringFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitReplaceFunction(SqlReplaceFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitIndexOfFunction(SqlIndexOfFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitLastIndexOfFunction(SqlLastIndexOfFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitSignFunction(SqlSignFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitAbsFunction(SqlAbsFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitCeilingFunction(SqlCeilingFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitFloorFunction(SqlFloorFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitTruncateFunction(SqlTruncateFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitPowerFunction(SqlPowerFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitSquareRootFunction(SqlSquareRootFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitMinFunction(SqlMinFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitMaxFunction(SqlMaxFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitCustomFunction(SqlFunctionExpressionNode node)
    {
        VisitFunction( node );
    }

    public void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node)
    {
        VisitAggregateFunction( node );
    }

    public void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node)
    {
        VisitAggregateFunction( node );
    }

    public void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node)
    {
        VisitAggregateFunction( node );
    }

    public void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node)
    {
        VisitAggregateFunction( node );
    }

    public void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node)
    {
        VisitAggregateFunction( node );
    }

    public void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node)
    {
        VisitAggregateFunction( node );
    }

    public void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        VisitAggregateFunction( node );
    }

    public void VisitRawCondition(SqlRawConditionNode node)
    {
        foreach ( var parameter in node.Parameters.Span )
            VisitParameter( parameter );
    }

    public void VisitTrue(SqlTrueNode node) { }
    public void VisitFalse(SqlFalseNode node) { }

    public void VisitEqualTo(SqlEqualToConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitNotEqualTo(SqlNotEqualToConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitGreaterThan(SqlGreaterThanConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitLessThan(SqlLessThanConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitGreaterThanOrEqualTo(SqlGreaterThanOrEqualToConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitLessThanOrEqualTo(SqlLessThanOrEqualToConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitAnd(SqlAndConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitOr(SqlOrConditionNode node)
    {
        VisitBinaryOperator( node.Left, node.Right );
    }

    public void VisitConditionValue(SqlConditionValueNode node)
    {
        this.Visit( node.Condition );
    }

    public void VisitBetween(SqlBetweenConditionNode node)
    {
        this.Visit( node.Value );
        this.Visit( node.Min );
        this.Visit( node.Max );
    }

    public void VisitExists(SqlExistsConditionNode node)
    {
        this.Visit( node.Query );
    }

    public void VisitLike(SqlLikeConditionNode node)
    {
        this.Visit( node.Value );
        this.Visit( node.Pattern );
        if ( node.Escape is not null )
            this.Visit( node.Escape );
    }

    public void VisitIn(SqlInConditionNode node)
    {
        this.Visit( node.Value );
        foreach ( var expression in node.Expressions.Span )
            this.Visit( expression );
    }

    public void VisitInQuery(SqlInQueryConditionNode node)
    {
        this.Visit( node.Value );
        this.Visit( node.Query );
    }

    public void VisitRawRecordSet(SqlRawRecordSetNode node) { }

    public void VisitTableRecordSet(SqlTableRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitTableBuilderRecordSet(SqlTableBuilderRecordSetNode node)
    {
        if ( ! ReferenceEquals( node.Table.Database, Database ) )
        {
            AddForbiddenNode( node );
            return;
        }

        var table = ReinterpretCast.To<SqliteTableBuilder>( node.Table );
        if ( table.IsRemoved )
            AddForbiddenNode( node );
        else
            ReferencedObjects.TryAdd( table.Id, table );
    }

    public void VisitViewRecordSet(SqlViewRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitViewBuilderRecordSet(SqlViewBuilderRecordSetNode node)
    {
        if ( ! ReferenceEquals( node.View.Database, Database ) )
        {
            AddForbiddenNode( node );
            return;
        }

        var view = ReinterpretCast.To<SqliteViewBuilder>( node.View );
        if ( view.IsRemoved )
            AddForbiddenNode( node );
        else
            ReferencedObjects.TryAdd( view.Id, view );
    }

    public void VisitQueryRecordSet(SqlQueryRecordSetNode node)
    {
        this.Visit( node.Query );
    }

    public void VisitCommonTableExpressionRecordSet(SqlCommonTableExpressionRecordSetNode node) { }

    public void VisitTemporaryTableRecordSet(SqlTemporaryTableRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitJoinOn(SqlDataSourceJoinOnNode node)
    {
        this.Visit( node.InnerRecordSet );
        this.Visit( node.OnExpression );
    }

    public void VisitDataSource(SqlDataSourceNode node)
    {
        if ( node is not SqlDummyDataSourceNode )
        {
            this.Visit( node.From );
            foreach ( var join in node.Joins.Span )
                VisitJoinOn( join );
        }

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public void VisitSelectField(SqlSelectFieldNode node)
    {
        this.Visit( node.Expression );
    }

    public void VisitSelectCompoundField(SqlSelectCompoundFieldNode node) { }

    public void VisitSelectRecordSet(SqlSelectRecordSetNode node)
    {
        VisitNonQueryRecordSet( node.RecordSet );
    }

    public void VisitSelectAll(SqlSelectAllNode node) { }

    public void VisitSelectExpression(SqlSelectExpressionNode node)
    {
        this.Visit( node.Selection );
    }

    public void VisitRawQuery(SqlRawQueryExpressionNode node)
    {
        foreach ( var parameter in node.Parameters.Span )
            VisitParameter( parameter );
    }

    public void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node)
    {
        foreach ( var selection in node.Selection.Span )
            this.Visit( selection );

        this.Visit( node.DataSource );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public void VisitCompoundQuery(SqlCompoundQueryExpressionNode node)
    {
        foreach ( var selection in node.Selection.Span )
            this.Visit( selection );

        this.Visit( node.FirstQuery );
        foreach ( var component in node.FollowingQueries.Span )
            VisitCompoundQueryComponent( component );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public void VisitCompoundQueryComponent(SqlCompoundQueryComponentNode node)
    {
        this.Visit( node.Query );
    }

    public void VisitDistinctTrait(SqlDistinctTraitNode node) { }

    public void VisitFilterTrait(SqlFilterTraitNode node)
    {
        this.Visit( node.Filter );
    }

    public void VisitAggregationTrait(SqlAggregationTraitNode node)
    {
        foreach ( var expression in node.Expressions.Span )
            this.Visit( expression );
    }

    public void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node)
    {
        this.Visit( node.Filter );
    }

    public void VisitSortTrait(SqlSortTraitNode node)
    {
        foreach ( var orderBy in node.Ordering.Span )
            VisitOrderBy( orderBy );
    }

    public void VisitLimitTrait(SqlLimitTraitNode node)
    {
        this.Visit( node.Value );
    }

    public void VisitOffsetTrait(SqlOffsetTraitNode node)
    {
        this.Visit( node.Value );
    }

    public void VisitCommonTableExpressionTrait(SqlCommonTableExpressionTraitNode node)
    {
        foreach ( var cte in node.CommonTableExpressions.Span )
            VisitCommonTableExpression( cte );
    }

    public void VisitOrderBy(SqlOrderByNode node)
    {
        this.Visit( node.Expression );
    }

    public void VisitCommonTableExpression(SqlCommonTableExpressionNode node)
    {
        this.Visit( node.Query );
    }

    public void VisitTypeCast(SqlTypeCastExpressionNode node)
    {
        this.Visit( node.Value );
    }

    public void VisitValues(SqlValuesNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitInsertInto(SqlInsertIntoNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitUpdate(SqlUpdateNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitValueAssignment(SqlValueAssignmentNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitDeleteFrom(SqlDeleteFromNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitCreateTemporaryTable(SqlCreateTemporaryTableNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitDropTemporaryTable(SqlDropTemporaryTableNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitStatementBatch(SqlStatementBatchNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitBeginTransaction(SqlBeginTransactionNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitCommitTransaction(SqlCommitTransactionNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitRollbackTransaction(SqlRollbackTransactionNode node)
    {
        AddForbiddenNode( node );
    }

    public void VisitCustom(SqlNodeBase node) { }

    [Pure]
    internal Chain<string> GetErrors()
    {
        var errors = Chain<string>.Empty;
        if ( _forbiddenNodes is null || _forbiddenNodes.Count == 0 )
            return errors;

        foreach ( var node in _forbiddenNodes )
        {
            switch ( node.NodeType )
            {
                case SqlNodeType.ColumnBuilder:
                {
                    var builder = ReinterpretCast.To<SqlColumnBuilderNode>( node );
                    errors = errors.Extend(
                        ReferenceEquals( builder.Value.Database, Database )
                            ? ExceptionResources.ColumnIsArchived( builder )
                            : ExceptionResources.ColumnBelongsToAnotherDatabase( builder ) );

                    break;
                }
                case SqlNodeType.TableBuilderRecordSet:
                {
                    var builder = ReinterpretCast.To<SqlTableBuilderRecordSetNode>( node );
                    errors = errors.Extend(
                        ReferenceEquals( builder.Table.Database, Database )
                            ? ExceptionResources.TableIsArchived( builder )
                            : ExceptionResources.TableBelongsToAnotherDatabase( builder ) );

                    break;
                }
                case SqlNodeType.ViewBuilderRecordSet:
                {
                    var builder = ReinterpretCast.To<SqlViewBuilderRecordSetNode>( node );
                    errors = errors.Extend(
                        ReferenceEquals( builder.View.Database, Database )
                            ? ExceptionResources.ViewIsArchived( builder )
                            : ExceptionResources.ViewBelongsToAnotherDatabase( builder ) );

                    break;
                }
                default:
                    errors = errors.Extend( ExceptionResources.UnexpectedNode( node ) );
                    break;
            }
        }

        return errors;
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

    private void AddForbiddenNode(SqlNodeBase node)
    {
        if ( _forbiddenNodes is null )
        {
            _forbiddenNodes = new List<SqlNodeBase> { node };
            return;
        }

        if ( ! _forbiddenNodes.Contains( node ) )
            _forbiddenNodes.Add( node );
    }
}
