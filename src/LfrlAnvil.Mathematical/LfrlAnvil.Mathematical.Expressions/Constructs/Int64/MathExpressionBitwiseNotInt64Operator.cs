﻿using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.Int64;

public sealed class MathExpressionBitwiseNotInt64Operator : MathExpressionUnaryOperator<long>
{
    protected override Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return TryGetArgumentValue( operand, out var value )
            ? Expression.Constant( ~value )
            : null;
    }

    protected override Expression CreateUnaryExpression(Expression operand)
    {
        return Expression.Not( operand );
    }
}
