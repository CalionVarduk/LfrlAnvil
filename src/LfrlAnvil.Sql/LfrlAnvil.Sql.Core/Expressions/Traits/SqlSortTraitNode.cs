namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a single sort trait.
/// </summary>
public sealed class SqlSortTraitNode : SqlTraitNode
{
    internal SqlSortTraitNode(SqlOrderByNode[] ordering)
        : base( SqlNodeType.SortTrait )
    {
        Ordering = ordering;
    }

    /// <summary>
    /// Collection of ordering definitions.
    /// </summary>
    public ReadOnlyArray<SqlOrderByNode> Ordering { get; }
}
