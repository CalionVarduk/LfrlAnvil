using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Int32;

public sealed class MathExpressionCompareInt32Operator : MathExpressionBinaryOperator<int>
{
    private readonly MethodInfo _compareTo;

    public MathExpressionCompareInt32Operator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( int ),
            typeof( int ),
            typeof( MathExpressionCompareInt32Operator ) );
    }

    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue.CompareTo( rightValue ) )
            : null;
    }

    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Call( left, _compareTo, right );
    }
}
