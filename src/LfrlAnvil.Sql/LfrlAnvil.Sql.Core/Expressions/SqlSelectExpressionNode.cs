namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlSelectExpressionNode : SqlExpressionNode
{
    internal SqlSelectExpressionNode(SqlSelectNode selection)
        : base( SqlNodeType.SelectExpression )
    {
        Selection = selection;
    }

    public SqlSelectNode Selection { get; }
}
