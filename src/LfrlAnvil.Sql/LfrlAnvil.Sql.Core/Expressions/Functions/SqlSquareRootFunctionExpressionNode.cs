namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns the value's square root.
/// </summary>
public sealed class SqlSquareRootFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlSquareRootFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.SquareRoot, new[] { argument } ) { }
}
