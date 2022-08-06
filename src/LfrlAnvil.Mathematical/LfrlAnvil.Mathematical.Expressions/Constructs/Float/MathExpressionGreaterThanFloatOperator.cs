using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.Float;

public sealed class MathExpressionGreaterThanFloatOperator : MathExpressionBinaryOperator<float>
{
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue > rightValue )
            : null;
    }

    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.GreaterThan( left, right );
    }
}
