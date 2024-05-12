namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns a numeric sign representation
/// of its parameter.
/// </summary>
public sealed class SqlSignFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlSignFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Sign, new[] { argument } ) { }
}
