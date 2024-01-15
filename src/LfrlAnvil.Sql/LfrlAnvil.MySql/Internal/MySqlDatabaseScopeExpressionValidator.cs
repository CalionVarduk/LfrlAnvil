using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.MySql.Internal;

internal sealed class MySqlDatabaseScopeExpressionValidator : MySqlExpressionValidator
{
    internal MySqlDatabaseScopeExpressionValidator(MySqlDatabaseBuilder database)
    {
        Database = database;
        ReferencedObjects = new Dictionary<ulong, MySqlObjectBuilder>();
    }

    internal MySqlDatabaseBuilder Database { get; }
    internal Dictionary<ulong, MySqlObjectBuilder> ReferencedObjects { get; }

    public void VisitDataFieldRecordSet(SqlRecordSetNode node)
    {
        if ( node.NodeType != SqlNodeType.QueryRecordSet && node.NodeType != SqlNodeType.NamedFunctionRecordSet )
            this.Visit( node );
    }

    public override void VisitRawDataField(SqlRawDataFieldNode node)
    {
        VisitDataFieldRecordSet( node.RecordSet );
    }

    public override void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        if ( ! ReferenceEquals( node.Value.Database, Database ) )
        {
            AddForbiddenNode( node );
            return;
        }

        var column = ReinterpretCast.To<MySqlColumnBuilder>( node.Value );
        if ( column.IsRemoved )
            AddForbiddenNode( node );
        else
            ReferencedObjects.TryAdd( column.Id, column );

        VisitDataFieldRecordSet( node.RecordSet );
    }

    public override void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        VisitDataFieldRecordSet( node.RecordSet );
    }

    public override void VisitTableBuilder(SqlTableBuilderNode node)
    {
        if ( ! ReferenceEquals( node.Table.Database, Database ) )
        {
            AddForbiddenNode( node );
            return;
        }

        var table = ReinterpretCast.To<MySqlTableBuilder>( node.Table );
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

        var view = ReinterpretCast.To<MySqlViewBuilder>( node.View );
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
