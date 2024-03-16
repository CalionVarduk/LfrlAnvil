namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlExtractDayFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlExtractDayFunctionExpressionNode(SqlExpressionNode argument, SqlTemporalUnit unit)
        : base( SqlFunctionType.ExtractDay, new[] { argument } )
    {
        Assume.True( unit is SqlTemporalUnit.Year or SqlTemporalUnit.Month or SqlTemporalUnit.Week );
        Unit = unit;
    }

    public SqlTemporalUnit Unit { get; }
}
