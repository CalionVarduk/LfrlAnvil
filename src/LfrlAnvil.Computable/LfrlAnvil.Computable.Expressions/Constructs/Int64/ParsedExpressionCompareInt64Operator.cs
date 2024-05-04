using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Int64;

/// <summary>
/// Represents a <see cref="Int64"/> binary compare operator construct.
/// </summary>
public sealed class ParsedExpressionCompareInt64Operator : ParsedExpressionBinaryOperator<long>
{
    private readonly MethodInfo _compareTo;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionCompareInt64Operator"/> instance.
    /// </summary>
    public ParsedExpressionCompareInt64Operator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( long ),
            typeof( long ),
            typeof( ParsedExpressionCompareInt64Operator ) );
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
