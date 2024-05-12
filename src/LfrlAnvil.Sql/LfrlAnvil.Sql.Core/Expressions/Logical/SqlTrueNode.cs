namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines <b>true</b> value.
/// </summary>
public sealed class SqlTrueNode : SqlConditionNode
{
    internal SqlTrueNode()
        : base( SqlNodeType.True ) { }
}
