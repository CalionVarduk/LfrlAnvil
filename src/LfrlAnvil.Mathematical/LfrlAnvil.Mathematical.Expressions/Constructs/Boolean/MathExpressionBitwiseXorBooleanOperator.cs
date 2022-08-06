using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.Boolean;

public sealed class MathExpressionBitwiseXorBooleanOperator : MathExpressionBinaryOperator<bool>
{
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue ^ rightValue )
            : null;
    }

    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.ExclusiveOr( left, right );
    }
}
