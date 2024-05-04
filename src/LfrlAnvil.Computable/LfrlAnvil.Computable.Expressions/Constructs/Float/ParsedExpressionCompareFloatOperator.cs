﻿using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Float;

/// <summary>
/// Represents a <see cref="float"/> binary compare operator construct.
/// </summary>
public sealed class ParsedExpressionCompareFloatOperator : ParsedExpressionBinaryOperator<float>
{
    private readonly MethodInfo _compareTo;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionCompareFloatOperator"/> instance.
    /// </summary>
    public ParsedExpressionCompareFloatOperator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( float ),
            typeof( float ),
            typeof( ParsedExpressionCompareFloatOperator ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue.CompareTo( rightValue ) )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Call( left, _compareTo, right );
    }
}
