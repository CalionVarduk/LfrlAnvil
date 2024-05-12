namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns an absolute value.
/// </summary>
public sealed class SqlAbsFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlAbsFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Abs, new[] { argument } ) { }
}
