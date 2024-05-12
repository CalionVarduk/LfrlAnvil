namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns a substring
/// from the provided string.
/// </summary>
public sealed class SqlSubstringFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlSubstringFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode startIndex, SqlExpressionNode? length)
        : base( SqlFunctionType.Substring, length is null ? new[] { argument, startIndex } : new[] { argument, startIndex, length } ) { }
}
