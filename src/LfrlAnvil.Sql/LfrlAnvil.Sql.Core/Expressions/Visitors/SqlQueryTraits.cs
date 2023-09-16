using System;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public readonly record struct SqlQueryTraits(
    Chain<ReadOnlyMemory<SqlCommonTableExpressionNode>> CommonTableExpressions,
    Chain<ReadOnlyMemory<SqlOrderByNode>> Ordering,
    SqlExpressionNode? Limit,
    SqlExpressionNode? Offset,
    Chain<SqlTraitNode> Custom);
