using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <summary>
/// Represents a collection of traits attached to an <see cref="SqlExtendableQueryExpressionNode"/>.
/// </summary>
/// <param name="CommonTableExpressions">
/// Collection of common table expressions that is the result of parsing of all <see cref="SqlCommonTableExpressionTraitNode"/> instances.
/// </param>
/// <param name="ContainsRecursiveCommonTableExpression">
/// Specifies whether or not any of <see cref="SqlCommonTableExpressionTraitNode"/> instances returned <b>true</b>
/// for their <see cref="SqlCommonTableExpressionTraitNode.ContainsRecursive"/> property.
/// </param>
/// <param name="Ordering">
/// Collection of ordering expressions that is the result of parsing of all <see cref="SqlSortTraitNode"/> instances.
/// </param>
/// <param name="Limit">Underlying value of the last <see cref="SqlLimitTraitNode"/> instance.</param>
/// <param name="Offset">Underlying value of the last <see cref="SqlOffsetTraitNode"/> instance.</param>
/// <param name="Custom">Collection of all unrecognized <see cref="SqlTraitNode"/> instances.</param>
public readonly record struct SqlQueryTraits(
    Chain<ReadOnlyArray<SqlCommonTableExpressionNode>> CommonTableExpressions,
    bool ContainsRecursiveCommonTableExpression,
    Chain<ReadOnlyArray<SqlOrderByNode>> Ordering,
    SqlExpressionNode? Limit,
    SqlExpressionNode? Offset,
    Chain<SqlTraitNode> Custom
);
