using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlCurrentTimeFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCurrentTimeFunctionExpressionNode()
        : base( SqlFunctionType.CurrentTime, Array.Empty<SqlExpressionNode>() ) { }
}
