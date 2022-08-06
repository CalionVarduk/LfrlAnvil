using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs;

public sealed class MathExpressionLessThanOperator : MathExpressionBinaryOperator
{
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.LessThan( left, right );
    }
}
