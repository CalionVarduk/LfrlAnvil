using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Boolean;

/// <summary>
/// Represents a <see cref="Boolean"/> unary logical not operator construct.
/// </summary>
public sealed class ParsedExpressionNotOperator : ParsedExpressionUnaryOperator<bool>
{
    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return TryGetArgumentValue( operand, out var value )
            ? Expression.Constant( Boxed.GetBool( ! value ) )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateUnaryExpression(Expression operand)
    {
        return Expression.Not( operand );
    }
}
