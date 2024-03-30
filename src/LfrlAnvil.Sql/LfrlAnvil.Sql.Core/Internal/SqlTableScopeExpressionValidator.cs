using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

public class SqlTableScopeExpressionValidator : SqlExpressionValidator
{
    private readonly Dictionary<ulong, SqlColumnBuilder> _referencedColumns;

    protected internal SqlTableScopeExpressionValidator(SqlTableBuilder table)
    {
        Table = table;
        _referencedColumns = new Dictionary<ulong, SqlColumnBuilder>();
    }

    public SqlTableBuilder Table { get; }
    protected Dictionary<ulong, SqlColumnBuilder>.ValueCollection ReferencedColumns => _referencedColumns.Values;

    public override void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        if ( ! ReferenceEquals( node.Value.Table, Table ) )
        {
            AddForbiddenNode( node );
            return;
        }

        var column = ReinterpretCast.To<SqlColumnBuilder>( node.Value );
        if ( column.IsRemoved )
            AddForbiddenNode( node );
        else
            AddReferencedColumn( column );

        this.Visit( node.RecordSet );
    }

    public override void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitRecordsAffectedFunction(SqlRecordsAffectedFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCurrentUtcDateTimeFunction(SqlCurrentUtcDateTimeFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitNamedAggregateFunction(SqlNamedAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitMinAggregateFunction(SqlMinAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitMaxAggregateFunction(SqlMaxAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitAverageAggregateFunction(SqlAverageAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitSumAggregateFunction(SqlSumAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCountAggregateFunction(SqlCountAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitStringConcatAggregateFunction(SqlStringConcatAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitRowNumberWindowFunction(SqlRowNumberWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitRankWindowFunction(SqlRankWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitDenseRankWindowFunction(SqlDenseRankWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCumulativeDistributionWindowFunction(SqlCumulativeDistributionWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitNTileWindowFunction(SqlNTileWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitLagWindowFunction(SqlLagWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitLeadWindowFunction(SqlLeadWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitFirstValueWindowFunction(SqlFirstValueWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitLastValueWindowFunction(SqlLastValueWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitNthValueWindowFunction(SqlNthValueWindowFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitRawRecordSet(SqlRawRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitNamedFunctionRecordSet(SqlNamedFunctionRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitTableBuilder(SqlTableBuilderNode node)
    {
        if ( ! ReferenceEquals( node.Table, Table ) )
            AddForbiddenNode( node );
    }

    public override void VisitViewBuilder(SqlViewBuilderNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitQueryRecordSet(SqlQueryRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCommonTableExpressionRecordSet(SqlCommonTableExpressionRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitJoinOn(SqlDataSourceJoinOnNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitDataSource(SqlDataSourceNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitSelectField(SqlSelectFieldNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitSelectCompoundField(SqlSelectCompoundFieldNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitSelectRecordSet(SqlSelectRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitSelectAll(SqlSelectAllNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitRawQuery(SqlRawQueryExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCompoundQuery(SqlCompoundQueryExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCompoundQueryComponent(SqlCompoundQueryComponentNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitDistinctTrait(SqlDistinctTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitFilterTrait(SqlFilterTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitAggregationTrait(SqlAggregationTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitSortTrait(SqlSortTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitLimitTrait(SqlLimitTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitOffsetTrait(SqlOffsetTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCommonTableExpressionTrait(SqlCommonTableExpressionTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitWindowDefinitionTrait(SqlWindowDefinitionTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitWindowTrait(SqlWindowTraitNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitOrderBy(SqlOrderByNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCommonTableExpression(SqlCommonTableExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitWindowDefinition(SqlWindowDefinitionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitWindowFrame(SqlWindowFrameNode node)
    {
        AddForbiddenNode( node );
    }

    [Pure]
    public virtual Chain<string> GetErrors()
    {
        var errors = Chain<string>.Empty;
        var forbiddenNode = ForbiddenNodes;
        if ( forbiddenNode.Length == 0 )
            return errors;

        foreach ( var node in forbiddenNode )
        {
            switch ( node.NodeType )
            {
                case SqlNodeType.ColumnBuilder:
                {
                    var builder = ReinterpretCast.To<SqlColumnBuilderNode>( node );
                    errors = errors.Extend(
                        ReferenceEquals( builder.Value.Table, Table )
                            ? ExceptionResources.ColumnIsArchived( builder )
                            : ExceptionResources.ColumnBelongsToAnotherTable( builder ) );

                    break;
                }
                default:
                    errors = errors.Extend( ExceptionResources.UnexpectedNode( node ) );
                    break;
            }
        }

        return errors;
    }

    [Pure]
    public SqlColumnBuilder[] GetReferencedColumns()
    {
        return ReferencedColumns.ToArray();
    }

    protected void AddReferencedColumn(SqlColumnBuilder column)
    {
        _referencedColumns.TryAdd( column.Id, column );
    }
}
