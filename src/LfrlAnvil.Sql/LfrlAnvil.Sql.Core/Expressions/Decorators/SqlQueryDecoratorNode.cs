namespace LfrlAnvil.Sql.Expressions.Decorators;

public abstract class SqlQueryDecoratorNode : SqlNodeBase
{
    internal SqlQueryDecoratorNode(SqlNodeType nodeType)
        : base( nodeType ) { }
}
