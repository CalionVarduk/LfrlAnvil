﻿using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Int32;

public sealed class ParsedExpressionBitwiseOrInt32Operator : ParsedExpressionBinaryOperator<int>
{
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue | rightValue )
            : null;
    }

    [Pure]
    protected override Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && leftValue == 0
            ? right
            : null;
    }

    [Pure]
    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        return TryGetArgumentValue( right, out var rightValue ) && rightValue == 0
            ? left
            : null;
    }

    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Or( left, right );
    }
}