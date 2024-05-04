using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Double;

/// <summary>
/// Represents a <see cref="Double"/> binary compare operator construct.
/// </summary>
public sealed class ParsedExpressionCompareDoubleOperator : ParsedExpressionBinaryOperator<double>
{
    private readonly MethodInfo _compareTo;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionCompareDoubleOperator"/> instance.
    /// </summary>
    public ParsedExpressionCompareDoubleOperator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( double ),
            typeof( double ),
            typeof( ParsedExpressionCompareDoubleOperator ) );
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
