namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlTemporalDiffFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTemporalDiffFunctionExpressionNode(SqlExpressionNode start, SqlExpressionNode end, SqlTemporalUnit unit)
        : base( SqlFunctionType.TemporalDiff, new[] { start, end } )
    {
        Ensure.IsDefined( unit );
        Unit = unit;
    }

    public SqlTemporalUnit Unit { get; }
}
