using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a generic null-coalescing binary operator construct.
/// </summary>
public sealed class ParsedExpressionCoalesceOperator : ParsedExpressionBinaryOperator
{
    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        right = ExpressionHelpers.TryUpdateThrowType( right, left.Type );
        return Expression.Coalesce( left, right );
    }
}
