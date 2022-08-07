using System;
using System.Linq.Expressions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Int32;

public sealed class MathExpressionModuloInt32Operator : MathExpressionBinaryOperator<int>
{
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue % rightValue )
            : null;
    }

    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        if ( ! TryGetArgumentValue( right, out var rightValue ) )
            return null;

        if ( rightValue == 0 )
            throw new DivideByZeroException( ExceptionResources.DividedByZero );

        if ( rightValue is 1 or -1 )
            return Expression.Constant( 0, typeof( int ) );

        return null;
    }

    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Modulo( left, right );
    }
}
