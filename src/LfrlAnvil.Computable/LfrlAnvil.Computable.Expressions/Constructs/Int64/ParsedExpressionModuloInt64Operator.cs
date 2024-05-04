using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Int64;

/// <summary>
/// Represents a <see cref="Int64"/> binary modulo operator construct.
/// </summary>
public sealed class ParsedExpressionModuloInt64Operator : ParsedExpressionBinaryOperator<long>
{
    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue % rightValue )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        if ( ! TryGetArgumentValue( right, out var rightValue ) )
            return null;

        if ( rightValue == 0 )
            throw new DivideByZeroException( ExceptionResources.DividedByZero );

        if ( rightValue is 1 or -1 )
            return Expression.Constant( 0L, typeof( long ) );

        return null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Modulo( left, right );
    }
}
