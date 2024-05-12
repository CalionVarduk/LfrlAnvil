namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns its parameter's floor.
/// </summary>
public sealed class SqlFloorFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlFloorFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Floor, new[] { argument } ) { }
}
