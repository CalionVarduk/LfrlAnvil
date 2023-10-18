﻿using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlCurrentTimestampFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCurrentTimestampFunctionExpressionNode()
        : base( SqlFunctionType.CurrentTimestamp, Array.Empty<SqlExpressionNode>() ) { }
}