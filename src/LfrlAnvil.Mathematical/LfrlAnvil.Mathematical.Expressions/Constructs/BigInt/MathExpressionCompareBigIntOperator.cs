using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.BigInt;

public sealed class MathExpressionCompareBigIntOperator : MathExpressionBinaryOperator<BigInteger>
{
    private readonly MethodInfo _compareTo;

    public MathExpressionCompareBigIntOperator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( BigInteger ),
            typeof( BigInteger ),
            typeof( MathExpressionCompareBigIntOperator ) );
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
