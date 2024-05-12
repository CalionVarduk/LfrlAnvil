namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns the position of the first occurrence
/// of a value in a string.
/// </summary>
public sealed class SqlIndexOfFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlIndexOfFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode value)
        : base( SqlFunctionType.IndexOf, new[] { argument, value } ) { }
}
