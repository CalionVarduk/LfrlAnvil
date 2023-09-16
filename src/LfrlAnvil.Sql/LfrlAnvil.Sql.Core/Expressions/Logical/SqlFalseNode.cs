namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlFalseNode : SqlConditionNode
{
    internal SqlFalseNode()
        : base( SqlNodeType.False ) { }
}
