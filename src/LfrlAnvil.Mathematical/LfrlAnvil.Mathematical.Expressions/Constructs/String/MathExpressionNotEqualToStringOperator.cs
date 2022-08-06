﻿using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.String;

public sealed class MathExpressionNotEqualToStringOperator : MathExpressionBinaryOperator<string>
{
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue != rightValue )
            : null;
    }

    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.NotEqual( left, right );
    }
}
