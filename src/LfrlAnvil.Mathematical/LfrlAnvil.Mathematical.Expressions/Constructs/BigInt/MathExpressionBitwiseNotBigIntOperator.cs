using System.Linq.Expressions;
using System.Numerics;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.BigInt;

public sealed class MathExpressionBitwiseNotBigIntOperator : MathExpressionUnaryOperator<BigInteger>
{
    protected override Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return TryGetArgumentValue( operand, out var value )
            ? Expression.Constant( ~value )
            : null;
    }

    protected override Expression CreateUnaryExpression(Expression operand)
    {
        return Expression.Not( operand );
    }
}
