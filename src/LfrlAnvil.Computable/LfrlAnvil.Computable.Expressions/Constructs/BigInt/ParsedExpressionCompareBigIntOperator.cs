using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.BigInt;

public sealed class ParsedExpressionCompareBigIntOperator : ParsedExpressionBinaryOperator<BigInteger>
{
    private readonly MethodInfo _compareTo;

    public ParsedExpressionCompareBigIntOperator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( BigInteger ),
            typeof( BigInteger ),
            typeof( ParsedExpressionCompareBigIntOperator ) );
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
