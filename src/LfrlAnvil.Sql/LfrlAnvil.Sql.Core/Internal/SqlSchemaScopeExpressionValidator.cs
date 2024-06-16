// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents an object capable of recursive traversal over an SQL syntax tree that is responsible for
/// checking the validity of SQL syntax trees in the context of a whole database e.g. view source.
/// This validator also tracks all objects referenced by a syntax tree.
/// </summary>
public class SqlSchemaScopeExpressionValidator : SqlExpressionValidator
{
    private readonly Dictionary<ulong, SqlObjectBuilder> _referencedObjects;

    /// <summary>
    /// Creates a new <see cref="SqlSchemaScopeExpressionValidator"/> instance.
    /// </summary>
    /// <param name="schema"><see cref="SqlSchemaBuilder"/> instance that defines available objects.</param>
    protected internal SqlSchemaScopeExpressionValidator(SqlSchemaBuilder schema)
    {
        Schema = schema;
        _referencedObjects = new Dictionary<ulong, SqlObjectBuilder>();
    }

    /// <summary>
    /// <see cref="SqlSchemaBuilder"/> instance that defines available objects.
    /// </summary>
    public SqlSchemaBuilder Schema { get; }

    /// <summary>
    /// Collection of all referenced objects.
    /// </summary>
    protected Dictionary<ulong, SqlObjectBuilder>.ValueCollection ReferencedObjects => _referencedObjects.Values;

    /// <summary>
    /// Visits an <see cref="SqlRecordSetNode"/> of an <see cref="SqlDataFieldNode"/>.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    public void VisitDataFieldRecordSet(SqlRecordSetNode node)
    {
        if ( node.NodeType != SqlNodeType.QueryRecordSet && node.NodeType != SqlNodeType.NamedFunctionRecordSet )
            this.Visit( node );
    }

    /// <inheritdoc cref="SqlNodeVisitor.VisitRawDataField(SqlRawDataFieldNode)" />
    public override void VisitRawDataField(SqlRawDataFieldNode node)
    {
        VisitDataFieldRecordSet( node.RecordSet );
    }

    /// <inheritdoc cref="SqlNodeVisitor.VisitColumnBuilder(SqlColumnBuilderNode)" />
    /// <remarks>
    /// Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection only when
    /// it does not belong to the <see cref="SqlObjectBuilder.Database"/> of the <see cref="Schema"/>.
    /// </remarks>
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

    /// <inheritdoc cref="SqlNodeVisitor.VisitQueryDataField(SqlQueryDataFieldNode)" />
    public override void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        VisitDataFieldRecordSet( node.RecordSet );
    }

    /// <inheritdoc cref="SqlNodeVisitor.VisitTableBuilder(SqlTableBuilderNode)" />
    /// <remarks>
    /// Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection only when
    /// it does not belong to the <see cref="SqlObjectBuilder.Database"/> of the <see cref="Schema"/>.
    /// </remarks>
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

    /// <inheritdoc cref="SqlNodeVisitor.VisitViewBuilder(SqlViewBuilderNode)" />
    /// <remarks>
    /// Node is added to the <see cref="SqlExpressionValidator.ForbiddenNodes"/> collection only when
    /// it does not belong to the <see cref="SqlObjectBuilder.Database"/> of the <see cref="Schema"/>.
    /// </remarks>
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

    /// <summary>
    /// Returns a collection of all accumulated errors.
    /// </summary>
    /// <returns>Collection of all accumulated errors.</returns>
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

    /// <summary>
    /// Creates a new array from <see cref="ReferencedObjects"/>.
    /// </summary>
    /// <returns>New array from <see cref="ReferencedObjects"/>.</returns>
    [Pure]
    public SqlObjectBuilder[] GetReferencedObjects()
    {
        return ReferencedObjects.ToArray();
    }

    /// <summary>
    /// Adds the provided <paramref name="obj"/>, and its <paramref name="schema"/>, to <see cref="ReferencedObjects"/>.
    /// </summary>
    /// <param name="schema">Schema of the object to add.</param>
    /// <param name="obj">Object to add.</param>
    /// <remarks>
    /// Provided <paramref name="schema"/> will only be added to <see cref="ReferencedObjects"/> when it is not <see cref="Schema"/>.
    /// </remarks>
    protected void AddReferencedObject(SqlSchemaBuilder schema, SqlObjectBuilder obj)
    {
        if ( ! ReferenceEquals( Schema, schema ) )
            _referencedObjects.TryAdd( schema.Id, schema );

        _referencedObjects.TryAdd( obj.Id, obj );
    }
}
