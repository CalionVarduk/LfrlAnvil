using System.Linq.Expressions;
using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions.Constructs;

public sealed class MathExpressionCompareOperator : MathExpressionBinaryOperator
{
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        var compareTo = MemberInfoLocator.FindCompareToMethod( left.Type, right.Type, typeof( MathExpressionCompareOperator ) );
        return Expression.Call( left, compareTo, right );
    }
}
