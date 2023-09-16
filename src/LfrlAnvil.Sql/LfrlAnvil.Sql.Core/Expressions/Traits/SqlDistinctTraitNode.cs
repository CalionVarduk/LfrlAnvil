namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlDistinctTraitNode : SqlTraitNode
{
    internal SqlDistinctTraitNode()
        : base( SqlNodeType.DistinctTrait ) { }
}
