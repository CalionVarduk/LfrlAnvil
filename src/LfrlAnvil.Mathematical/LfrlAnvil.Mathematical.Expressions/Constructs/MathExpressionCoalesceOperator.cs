using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs;

public sealed class MathExpressionCoalesceOperator : MathExpressionBinaryOperator
{
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Coalesce( left, right );
    }
}
