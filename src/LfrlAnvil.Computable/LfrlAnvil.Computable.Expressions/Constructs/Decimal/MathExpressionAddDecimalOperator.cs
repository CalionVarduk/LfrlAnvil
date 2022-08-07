using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Decimal;

public sealed class MathExpressionAddDecimalOperator : MathExpressionBinaryOperator<decimal>
{
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue + rightValue )
            : null;
    }

    protected override Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && leftValue == 0
            ? right
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
        return Expression.Add( left, right );
    }
}
