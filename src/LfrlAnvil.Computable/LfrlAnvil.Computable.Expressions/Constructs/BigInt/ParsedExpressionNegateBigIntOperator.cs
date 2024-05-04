using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Numerics;

namespace LfrlAnvil.Computable.Expressions.Constructs.BigInt;

/// <summary>
/// Represents a <see cref="BigInteger"/> unary negate operator construct.
/// </summary>
public sealed class ParsedExpressionNegateBigIntOperator : ParsedExpressionUnaryOperator<BigInteger>
{
    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return TryGetArgumentValue( operand, out var value )
            ? Expression.Constant( -value )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateUnaryExpression(Expression operand)
    {
        return Expression.Negate( operand );
    }
}
