namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlTemporalAddFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTemporalAddFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode value, SqlTemporalUnit unit)
        : base( SqlFunctionType.TemporalAdd, new[] { argument, value } )
    {
        Ensure.IsDefined( unit );
        Unit = unit;
    }

    public SqlTemporalUnit Unit { get; }
}
