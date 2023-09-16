namespace LfrlAnvil.Sql.Expressions.Logical;

public abstract class SqlConditionNode : SqlNodeBase
{
    protected SqlConditionNode(SqlNodeType nodeType)
        : base( nodeType ) { }
}
