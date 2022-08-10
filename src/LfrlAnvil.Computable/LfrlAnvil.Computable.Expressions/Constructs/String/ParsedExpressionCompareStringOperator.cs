using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.String;

public sealed class ParsedExpressionCompareStringOperator : ParsedExpressionBinaryOperator<string>
{
    private readonly MethodInfo _compare;
    private readonly ConstantExpression _ordinal;

    public ParsedExpressionCompareStringOperator()
    {
        _compare = MemberInfoLocator.FindStringCompareMethod();
        _ordinal = Expression.Constant( StringComparison.Ordinal );
    }

    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( string.Compare( leftValue, rightValue, StringComparison.Ordinal ) )
            : null;
    }

    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Call( null, _compare, left, right, _ordinal );
    }
}
