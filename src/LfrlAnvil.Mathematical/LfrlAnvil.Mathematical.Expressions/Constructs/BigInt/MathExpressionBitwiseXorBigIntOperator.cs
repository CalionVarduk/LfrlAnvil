﻿using System.Linq.Expressions;
using System.Numerics;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.BigInt;

public sealed class MathExpressionBitwiseXorBigIntOperator : MathExpressionBinaryOperator<BigInteger>
{
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue ^ rightValue )
            : null;
    }

    protected override Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && leftValue == BigInteger.Zero
            ? right
            : null;
    }

    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        return TryGetArgumentValue( right, out var rightValue ) && rightValue == BigInteger.Zero
            ? left
            : null;
    }

    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.ExclusiveOr( left, right );
    }
}
