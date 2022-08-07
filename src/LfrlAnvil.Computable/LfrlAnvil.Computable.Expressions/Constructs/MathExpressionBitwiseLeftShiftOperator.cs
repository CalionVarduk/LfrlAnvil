using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public sealed class MathExpressionBitwiseLeftShiftOperator : MathExpressionBinaryOperator
{
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.LeftShift( left, right );
    }
}
