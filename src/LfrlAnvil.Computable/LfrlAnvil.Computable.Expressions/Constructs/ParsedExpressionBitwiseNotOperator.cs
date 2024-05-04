using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a generic bitwise not unary operator construct.
/// </summary>
public sealed class ParsedExpressionBitwiseNotOperator : ParsedExpressionUnaryOperator
{
    /// <inheritdoc />
    [Pure]
    protected override Expression CreateUnaryExpression(Expression operand)
    {
        return Expression.Not( operand );
    }
}
