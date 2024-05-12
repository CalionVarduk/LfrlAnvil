namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns the minimum value
/// from the provided range of parameters.
/// </summary>
public sealed class SqlMinFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlMinFunctionExpressionNode(SqlExpressionNode[] arguments)
        : base( SqlFunctionType.Min, arguments )
    {
        Ensure.IsNotEmpty( arguments );
    }
}
