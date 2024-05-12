namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns the value
/// raised to the desired power.
/// </summary>
public sealed class SqlPowerFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlPowerFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode power)
        : base( SqlFunctionType.Power, new[] { argument, power } ) { }
}
