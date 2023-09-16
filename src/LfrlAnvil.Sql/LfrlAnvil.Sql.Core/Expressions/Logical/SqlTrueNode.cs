namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlTrueNode : SqlConditionNode
{
    internal SqlTrueNode()
        : base( SqlNodeType.True ) { }
}
