namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns a trimmed version of its parameter.
/// Parameter is trimmed on both ends.
/// </summary>
public sealed class SqlTrimFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTrimFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode? characters)
        : base( SqlFunctionType.Trim, characters is null ? new[] { argument } : new[] { argument, characters } ) { }
}
