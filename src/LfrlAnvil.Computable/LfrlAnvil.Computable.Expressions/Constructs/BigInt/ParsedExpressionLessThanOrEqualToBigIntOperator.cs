using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Numerics;

namespace LfrlAnvil.Computable.Expressions.Constructs.BigInt;

public sealed class ParsedExpressionLessThanOrEqualToBigIntOperator : ParsedExpressionBinaryOperator<BigInteger>
{
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( Boxed.GetBool( leftValue <= rightValue ) )
            : null;
    }

    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.LessThanOrEqual( left, right );
    }
}
