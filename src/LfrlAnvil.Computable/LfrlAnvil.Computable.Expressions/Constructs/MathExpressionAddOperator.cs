using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public sealed class MathExpressionAddOperator : MathExpressionBinaryOperator
{
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Add( left, right );
    }
}
