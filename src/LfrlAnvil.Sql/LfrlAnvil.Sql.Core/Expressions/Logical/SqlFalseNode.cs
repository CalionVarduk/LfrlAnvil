namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines <b>false</b> value.
/// </summary>
public sealed class SqlFalseNode : SqlConditionNode
{
    internal SqlFalseNode()
        : base( SqlNodeType.False ) { }
}
