using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Boolean;

public sealed class MathExpressionNotOperator : MathExpressionUnaryOperator<bool>
{
    protected override Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return TryGetArgumentValue( operand, out var value )
            ? Expression.Constant( ! value )
            : null;
    }

    protected override Expression CreateUnaryExpression(Expression operand)
    {
        return Expression.Not( operand );
    }
}
