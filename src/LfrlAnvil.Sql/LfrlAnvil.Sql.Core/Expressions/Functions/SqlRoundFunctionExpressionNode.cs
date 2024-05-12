namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns the value
/// rounded to the specified decimal precision.
/// </summary>
public sealed class SqlRoundFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlRoundFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode precision)
        : base( SqlFunctionType.Round, new[] { argument, precision } ) { }
}
