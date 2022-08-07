using System.Linq.Expressions;
using System.Numerics;

namespace LfrlAnvil.Computable.Expressions.Constructs.BigInt;

public sealed class MathExpressionSubtractBigIntOperator : MathExpressionBinaryOperator<BigInteger>
{
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue - rightValue )
            : null;
    }

    protected override Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && leftValue == BigInteger.Zero
            ? Expression.Negate( right )
            : null;
    }

    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        return TryGetArgumentValue( right, out var rightValue ) && rightValue == BigInteger.Zero
            ? left
            : null;
    }

    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Subtract( left, right );
    }
}
