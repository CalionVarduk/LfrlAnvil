namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlReplaceFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlReplaceFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode oldValue, SqlExpressionNode newValue)
        : base( SqlFunctionType.Replace, new[] { argument, oldValue, newValue } )
    {
        Type = argument.Type;
    }

    public override SqlExpressionType? Type { get; }
}
