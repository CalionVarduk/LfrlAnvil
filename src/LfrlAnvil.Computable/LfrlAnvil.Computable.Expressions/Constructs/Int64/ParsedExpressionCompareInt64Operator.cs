using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Int64;

public sealed class ParsedExpressionCompareInt64Operator : ParsedExpressionBinaryOperator<long>
{
    private readonly MethodInfo _compareTo;

    public ParsedExpressionCompareInt64Operator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( long ),
            typeof( long ),
            typeof( ParsedExpressionCompareInt64Operator ) );
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
