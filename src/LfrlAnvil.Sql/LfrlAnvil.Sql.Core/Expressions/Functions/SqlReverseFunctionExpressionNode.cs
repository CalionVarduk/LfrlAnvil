namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns reversed string value.
/// </summary>
public sealed class SqlReverseFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlReverseFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Reverse, new[] { argument } ) { }
}
