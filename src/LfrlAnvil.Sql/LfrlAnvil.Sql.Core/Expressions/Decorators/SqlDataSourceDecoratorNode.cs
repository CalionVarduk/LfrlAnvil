namespace LfrlAnvil.Sql.Expressions.Decorators;

public abstract class SqlDataSourceDecoratorNode : SqlNodeBase
{
    protected SqlDataSourceDecoratorNode(SqlNodeType nodeType)
        : base( nodeType ) { }
}
