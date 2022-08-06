using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs;

public sealed class MathExpressionBitwiseAndOperator : MathExpressionBinaryOperator
{
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.And( left, right );
    }
}
