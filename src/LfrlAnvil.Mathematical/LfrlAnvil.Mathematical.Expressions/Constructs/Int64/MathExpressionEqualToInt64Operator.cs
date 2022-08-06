using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.Int64;

public sealed class MathExpressionEqualToInt64Operator : MathExpressionBinaryOperator<long>
{
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue == rightValue )
            : null;
    }

    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Equal( left, right );
    }
}
