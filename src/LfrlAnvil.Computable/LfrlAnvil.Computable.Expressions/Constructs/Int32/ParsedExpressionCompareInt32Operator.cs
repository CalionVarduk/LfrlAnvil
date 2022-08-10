using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Int32;

public sealed class ParsedExpressionCompareInt32Operator : ParsedExpressionBinaryOperator<int>
{
    private readonly MethodInfo _compareTo;

    public ParsedExpressionCompareInt32Operator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( int ),
            typeof( int ),
            typeof( ParsedExpressionCompareInt32Operator ) );
    }

    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue.CompareTo( rightValue ) )
            : null;
    }

    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Call( left, _compareTo, right );
    }
}
