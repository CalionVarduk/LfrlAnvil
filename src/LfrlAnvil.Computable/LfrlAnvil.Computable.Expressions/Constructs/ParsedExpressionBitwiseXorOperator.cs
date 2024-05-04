using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a generic bitwise xor binary operator construct.
/// </summary>
public sealed class ParsedExpressionBitwiseXorOperator : ParsedExpressionBinaryOperator
{
    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.ExclusiveOr( left, right );
    }
}
