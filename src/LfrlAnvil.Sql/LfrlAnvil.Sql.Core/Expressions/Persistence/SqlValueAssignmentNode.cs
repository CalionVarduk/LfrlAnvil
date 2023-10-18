﻿using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

public sealed class SqlValueAssignmentNode : SqlNodeBase
{
    internal SqlValueAssignmentNode(SqlDataFieldNode dataField, SqlExpressionNode value)
        : base( SqlNodeType.ValueAssignment )
    {
        DataField = dataField;
        Value = value;
    }

    public SqlDataFieldNode DataField { get; }
    public SqlExpressionNode Value { get; }
}