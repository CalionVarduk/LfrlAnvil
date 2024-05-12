namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns a trimmed version of its parameter.
/// Parameter is trimmed only at the start.
/// </summary>
public sealed class SqlTrimStartFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTrimStartFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode? characters)
        : base( SqlFunctionType.TrimStart, characters is null ? new[] { argument } : new[] { argument, characters } ) { }
}
