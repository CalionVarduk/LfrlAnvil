namespace LfrlAnvil.Sql.Expressions.Decorators;

public abstract class SqlAggregateFunctionDecoratorNode : SqlNodeBase
{
    protected SqlAggregateFunctionDecoratorNode(SqlNodeType nodeType)
        : base( nodeType ) { }
}
