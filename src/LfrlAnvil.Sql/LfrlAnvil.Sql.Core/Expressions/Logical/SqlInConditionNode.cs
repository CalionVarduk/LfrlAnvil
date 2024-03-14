namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlInConditionNode : SqlConditionNode
{
    internal SqlInConditionNode(SqlExpressionNode value, SqlExpressionNode[] expressions, bool isNegated)
        : base( SqlNodeType.In )
    {
        Assume.IsNotEmpty( expressions );
        Value = value;
        Expressions = expressions;
        IsNegated = isNegated;
    }

    public SqlExpressionNode Value { get; }
    public ReadOnlyArray<SqlExpressionNode> Expressions { get; }
    public bool IsNegated { get; }
}
