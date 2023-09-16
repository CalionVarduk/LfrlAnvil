namespace LfrlAnvil.Sql.Expressions.Traits;

public abstract class SqlTraitNode : SqlNodeBase
{
    protected SqlTraitNode(SqlNodeType nodeType)
        : base( nodeType ) { }
}
