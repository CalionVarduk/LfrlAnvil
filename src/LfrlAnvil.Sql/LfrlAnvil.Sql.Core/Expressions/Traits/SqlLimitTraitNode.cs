namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlLimitTraitNode : SqlTraitNode
{
    internal SqlLimitTraitNode(SqlExpressionNode value)
        : base( SqlNodeType.LimitTrait )
    {
        Value = value;
    }

    public SqlExpressionNode Value { get; }
}
