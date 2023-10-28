﻿using System;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

public sealed class SqlUpdateNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlUpdateNode(SqlDataSourceNode dataSource, ReadOnlyMemory<SqlValueAssignmentNode> assignments)
        : base( SqlNodeType.Update )
    {
        DataSource = dataSource;
        Assignments = assignments;
    }

    public SqlDataSourceNode DataSource { get; }
    public ReadOnlyMemory<SqlValueAssignmentNode> Assignments { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
