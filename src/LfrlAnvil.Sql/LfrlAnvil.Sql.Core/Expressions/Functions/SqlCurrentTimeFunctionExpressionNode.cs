using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlCurrentTimeFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCurrentTimeFunctionExpressionNode()
        : base( SqlFunctionType.CurrentTime, Array.Empty<SqlExpressionNode>() )
    {
        Type = SqlExpressionType.Create<TimeOnly>();
    }

    public override SqlExpressionType? Type { get; }
}
