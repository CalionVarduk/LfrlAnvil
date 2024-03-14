namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlSortTraitNode : SqlTraitNode
{
    internal SqlSortTraitNode(SqlOrderByNode[] ordering)
        : base( SqlNodeType.SortTrait )
    {
        Ordering = ordering;
    }

    public ReadOnlyArray<SqlOrderByNode> Ordering { get; }
}
