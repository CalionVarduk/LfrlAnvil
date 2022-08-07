using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public sealed class MathExpressionEqualToOperator : MathExpressionBinaryOperator
{
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Equal( left, right );
    }
}
