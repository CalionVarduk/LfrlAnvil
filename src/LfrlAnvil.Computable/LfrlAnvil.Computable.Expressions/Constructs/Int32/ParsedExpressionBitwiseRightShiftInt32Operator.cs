using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Int32;

/// <summary>
/// Represents a <see cref="Int32"/> binary bitwise right shift operator construct.
/// </summary>
public sealed class ParsedExpressionBitwiseRightShiftInt32Operator : ParsedExpressionBinaryOperator<int, int>
{
    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetLeftArgumentValue( left, out var leftValue ) && TryGetRightArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue >> rightValue )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return TryGetLeftArgumentValue( left, out var leftValue ) && leftValue == 0
            ? left
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        if ( ! TryGetRightArgumentValue( right, out var rightValue ) )
            return null;

        rightValue = rightValue.EuclidModulo( 32 );
        return rightValue == 0 ? left : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.RightShift( left, right );
    }
}
