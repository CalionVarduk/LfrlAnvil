using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlFilterTraitNode : SqlTraitNode
{
    internal SqlFilterTraitNode(SqlConditionNode filter, bool isConjunction)
        : base( SqlNodeType.FilterTrait )
    {
        Filter = filter;
        IsConjunction = isConjunction;
    }

    public SqlConditionNode Filter { get; }
    public bool IsConjunction { get; }
}
