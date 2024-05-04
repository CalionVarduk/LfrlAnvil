using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a generic negate unary operator construct.
/// </summary>
public sealed class ParsedExpressionNegateOperator : ParsedExpressionUnaryOperator
{
    /// <inheritdoc />
    [Pure]
    protected override Expression CreateUnaryExpression(Expression operand)
    {
        return Expression.Negate( operand );
    }
}
