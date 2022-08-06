using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.Float;

public sealed class MathExpressionCompareFloatOperator : MathExpressionBinaryOperator<float>
{
    private readonly MethodInfo _compareTo;

    public MathExpressionCompareFloatOperator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( float ),
            typeof( float ),
            typeof( MathExpressionCompareFloatOperator ) );
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
