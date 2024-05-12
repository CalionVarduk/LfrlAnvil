namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns its parameter's length.
/// </summary>
public sealed class SqlLengthFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlLengthFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Length, new[] { argument } ) { }
}
