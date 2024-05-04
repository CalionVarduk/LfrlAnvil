using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Numerics;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Computable.Expressions.Constructs.BigInt;

/// <summary>
/// Represents a <see cref="BigInteger"/> binary modulo operator construct.
/// </summary>
public sealed class ParsedExpressionModuloBigIntOperator : ParsedExpressionBinaryOperator<BigInteger>
{
    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue % rightValue )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        if ( ! TryGetArgumentValue( right, out var rightValue ) )
            return null;

        if ( rightValue == BigInteger.Zero )
            throw new DivideByZeroException( ExceptionResources.DividedByZero );

        if ( rightValue == BigInteger.One || rightValue == BigInteger.MinusOne )
            return Expression.Constant( BigInteger.Zero, typeof( BigInteger ) );

        return null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Modulo( left, right );
    }
}
