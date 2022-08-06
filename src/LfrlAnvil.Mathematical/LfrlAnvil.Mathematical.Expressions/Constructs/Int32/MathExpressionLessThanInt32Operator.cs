using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.Int32;

public sealed class MathExpressionLessThanInt32Operator : MathExpressionBinaryOperator<int>
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
