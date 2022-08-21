﻿using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Numerics;

namespace LfrlAnvil.Computable.Expressions.Constructs.BigInt;

public sealed class ParsedExpressionBitwiseRightShiftBigIntOperator : ParsedExpressionBinaryOperator<BigInteger, int>
{
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetLeftArgumentValue( left, out var leftValue ) && TryGetRightArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue >> rightValue )
            : null;
    }

    [Pure]
    protected override Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return TryGetLeftArgumentValue( left, out var leftValue ) && leftValue == BigInteger.Zero
            ? left
            : null;
    }

    [Pure]
    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        return TryGetRightArgumentValue( right, out var rightValue ) && rightValue == 0
            ? left
            : null;
    }

    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.RightShift( left, right );
    }
}