using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a generic less than or equal to binary operator construct.
/// </summary>
public sealed class ParsedExpressionLessThanOrEqualToOperator : ParsedExpressionBinaryOperator
{
    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.LessThanOrEqual( left, right );
    }
}
