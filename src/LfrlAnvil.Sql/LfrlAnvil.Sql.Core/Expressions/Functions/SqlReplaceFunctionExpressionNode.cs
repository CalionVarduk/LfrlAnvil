namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns the string value
/// with all occurrences of the specified substring replaced with another specified substring.
/// </summary>
public sealed class SqlReplaceFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlReplaceFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode oldValue, SqlExpressionNode newValue)
        : base( SqlFunctionType.Replace, new[] { argument, oldValue, newValue } ) { }
}
