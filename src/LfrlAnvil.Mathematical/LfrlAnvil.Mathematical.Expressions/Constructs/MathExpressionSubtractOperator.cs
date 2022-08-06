using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs;

public sealed class MathExpressionSubtractOperator : MathExpressionBinaryOperator
{
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Subtract( left, right );
    }
}
