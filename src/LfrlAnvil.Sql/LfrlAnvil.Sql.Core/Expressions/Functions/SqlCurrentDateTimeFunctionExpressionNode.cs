using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlCurrentDateTimeFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCurrentDateTimeFunctionExpressionNode()
        : base( SqlFunctionType.CurrentDateTime, Array.Empty<SqlExpressionNode>() ) { }
}
