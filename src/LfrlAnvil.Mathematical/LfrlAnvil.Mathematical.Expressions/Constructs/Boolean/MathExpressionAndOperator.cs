﻿using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.Boolean;

public sealed class MathExpressionAndOperator : MathExpressionBinaryOperator<bool>
{
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue && rightValue )
            : null;
    }

    protected override Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        if ( ! TryGetArgumentValue( left, out var leftValue ) )
            return null;

        return leftValue ? right : left;
    }

    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        if ( ! TryGetArgumentValue( right, out var rightValue ) )
            return null;

        return rightValue ? left : right;
    }

    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.AndAlso( left, right );
    }
}
