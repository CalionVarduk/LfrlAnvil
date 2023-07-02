using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlRecordsAffectedFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlRecordsAffectedFunctionExpressionNode()
        : base( SqlFunctionType.RecordsAffected, Array.Empty<SqlExpressionNode>() )
    {
        Type = SqlExpressionType.Create<long>();
    }

    public override SqlExpressionType? Type { get; }
}
