using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.Double;

public sealed class MathExpressionNegateDoubleOperator : MathExpressionUnaryOperator<double>
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
