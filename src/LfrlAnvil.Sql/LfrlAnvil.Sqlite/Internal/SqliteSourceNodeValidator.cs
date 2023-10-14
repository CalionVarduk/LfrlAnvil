﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sqlite.Internal;

internal abstract class SqliteSourceNodeValidator : SqlNodeVisitor
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

    public override void VisitTableRecordSet(SqlTableRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitViewRecordSet(SqlViewRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitTemporaryTableRecordSet(SqlTemporaryTableRecordSetNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitValues(SqlValuesNode node)
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

    public override void VisitColumnDefinition(SqlColumnDefinitionNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitCreateTemporaryTable(SqlCreateTemporaryTableNode node)
    {
        AddForbiddenNode( node );
    }

    public override void VisitDropTemporaryTable(SqlDropTemporaryTableNode node)
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
