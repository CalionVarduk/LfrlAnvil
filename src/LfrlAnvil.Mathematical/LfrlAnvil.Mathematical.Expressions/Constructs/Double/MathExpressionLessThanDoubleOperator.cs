using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.Double;

public sealed class MathExpressionLessThanDoubleOperator : MathExpressionBinaryOperator<double>
{
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue < rightValue )
            : null;
    }

    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.LessThan( left, right );
    }
}
