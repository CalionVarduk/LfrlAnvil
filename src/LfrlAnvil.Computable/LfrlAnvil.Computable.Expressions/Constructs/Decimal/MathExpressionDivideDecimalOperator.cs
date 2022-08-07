using System;
using System.Linq.Expressions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Decimal;

public sealed class MathExpressionDivideDecimalOperator : MathExpressionBinaryOperator<decimal>
{
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue / rightValue )
            : null;
    }

    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        if ( ! TryGetArgumentValue( right, out var rightValue ) )
            return null;

        if ( rightValue == 0 )
            throw new DivideByZeroException( ExceptionResources.DividedByZero );

        if ( rightValue == 1 )
            return left;

        if ( rightValue == -1 )
            return Expression.Negate( left );

        return null;
    }

    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Divide( left, right );
    }
}
