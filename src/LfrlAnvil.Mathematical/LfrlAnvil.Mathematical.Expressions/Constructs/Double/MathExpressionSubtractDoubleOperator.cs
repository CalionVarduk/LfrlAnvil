using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.Double;

public sealed class MathExpressionSubtractDoubleOperator : MathExpressionBinaryOperator<double>
{
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue - rightValue )
            : null;
    }

    protected override Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && leftValue == 0
            ? Expression.Negate( right )
            : null;
    }

    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        return TryGetArgumentValue( right, out var rightValue ) && rightValue == 0
            ? left
            : null;
    }

    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Subtract( left, right );
    }
}
