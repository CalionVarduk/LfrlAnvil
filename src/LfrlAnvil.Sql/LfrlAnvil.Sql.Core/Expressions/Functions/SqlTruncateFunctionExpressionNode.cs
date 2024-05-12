namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns a truncated value.
/// </summary>
public sealed class SqlTruncateFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTruncateFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode? precision)
        : base( SqlFunctionType.Truncate, precision is null ? new[] { argument } : new[] { argument, precision } ) { }
}
