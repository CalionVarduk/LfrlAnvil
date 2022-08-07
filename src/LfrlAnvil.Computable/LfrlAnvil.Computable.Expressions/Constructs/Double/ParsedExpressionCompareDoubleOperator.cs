using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Double;

public sealed class ParsedExpressionCompareDoubleOperator : ParsedExpressionBinaryOperator<double>
{
    private readonly MethodInfo _compareTo;

    public ParsedExpressionCompareDoubleOperator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( double ),
            typeof( double ),
            typeof( ParsedExpressionCompareDoubleOperator ) );
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
