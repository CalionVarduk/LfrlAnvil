using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Float;

public sealed class MathExpressionNegateFloatOperator : MathExpressionUnaryOperator<float>
{
    protected override Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return TryGetArgumentValue( operand, out var value )
            ? Expression.Constant( -value )
            : null;
    }

    protected override Expression CreateUnaryExpression(Expression operand)
    {
        return Expression.Negate( operand );
    }
}
