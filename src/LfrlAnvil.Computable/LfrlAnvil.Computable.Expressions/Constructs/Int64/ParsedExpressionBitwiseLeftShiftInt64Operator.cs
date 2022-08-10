using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Int64;

public sealed class ParsedExpressionBitwiseLeftShiftInt64Operator : ParsedExpressionBinaryOperator<long, int>
{
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetLeftArgumentValue( left, out var leftValue ) && TryGetRightArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue << rightValue )
            : null;
    }

    [Pure]
    protected override Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return TryGetLeftArgumentValue( left, out var leftValue ) && leftValue == 0
            ? left
            : null;
    }

    [Pure]
    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        if ( ! TryGetRightArgumentValue( right, out var rightValue ) )
            return null;

        rightValue = rightValue.EuclidModulo( 64 );
        return rightValue == 0 ? left : null;
    }

    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.LeftShift( left, right );
    }
}
