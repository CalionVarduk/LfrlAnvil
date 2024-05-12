namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node.
/// </summary>
public abstract class SqlConditionNode : SqlNodeBase
{
    internal SqlConditionNode(SqlNodeType nodeType)
        : base( nodeType ) { }

    /// <summary>
    /// Creates a new <see cref="SqlConditionNode"/> of <see cref="SqlNodeType.Unknown"/> type.
    /// </summary>
    protected SqlConditionNode() { }
}
