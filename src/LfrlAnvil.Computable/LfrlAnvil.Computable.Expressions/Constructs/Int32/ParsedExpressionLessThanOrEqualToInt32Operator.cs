using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Int32;

public sealed class ParsedExpressionLessThanOrEqualToInt32Operator : ParsedExpressionBinaryOperator<int>
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
