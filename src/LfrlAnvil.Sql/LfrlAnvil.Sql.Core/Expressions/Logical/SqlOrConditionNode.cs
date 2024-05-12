namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a binary logical or operation.
/// </summary>
public sealed class SqlOrConditionNode : SqlConditionNode
{
    internal SqlOrConditionNode(SqlConditionNode left, SqlConditionNode right)
        : base( SqlNodeType.Or )
    {
        Left = left;
        Right = right;
    }

    /// <summary>
    /// First operand.
    /// </summary>
    public SqlConditionNode Left { get; }

    /// <summary>
    /// Second operand.
    /// </summary>
    public SqlConditionNode Right { get; }
}
