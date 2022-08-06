using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs;

public sealed class MathExpressionBitwiseNotOperator : MathExpressionUnaryOperator
{
    protected override Expression CreateUnaryExpression(Expression operand)
    {
        return Expression.Not( operand );
    }
}
