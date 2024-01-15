using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.MySql.Internal;

internal abstract class MySqlExpressionValidator : SqlNodeVisitor
{
    private List<SqlNodeBase>? _forbiddenNodes;

    protected ReadOnlySpan<SqlNodeBase> ForbiddenNodes => CollectionsMarshal.AsSpan( _forbiddenNodes );

    public override void VisitRawDataField(SqlRawDataFieldNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitParameter(SqlParameterNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitColumn(SqlColumnNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitColumnBuilder(SqlColumnBuilderNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitQueryDataField(SqlQueryDataFieldNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitViewDataField(SqlViewDataFieldNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitTable(SqlTableNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitView(SqlViewNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitNewTable(SqlNewTableNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitNewView(SqlNewViewNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitValues(SqlValuesNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitRawStatement(SqlRawStatementNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitInsertInto(SqlInsertIntoNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitUpdate(SqlUpdateNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitValueAssignment(SqlValueAssignmentNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitDeleteFrom(SqlDeleteFromNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitTruncate(SqlTruncateNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitPrimaryKeyDefinition(SqlPrimaryKeyDefinitionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitForeignKeyDefinition(SqlForeignKeyDefinitionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCheckDefinition(SqlCheckDefinitionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCreateTable(SqlCreateTableNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCreateView(SqlCreateViewNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCreateIndex(SqlCreateIndexNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitRenameTable(SqlRenameTableNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitRenameColumn(SqlRenameColumnNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitAddColumn(SqlAddColumnNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitDropColumn(SqlDropColumnNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitDropTable(SqlDropTableNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitDropView(SqlDropViewNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitDropIndex(SqlDropIndexNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitStatementBatch(SqlStatementBatchNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitBeginTransaction(SqlBeginTransactionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCommitTransaction(SqlCommitTransactionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitRollbackTransaction(SqlRollbackTransactionNode node)
    {
        AddForbiddenNode( node );
    }

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
