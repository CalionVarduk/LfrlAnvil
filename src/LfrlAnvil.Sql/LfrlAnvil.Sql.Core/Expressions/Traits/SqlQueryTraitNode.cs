namespace LfrlAnvil.Sql.Expressions.Traits;

public abstract class SqlQueryTraitNode : SqlNodeBase
{
    internal SqlQueryTraitNode(SqlNodeType nodeType)
        : base( nodeType ) { }
}
