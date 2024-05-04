using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Int64;

/// <summary>
/// Represents a <see cref="Int64"/> unary bitwise not operator construct.
/// </summary>
public sealed class ParsedExpressionBitwiseNotInt64Operator : ParsedExpressionUnaryOperator<long>
{
    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return TryGetArgumentValue( operand, out var value )
            ? Expression.Constant( ~value )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateUnaryExpression(Expression operand)
    {
        return Expression.Not( operand );
    }
}
