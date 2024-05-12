namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns its parameter with all of its
/// elements converted to their uppercase version.
/// </summary>
public sealed class SqlToUpperFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlToUpperFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.ToUpper, new[] { argument } ) { }
}
