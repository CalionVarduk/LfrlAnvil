﻿using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Numerics;

namespace LfrlAnvil.Computable.Expressions.Constructs.BigInt;

/// <summary>
/// Represents a <see cref="BigInteger"/> binary greater than or equal to operator construct.
/// </summary>
public sealed class ParsedExpressionGreaterThanOrEqualToBigIntOperator : ParsedExpressionBinaryOperator<BigInteger>
{
    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( Boxed.GetBool( leftValue >= rightValue ) )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.GreaterThanOrEqual( left, right );
    }
}
