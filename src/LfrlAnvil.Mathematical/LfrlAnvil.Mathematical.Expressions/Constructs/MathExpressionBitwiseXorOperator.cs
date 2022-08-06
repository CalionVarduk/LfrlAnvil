using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs;

public sealed class MathExpressionBitwiseXorOperator : MathExpressionBinaryOperator
{
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.ExclusiveOr( left, right );
    }
}
