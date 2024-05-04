using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.String;

/// <summary>
/// Represents a <see cref="String"/> binary compare operator construct.
/// </summary>
public sealed class ParsedExpressionCompareStringOperator : ParsedExpressionBinaryOperator<string>
{
    private readonly MethodInfo _compare;
    private readonly ConstantExpression _ordinal;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionCompareStringOperator"/> instance.
    /// </summary>
    public ParsedExpressionCompareStringOperator()
    {
        _compare = MemberInfoLocator.FindStringCompareMethod();
        _ordinal = Expression.Constant( StringComparison.Ordinal );
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( string.Compare( leftValue, rightValue, StringComparison.Ordinal ) )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Call( null, _compare, left, right, _ordinal );
    }
}
