using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Numerics;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Computable.Expressions.Constructs.BigInt;

public sealed class ParsedExpressionDivideBigIntOperator : ParsedExpressionBinaryOperator<BigInteger>
{
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue / rightValue )
            : null;
    }

    [Pure]
    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        if ( ! TryGetArgumentValue( right, out var rightValue ) )
            return null;

        if ( rightValue == BigInteger.Zero )
            throw new DivideByZeroException( ExceptionResources.DividedByZero );

        if ( rightValue == BigInteger.One )
            return left;

        if ( rightValue == BigInteger.MinusOne )
            return Expression.Negate( left );

        return null;
    }

    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Divide( left, right );
    }
}
