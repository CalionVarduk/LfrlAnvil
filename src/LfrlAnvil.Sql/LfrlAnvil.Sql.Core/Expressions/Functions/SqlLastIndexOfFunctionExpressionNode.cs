namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns the position of the last occurrence
/// of a value in a string.
/// </summary>
public sealed class SqlLastIndexOfFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlLastIndexOfFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode value)
        : base( SqlFunctionType.LastIndexOf, new[] { argument, value } ) { }
}
