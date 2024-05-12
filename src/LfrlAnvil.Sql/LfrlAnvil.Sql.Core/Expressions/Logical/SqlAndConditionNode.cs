namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a binary logical and operation.
/// </summary>
public sealed class SqlAndConditionNode : SqlConditionNode
{
    internal SqlAndConditionNode(SqlConditionNode left, SqlConditionNode right)
        : base( SqlNodeType.And )
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
