using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public sealed class ParsedExpressionCompareOperator : ParsedExpressionBinaryOperator
{
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        var compareTo = MemberInfoLocator.FindCompareToMethod( left.Type, right.Type, typeof( ParsedExpressionCompareOperator ) );
        return Expression.Call( left, compareTo, right );
    }
}
