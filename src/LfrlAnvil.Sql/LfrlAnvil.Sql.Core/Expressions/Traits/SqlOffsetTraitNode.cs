namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlOffsetTraitNode : SqlTraitNode
{
    internal SqlOffsetTraitNode(SqlExpressionNode value)
        : base( SqlNodeType.OffsetTrait )
    {
        Value = value;
    }

    public SqlExpressionNode Value { get; }
}
