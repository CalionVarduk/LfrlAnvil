namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlMinFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlMinFunctionExpressionNode(SqlExpressionNode[] arguments)
        : base( SqlFunctionType.Min, arguments )
    {
        Ensure.IsNotEmpty( arguments );
    }
}
