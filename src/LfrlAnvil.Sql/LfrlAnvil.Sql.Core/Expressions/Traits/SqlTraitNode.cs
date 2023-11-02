namespace LfrlAnvil.Sql.Expressions.Traits;

public abstract class SqlTraitNode : SqlNodeBase
{
    protected SqlTraitNode()
        : this( SqlNodeType.Unknown ) { }

    internal SqlTraitNode(SqlNodeType nodeType)
        : base( nodeType ) { }
}
