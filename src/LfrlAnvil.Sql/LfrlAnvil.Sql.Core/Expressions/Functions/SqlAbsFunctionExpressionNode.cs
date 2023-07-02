namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlAbsFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlAbsFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Abs, new[] { argument } )
    {
        Type = argument.Type;
    }

    public override SqlExpressionType? Type { get; }
}
