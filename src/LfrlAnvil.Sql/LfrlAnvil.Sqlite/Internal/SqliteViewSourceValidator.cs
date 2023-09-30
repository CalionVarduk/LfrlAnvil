using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteViewSourceValidator : SqliteSourceNodeValidator
{
    internal SqliteViewSourceValidator(SqliteDatabaseBuilder database)
    {
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

    public override void VisitRawDataField(SqlRawDataFieldNode node)
    {
        VisitNonQueryRecordSet( node.RecordSet );
    }

    public override void VisitColumnBuilder(SqlColumnBuilderNode node)
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

    public override void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        VisitNonQueryRecordSet( node.RecordSet );
    }

    public override void VisitRawRecordSet(SqlRawRecordSetNode node) { }

    public override void VisitTableBuilderRecordSet(SqlTableBuilderRecordSetNode node)
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

    public override void VisitViewBuilderRecordSet(SqlViewBuilderRecordSetNode node)
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

    public override void VisitQueryRecordSet(SqlQueryRecordSetNode node)
    {
        this.Visit( node.Query );
    }

    public override void VisitCommonTableExpressionRecordSet(SqlCommonTableExpressionRecordSetNode node) { }

    public override void VisitTemporaryTableRecordSet(SqlTemporaryTableRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitJoinOn(SqlDataSourceJoinOnNode node)
    {
        this.Visit( node.InnerRecordSet );
        this.Visit( node.OnExpression );
    }

    public override void VisitDataSource(SqlDataSourceNode node)
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

    public override void VisitSelectField(SqlSelectFieldNode node)
    {
        this.Visit( node.Expression );
    }

    public override void VisitSelectCompoundField(SqlSelectCompoundFieldNode node) { }

    public override void VisitSelectRecordSet(SqlSelectRecordSetNode node)
    {
        VisitNonQueryRecordSet( node.RecordSet );
    }

    public override void VisitSelectAll(SqlSelectAllNode node) { }

    public override void VisitRawQuery(SqlRawQueryExpressionNode node)
    {
        foreach ( var parameter in node.Parameters.Span )
            VisitParameter( parameter );
    }

    public override void VisitDataSourceQuery(SqlDataSourceQueryExpressionNode node)
    {
        foreach ( var selection in node.Selection.Span )
            this.Visit( selection );

        this.Visit( node.DataSource );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public override void VisitCompoundQuery(SqlCompoundQueryExpressionNode node)
    {
        foreach ( var selection in node.Selection.Span )
            this.Visit( selection );

        this.Visit( node.FirstQuery );
        foreach ( var component in node.FollowingQueries.Span )
            VisitCompoundQueryComponent( component );

        foreach ( var trait in node.Traits )
            this.Visit( trait );
    }

    public override void VisitDistinctTrait(SqlDistinctTraitNode node) { }

    public override void VisitFilterTrait(SqlFilterTraitNode node)
    {
        this.Visit( node.Filter );
    }

    public override void VisitAggregationTrait(SqlAggregationTraitNode node)
    {
        foreach ( var expression in node.Expressions.Span )
            this.Visit( expression );
    }

    public override void VisitAggregationFilterTrait(SqlAggregationFilterTraitNode node)
    {
        this.Visit( node.Filter );
    }

    public override void VisitSortTrait(SqlSortTraitNode node)
    {
        foreach ( var orderBy in node.Ordering.Span )
            VisitOrderBy( orderBy );
    }

    public override void VisitLimitTrait(SqlLimitTraitNode node)
    {
        this.Visit( node.Value );
    }

    public override void VisitOffsetTrait(SqlOffsetTraitNode node)
    {
        this.Visit( node.Value );
    }

    public override void VisitCommonTableExpressionTrait(SqlCommonTableExpressionTraitNode node)
    {
        foreach ( var cte in node.CommonTableExpressions.Span )
            VisitCommonTableExpression( cte );
    }

    public override void VisitOrderBy(SqlOrderByNode node)
    {
        this.Visit( node.Expression );
    }

    public override void VisitCommonTableExpression(SqlCommonTableExpressionNode node)
    {
        this.Visit( node.Query );
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
}
