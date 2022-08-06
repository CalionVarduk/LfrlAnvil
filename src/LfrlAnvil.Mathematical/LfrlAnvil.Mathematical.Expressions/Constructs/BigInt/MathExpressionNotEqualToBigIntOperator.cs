using System.Linq.Expressions;
using System.Numerics;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.BigInt;

public sealed class MathExpressionNotEqualToBigIntOperator : MathExpressionBinaryOperator<BigInteger>
{
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue != rightValue )
            : null;
    }

    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.NotEqual( left, right );
    }
}
