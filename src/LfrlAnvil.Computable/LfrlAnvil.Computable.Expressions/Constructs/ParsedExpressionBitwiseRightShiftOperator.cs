using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a generic bitwise right shift binary operator construct.
/// </summary>
public sealed class ParsedExpressionBitwiseRightShiftOperator : ParsedExpressionBinaryOperator
{
    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.RightShift( left, right );
    }
}
