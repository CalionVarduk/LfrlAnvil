namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlEqualToConditionNode : SqlConditionNode
{
    internal SqlEqualToConditionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.EqualTo )
    {
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }
}
