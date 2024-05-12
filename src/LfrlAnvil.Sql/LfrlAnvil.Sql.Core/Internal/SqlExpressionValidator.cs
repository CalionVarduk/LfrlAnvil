using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents an object capable of recursive traversal over an SQL syntax tree that is responsible for
/// checking the validity of SQL syntax trees.
/// </summary>
public abstract class SqlExpressionValidator : SqlNodeVisitor
{
    private List<SqlNodeBase>? _forbiddenNodes;

    /// <summary>
    /// Collection of <see cref="SqlNodeBase"/> instances forbidden by this validator.
    /// </summary>
    protected ReadOnlySpan<SqlNodeBase> ForbiddenNodes => CollectionsMarshal.AsSpan( _forbiddenNodes );

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitRawDataField(SqlRawDataFieldNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitParameter(SqlParameterNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitColumn(SqlColumnNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitViewDataField(SqlViewDataFieldNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitTable(SqlTableNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitView(SqlViewNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitNewTable(SqlNewTableNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitNewView(SqlNewViewNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitValues(SqlValuesNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitRawStatement(SqlRawStatementNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitInsertInto(SqlInsertIntoNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitUpdate(SqlUpdateNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitUpsert(SqlUpsertNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitValueAssignment(SqlValueAssignmentNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitDeleteFrom(SqlDeleteFromNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitTruncate(SqlTruncateNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitForeignKeyDefinition(SqlForeignKeyDefinitionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitCheckDefinition(SqlCheckDefinitionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitCreateTable(SqlCreateTableNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitCreateView(SqlCreateViewNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitCreateIndex(SqlCreateIndexNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitRenameTable(SqlRenameTableNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitRenameColumn(SqlRenameColumnNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitAddColumn(SqlAddColumnNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitDropColumn(SqlDropColumnNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitDropTable(SqlDropTableNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitDropView(SqlDropViewNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitDropIndex(SqlDropIndexNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitStatementBatch(SqlStatementBatchNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitBeginTransaction(SqlBeginTransactionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitCommitTransaction(SqlCommitTransactionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <inheritdoc />
    /// <remarks>Node is added to the <see cref="ForbiddenNodes"/> collection.</remarks>
    public override void VisitRollbackTransaction(SqlRollbackTransactionNode node)
    {
        AddForbiddenNode( node );
    }

    /// <summary>
    /// Adds the provided <paramref name="node"/> to the <see cref="ForbiddenNodes"/> collection, unless it already exists.
    /// </summary>
    /// <param name="node">Node to add.</param>
    protected void AddForbiddenNode(SqlNodeBase node)
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
