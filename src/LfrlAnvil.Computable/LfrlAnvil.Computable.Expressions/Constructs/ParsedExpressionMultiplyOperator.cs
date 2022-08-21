﻿using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public sealed class ParsedExpressionMultiplyOperator : ParsedExpressionBinaryOperator
{
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Multiply( left, right );
    }
}