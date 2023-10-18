﻿using System;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlSwitchExpressionNode : SqlExpressionNode
{
    internal SqlSwitchExpressionNode(SqlSwitchCaseNode[] cases, SqlExpressionNode @default)
        : base( SqlNodeType.Switch )
    {
        Ensure.IsNotEmpty( cases, nameof( cases ) );
        Cases = cases;
        Default = @default;
    }

    public ReadOnlyMemory<SqlSwitchCaseNode> Cases { get; }
    public SqlExpressionNode Default { get; }
}