using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

public class SqlSchemaScopeExpressionValidator : SqlExpressionValidator
{
    private readonly Dictionary<ulong, SqlObjectBuilder> _referencedObjects;

    protected internal SqlSchemaScopeExpressionValidator(SqlSchemaBuilder schema)
    {
        Schema = schema;
        _referencedObjects = new Dictionary<ulong, SqlObjectBuilder>();
    }

    public SqlSchemaBuilder Schema { get; }

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
        if ( ! ReferenceEquals( node.Value.Database, Schema.Database ) )
        {
            AddForbiddenNode( node );
            return;
        }

        var column = ReinterpretCast.To<SqlColumnBuilder>( node.Value );
        if ( column.IsRemoved )
            AddForbiddenNode( node );
        else
            AddReferencedObject( column.Table.Schema, column );

        VisitDataFieldRecordSet( node.RecordSet );
    }

    public override void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        VisitDataFieldRecordSet( node.RecordSet );
    }

    public override void VisitTableBuilder(SqlTableBuilderNode node)
    {
        if ( ! ReferenceEquals( node.Table.Database, Schema.Database ) )
        {
            AddForbiddenNode( node );
            return;
        }

        var table = ReinterpretCast.To<SqlTableBuilder>( node.Table );
        if ( table.IsRemoved )
            AddForbiddenNode( node );
        else
            AddReferencedObject( table.Schema, table );
    }

    public override void VisitViewBuilder(SqlViewBuilderNode node)
    {
        if ( ! ReferenceEquals( node.View.Database, Schema.Database ) )
        {
            AddForbiddenNode( node );
            return;
        }

        var view = ReinterpretCast.To<SqlViewBuilder>( node.View );
        if ( view.IsRemoved )
            AddForbiddenNode( node );
        else
            AddReferencedObject( view.Schema, view );
    }

    [Pure]
    public virtual Chain<string> GetErrors()
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
                        ReferenceEquals( builder.Value.Database, Schema.Database )
                            ? ExceptionResources.ColumnIsArchived( builder )
                            : ExceptionResources.ColumnBelongsToAnotherDatabase( builder ) );

                    break;
                }
                case SqlNodeType.TableBuilder:
                {
                    var builder = ReinterpretCast.To<SqlTableBuilderNode>( node );
                    errors = errors.Extend(
                        ReferenceEquals( builder.Table.Database, Schema.Database )
                            ? ExceptionResources.TableIsArchived( builder )
                            : ExceptionResources.TableBelongsToAnotherDatabase( builder ) );

                    break;
                }
                case SqlNodeType.ViewBuilder:
                {
                    var builder = ReinterpretCast.To<SqlViewBuilderNode>( node );
                    errors = errors.Extend(
                        ReferenceEquals( builder.View.Database, Schema.Database )
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

    [Pure]
    public SqlObjectBuilder[] GetReferencedObjects()
    {
        return _referencedObjects.Values.ToArray();
    }

    protected void AddReferencedObject(SqlSchemaBuilder schema, SqlObjectBuilder obj)
    {
        if ( ! ReferenceEquals( Schema, schema ) )
            _referencedObjects.TryAdd( schema.Id, schema );

        _referencedObjects.TryAdd( obj.Id, obj );
    }
}
