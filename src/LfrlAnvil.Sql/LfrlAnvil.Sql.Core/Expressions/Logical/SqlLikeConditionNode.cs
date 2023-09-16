namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlLikeConditionNode : SqlConditionNode
{
    internal SqlLikeConditionNode(SqlExpressionNode value, SqlExpressionNode pattern, SqlExpressionNode? escape, bool isNegated)
        : base( SqlNodeType.Like )
    {
        Value = value;
        Pattern = pattern;
        Escape = escape;
        IsNegated = isNegated;
    }

    public SqlExpressionNode Value { get; }
    public SqlExpressionNode Pattern { get; }
    public SqlExpressionNode? Escape { get; }
    public bool IsNegated { get; }
}
