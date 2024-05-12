namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that null-coalesces its parameters.
/// </summary>
public sealed class SqlCoalesceFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCoalesceFunctionExpressionNode(SqlExpressionNode[] arguments)
        : base( SqlFunctionType.Coalesce, arguments )
    {
        Ensure.IsNotEmpty( arguments );
    }
}
