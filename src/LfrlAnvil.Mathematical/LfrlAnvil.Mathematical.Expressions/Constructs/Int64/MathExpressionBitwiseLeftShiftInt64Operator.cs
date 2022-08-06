using System.Linq.Expressions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.Int64;

public sealed class MathExpressionBitwiseLeftShiftInt64Operator : MathExpressionBinaryOperator<long, int>
{
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetLeftArgumentValue( left, out var leftValue ) && TryGetRightArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue << rightValue )
            : null;
    }

    protected override Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return TryGetLeftArgumentValue( left, out var leftValue ) && leftValue == 0
            ? left
            : null;
    }

    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        if ( ! TryGetRightArgumentValue( right, out var rightValue ) )
            return null;

        rightValue = rightValue.EuclidModulo( 64 );
        return rightValue == 0 ? left : null;
    }

    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.LeftShift( left, right );
    }
}
