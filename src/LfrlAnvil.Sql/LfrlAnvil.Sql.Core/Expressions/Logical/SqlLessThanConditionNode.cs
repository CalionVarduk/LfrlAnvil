namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlLessThanConditionNode : SqlConditionNode
{
    internal SqlLessThanConditionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.LessThan )
    {
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }
}
