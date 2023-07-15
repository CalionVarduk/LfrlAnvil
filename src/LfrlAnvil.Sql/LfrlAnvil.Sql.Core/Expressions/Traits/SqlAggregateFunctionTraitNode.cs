namespace LfrlAnvil.Sql.Expressions.Traits;

public abstract class SqlAggregateFunctionTraitNode : SqlNodeBase
{
    protected SqlAggregateFunctionTraitNode(SqlNodeType nodeType)
        : base( nodeType ) { }
}
