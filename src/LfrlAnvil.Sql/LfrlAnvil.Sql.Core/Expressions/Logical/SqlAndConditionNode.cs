namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlAndConditionNode : SqlConditionNode
{
    internal SqlAndConditionNode(SqlConditionNode left, SqlConditionNode right)
        : base( SqlNodeType.And )
    {
        Left = left;
        Right = right;
    }

    public SqlConditionNode Left { get; }
    public SqlConditionNode Right { get; }
}
