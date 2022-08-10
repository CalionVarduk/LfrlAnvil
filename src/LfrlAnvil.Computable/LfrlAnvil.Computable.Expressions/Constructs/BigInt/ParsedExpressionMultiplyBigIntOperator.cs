using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Numerics;

namespace LfrlAnvil.Computable.Expressions.Constructs.BigInt;

public sealed class ParsedExpressionMultiplyBigIntOperator : ParsedExpressionBinaryOperator<BigInteger>
{
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue * rightValue )
            : null;
    }

    [Pure]
    protected override Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return TryCreateFromOneConstantInternal( right, left );
    }

    [Pure]
    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        return TryCreateFromOneConstantInternal( left, right );
    }

    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Multiply( left, right );
    }

    [Pure]
    private static Expression? TryCreateFromOneConstantInternal(Expression expression, ConstantExpression constant)
    {
        if ( ! TryGetArgumentValue( constant, out var value ) )
            return null;

        if ( value == BigInteger.Zero )
            return constant;

        if ( value == BigInteger.One )
            return expression;

        if ( value == BigInteger.MinusOne )
            return Expression.Negate( expression );

        return null;
    }
}
