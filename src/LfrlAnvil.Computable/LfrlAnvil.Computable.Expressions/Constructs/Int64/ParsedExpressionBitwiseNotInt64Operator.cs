using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Int64;

public sealed class ParsedExpressionBitwiseNotInt64Operator : ParsedExpressionUnaryOperator<long>
{
    [Pure]
    protected override Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return TryGetArgumentValue( operand, out var value )
            ? Expression.Constant( ~value )
            : null;
    }

    [Pure]
    protected override Expression CreateUnaryExpression(Expression operand)
    {
        return Expression.Not( operand );
    }
}
