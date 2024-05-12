namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that extracts a day component from its parameter.
/// </summary>
public sealed class SqlExtractDayFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlExtractDayFunctionExpressionNode(SqlExpressionNode argument, SqlTemporalUnit unit)
        : base( SqlFunctionType.ExtractDay, new[] { argument } )
    {
        Assume.True( unit is SqlTemporalUnit.Year or SqlTemporalUnit.Month or SqlTemporalUnit.Week );
        Unit = unit;
    }

    /// <summary>
    /// <see cref="SqlTemporalUnit"/> that specifies the day component to extract. Can be one of the three following values:
    /// <list type="bullet">
    /// <item><description><see cref="SqlTemporalUnit.Year"/> for a day of year,</description></item>
    /// <item><description><see cref="SqlTemporalUnit.Month"/> for a day of month,</description></item>
    /// <item><description><see cref="SqlTemporalUnit.Week"/> for a day of week.</description></item>
    /// </list>
    /// </summary>
    public SqlTemporalUnit Unit { get; }
}
