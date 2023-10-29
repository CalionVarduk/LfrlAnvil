namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlMaxFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlMaxFunctionExpressionNode(SqlExpressionNode[] arguments)
        : base( SqlFunctionType.Max, arguments )
    {
        Ensure.IsNotEmpty( arguments );
    }
}
