using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Decimal;

/// <summary>
/// Represents a <see cref="Decimal"/> binary compare operator construct.
/// </summary>
public sealed class ParsedExpressionCompareDecimalOperator : ParsedExpressionBinaryOperator<decimal>
{
    private readonly MethodInfo _compareTo;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionCompareDecimalOperator"/> instance.
    /// </summary>
    public ParsedExpressionCompareDecimalOperator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( decimal ),
            typeof( decimal ),
            typeof( ParsedExpressionCompareDecimalOperator ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue.CompareTo( rightValue ) )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Call( left, _compareTo, right );
    }
}
