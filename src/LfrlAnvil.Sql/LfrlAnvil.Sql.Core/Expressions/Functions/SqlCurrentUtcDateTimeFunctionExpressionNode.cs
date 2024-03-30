using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlCurrentUtcDateTimeFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCurrentUtcDateTimeFunctionExpressionNode()
        : base( SqlFunctionType.CurrentUtcDateTime, Array.Empty<SqlExpressionNode>() ) { }
}
