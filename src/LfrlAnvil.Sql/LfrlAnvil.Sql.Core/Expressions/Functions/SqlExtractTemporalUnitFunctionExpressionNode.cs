namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlExtractTemporalUnitFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlExtractTemporalUnitFunctionExpressionNode(SqlExpressionNode argument, SqlTemporalUnit unit)
        : base( SqlFunctionType.ExtractTemporalUnit, new[] { argument } )
    {
        Ensure.IsDefined( unit );
        Unit = unit;
    }

    public SqlTemporalUnit Unit { get; }
}
