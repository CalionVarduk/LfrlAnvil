using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Boolean;

/// <summary>
/// Represents a <see cref="Boolean"/> binary compare operator construct.
/// </summary>
public sealed class ParsedExpressionCompareBooleanOperator : ParsedExpressionBinaryOperator<bool>
{
    private readonly MethodInfo _compareTo;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionCompareBooleanOperator"/> instance.
    /// </summary>
    public ParsedExpressionCompareBooleanOperator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( bool ),
            typeof( bool ),
            typeof( ParsedExpressionCompareBooleanOperator ) );
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
