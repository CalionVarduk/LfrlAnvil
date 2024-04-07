using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public readonly record struct SqlQueryTraits(
    Chain<ReadOnlyArray<SqlCommonTableExpressionNode>> CommonTableExpressions,
    bool ContainsRecursiveCommonTableExpression,
    Chain<ReadOnlyArray<SqlOrderByNode>> Ordering,
    SqlExpressionNode? Limit,
    SqlExpressionNode? Offset,
    Chain<SqlTraitNode> Custom
);
