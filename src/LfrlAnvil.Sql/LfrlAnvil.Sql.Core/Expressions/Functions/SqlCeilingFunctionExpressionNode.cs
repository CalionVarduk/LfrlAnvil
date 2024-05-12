namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns its parameter's ceiling.
/// </summary>
public sealed class SqlCeilingFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCeilingFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Ceiling, new[] { argument } ) { }
}
