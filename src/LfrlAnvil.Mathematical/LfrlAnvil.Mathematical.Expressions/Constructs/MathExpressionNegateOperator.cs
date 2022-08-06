using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs;

public sealed class MathExpressionNegateOperator : MathExpressionUnaryOperator
{
    protected override Expression CreateUnaryExpression(Expression operand)
    {
        return Expression.Negate( operand );
    }
}
