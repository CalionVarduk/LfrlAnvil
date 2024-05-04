using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a generic compare binary operator construct.
/// </summary>
public sealed class ParsedExpressionCompareOperator : ParsedExpressionBinaryOperator
{
    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        var compareTo = MemberInfoLocator.FindCompareToMethod( left.Type, right.Type, typeof( ParsedExpressionCompareOperator ) );
        return Expression.Call( left, compareTo, right );
    }
}
