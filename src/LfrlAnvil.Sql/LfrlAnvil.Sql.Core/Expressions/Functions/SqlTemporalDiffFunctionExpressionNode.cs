namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlTemporalDiffFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTemporalDiffFunctionExpressionNode(SqlExpressionNode first, SqlExpressionNode second, SqlTemporalUnit unit)
        : base( SqlFunctionType.TemporalDiff, new[] { first, second } )
    {
        Ensure.IsDefined( unit );
        Unit = unit;
    }

    public SqlTemporalUnit Unit { get; }
}
