using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Decimal;

public sealed class ParsedExpressionModuloDecimalOperator : ParsedExpressionBinaryOperator<decimal>
{
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue % rightValue )
            : null;
    }

    [Pure]
    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        if ( ! TryGetArgumentValue( right, out var rightValue ) )
            return null;

        if ( rightValue == 0 )
            throw new DivideByZeroException( ExceptionResources.DividedByZero );

        return null;
    }

    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Modulo( left, right );
    }
}
