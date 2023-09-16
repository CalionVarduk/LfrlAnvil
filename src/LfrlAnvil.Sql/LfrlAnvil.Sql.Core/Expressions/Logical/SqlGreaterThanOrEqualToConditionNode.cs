namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlGreaterThanOrEqualToConditionNode : SqlConditionNode
{
    internal SqlGreaterThanOrEqualToConditionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.GreaterThanOrEqualTo )
    {
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }
}
