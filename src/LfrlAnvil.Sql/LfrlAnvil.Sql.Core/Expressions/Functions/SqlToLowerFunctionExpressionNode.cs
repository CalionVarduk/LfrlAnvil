namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlToLowerFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlToLowerFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.ToLower, new[] { argument } )
    {
        Type = argument.Type;
    }

    public override SqlExpressionType? Type { get; }
}
