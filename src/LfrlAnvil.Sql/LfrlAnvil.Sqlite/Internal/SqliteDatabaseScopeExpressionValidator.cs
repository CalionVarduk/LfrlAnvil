using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteDatabaseScopeExpressionValidator : SqliteExpressionValidator
{
    internal SqliteDatabaseScopeExpressionValidator(SqliteDatabaseBuilder database)
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

    public override void VisitTableBuilder(SqlTableBuilderNode node)
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

    public override void VisitViewBuilder(SqlViewBuilderNode node)
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

    [Pure]
    internal Chain<string> GetErrors()
    {
        var errors = Chain<string>.Empty;
        var forbiddenNodes = ForbiddenNodes;
        if ( forbiddenNodes.Length == 0 )
            return errors;

        foreach ( var node in forbiddenNodes )
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
                case SqlNodeType.TableBuilder:
                {
                    var builder = ReinterpretCast.To<SqlTableBuilderNode>( node );
                    errors = errors.Extend(
                        ReferenceEquals( builder.Table.Database, Database )
                            ? ExceptionResources.TableIsArchived( builder )
                            : ExceptionResources.TableBelongsToAnotherDatabase( builder ) );

                    break;
                }
                case SqlNodeType.ViewBuilder:
                {
                    var builder = ReinterpretCast.To<SqlViewBuilderNode>( node );
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
