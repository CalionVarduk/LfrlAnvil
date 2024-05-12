namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns a trimmed version of its parameter.
/// Parameter is trimmed only at the end.
/// </summary>
public sealed class SqlTrimEndFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTrimEndFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode? characters)
        : base( SqlFunctionType.TrimEnd, characters is null ? new[] { argument } : new[] { argument, characters } ) { }
}
