namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a single distinct trait.
/// </summary>
public sealed class SqlDistinctTraitNode : SqlTraitNode
{
    internal SqlDistinctTraitNode()
        : base( SqlNodeType.DistinctTrait ) { }
}
