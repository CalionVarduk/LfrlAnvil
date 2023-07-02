namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlPowerFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlPowerFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode power)
        : base( SqlFunctionType.Power, new[] { argument, power } )
    {
        Type = argument.Type;
    }

    public override SqlExpressionType? Type { get; }
}
