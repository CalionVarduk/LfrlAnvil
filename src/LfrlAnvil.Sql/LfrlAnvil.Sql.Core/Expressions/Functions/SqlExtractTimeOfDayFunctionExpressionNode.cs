namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that extracts the time of day part
/// from its parameter.
/// </summary>
public sealed class SqlExtractTimeOfDayFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlExtractTimeOfDayFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.ExtractTimeOfDay, new[] { argument } ) { }
}
