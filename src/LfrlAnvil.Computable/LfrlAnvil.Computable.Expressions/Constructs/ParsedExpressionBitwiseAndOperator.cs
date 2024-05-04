using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a generic binary bitwise and operator construct.
/// </summary>
public sealed class ParsedExpressionBitwiseAndOperator : ParsedExpressionBinaryOperator
{
    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.And( left, right );
    }
}
