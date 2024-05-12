namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns its parameter with all of its
/// elements converted to their lowercase version.
/// </summary>
public sealed class SqlToLowerFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlToLowerFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.ToLower, new[] { argument } ) { }
}
