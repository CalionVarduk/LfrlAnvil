using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.BigInt;

/// <summary>
/// Represents a <see cref="BigInteger"/> binary compare operator construct.
/// </summary>
public sealed class ParsedExpressionCompareBigIntOperator : ParsedExpressionBinaryOperator<BigInteger>
{
    private readonly MethodInfo _compareTo;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionCompareBigIntOperator"/> instance.
    /// </summary>
    public ParsedExpressionCompareBigIntOperator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( BigInteger ),
            typeof( BigInteger ),
            typeof( ParsedExpressionCompareBigIntOperator ) );
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
