namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that calculates a difference
/// between two date and/or time parameters and converts the result to the given unit.
/// </summary>
public sealed class SqlTemporalDiffFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTemporalDiffFunctionExpressionNode(SqlExpressionNode start, SqlExpressionNode end, SqlTemporalUnit unit)
        : base( SqlFunctionType.TemporalDiff, new[] { start, end } )
    {
        Ensure.IsDefined( unit );
        Unit = unit;
    }

    /// <summary>
    /// <see cref="SqlTemporalUnit"/> that specifies the unit of the returned result.
    /// </summary>
    public SqlTemporalUnit Unit { get; }
}
