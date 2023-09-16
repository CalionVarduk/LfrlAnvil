namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCompoundQueryComponentNode : SqlNodeBase
{
    internal SqlCompoundQueryComponentNode(SqlQueryExpressionNode query, SqlCompoundQueryOperator @operator)
        : base( SqlNodeType.CompoundQueryComponent )
    {
        Query = query;
        Operator = @operator;
    }

    public SqlQueryExpressionNode Query { get; }
    public SqlCompoundQueryOperator Operator { get; }
}
