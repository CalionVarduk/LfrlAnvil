using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Boolean;

public sealed class MathExpressionCompareBooleanOperator : MathExpressionBinaryOperator<bool>
{
    private readonly MethodInfo _compareTo;

    public MathExpressionCompareBooleanOperator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( bool ),
            typeof( bool ),
            typeof( MathExpressionCompareBooleanOperator ) );
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
