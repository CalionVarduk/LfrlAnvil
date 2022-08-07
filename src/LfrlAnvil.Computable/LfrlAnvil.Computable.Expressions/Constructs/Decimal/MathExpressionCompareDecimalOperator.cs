using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Decimal;

public sealed class MathExpressionCompareDecimalOperator : MathExpressionBinaryOperator<decimal>
{
    private readonly MethodInfo _compareTo;

    public MathExpressionCompareDecimalOperator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( decimal ),
            typeof( decimal ),
            typeof( MathExpressionCompareDecimalOperator ) );
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
