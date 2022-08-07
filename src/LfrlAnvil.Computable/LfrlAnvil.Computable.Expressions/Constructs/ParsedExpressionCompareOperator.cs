using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public sealed class ParsedExpressionCompareOperator : ParsedExpressionBinaryOperator
{
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        var compareTo = MemberInfoLocator.FindCompareToMethod( left.Type, right.Type, typeof( ParsedExpressionCompareOperator ) );
        return Expression.Call( left, compareTo, right );
    }
}
