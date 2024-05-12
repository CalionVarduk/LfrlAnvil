namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns the maximum value
/// from the provided range of parameters.
/// </summary>
public sealed class SqlMaxFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlMaxFunctionExpressionNode(SqlExpressionNode[] arguments)
        : base( SqlFunctionType.Max, arguments )
    {
        Ensure.IsNotEmpty( arguments );
    }
}
