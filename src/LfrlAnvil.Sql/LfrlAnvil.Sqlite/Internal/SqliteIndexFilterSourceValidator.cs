using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteIndexFilterSourceValidator : SqliteSourceNodeValidator
{
    internal SqliteIndexFilterSourceValidator(SqliteTableBuilder table)
    {
        Table = table;
        ReferencedColumns = new Dictionary<ulong, SqliteColumnBuilder>();
    }

    internal SqliteTableBuilder Table { get; }
    internal Dictionary<ulong, SqliteColumnBuilder> ReferencedColumns { get; }

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

    public override void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node)
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

    public override void VisitCustomAggregateFunction(SqlAggregateFunctionExpressionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitRawDataField(SqlRawDataFieldNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        if ( ! ReferenceEquals( node.Value.Table, Table ) )
        {
            AddForbiddenNode( node );
            return;
        }

        var column = ReinterpretCast.To<SqliteColumnBuilder>( node.Value );
        if ( column.IsRemoved )
            AddForbiddenNode( node );
        else
            ReferencedColumns.TryAdd( column.Id, column );

        this.Visit( node.RecordSet );
    }

    public override void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitTableBuilderRecordSet(SqlTableBuilderRecordSetNode node)
    {
        if ( ! ReferenceEquals( node.Table, Table ) )
            AddForbiddenNode( node );
    }

    [Pure]
    internal Chain<string> GetErrors()
    {
        var errors = Chain<string>.Empty;
        if ( ForbiddenNodes is null || ForbiddenNodes.Count == 0 )
            return errors;

        foreach ( var node in ForbiddenNodes )
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
}
