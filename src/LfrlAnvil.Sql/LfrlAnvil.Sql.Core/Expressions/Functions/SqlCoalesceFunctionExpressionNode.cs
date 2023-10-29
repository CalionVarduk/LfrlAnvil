namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlCoalesceFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCoalesceFunctionExpressionNode(SqlExpressionNode[] arguments)
        : base( SqlFunctionType.Coalesce, arguments )
    {
        Ensure.IsNotEmpty( arguments );
    }
}
