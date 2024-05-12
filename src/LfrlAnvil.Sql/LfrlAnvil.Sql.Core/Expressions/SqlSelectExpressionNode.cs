namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a conversion of <see cref="SqlSelectNode"/>
/// to <see cref="SqlExpressionNode"/>.
/// </summary>
public sealed class SqlSelectExpressionNode : SqlExpressionNode
{
    internal SqlSelectExpressionNode(SqlSelectNode selection)
        : base( SqlNodeType.SelectExpression )
    {
        Selection = selection;
    }

    /// <summary>
    /// Underlying selection.
    /// </summary>
    public SqlSelectNode Selection { get; }
}
