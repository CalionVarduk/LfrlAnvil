using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a unary operator construct.
/// </summary>
public abstract class ParsedExpressionUnaryOperator
{
    [Pure]
    internal Expression Process(Expression operand)
    {
        var result = CreateResult( operand );
        return result;
    }

    /// <summary>
    /// Attempts to create an expression from a constant.
    /// </summary>
    /// <param name="operand">Constant argument.</param>
    /// <returns>New <see cref="Expression"/> or null when it could not be created.</returns>
    [Pure]
    protected virtual Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return null;
    }

    /// <summary>
    /// Creates an expression.
    /// </summary>
    /// <param name="operand">Argument.</param>
    /// <returns>New <see cref="Expression"/>.</returns>
    [Pure]
    protected abstract Expression CreateUnaryExpression(Expression operand);

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Expression CreateResult(Expression operand)
    {
        if ( operand is ConstantExpression constant )
            return TryCreateFromConstant( constant ) ?? CreateUnaryExpression( operand );

        return CreateUnaryExpression( operand );
    }
}

/// <summary>
/// Represents a unary operator construct.
/// </summary>
/// <typeparam name="TArg">Argument's type.</typeparam>
public abstract class ParsedExpressionUnaryOperator<TArg> : ParsedExpressionTypedUnaryOperator
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionUnaryOperator{TArg}"/> instance.
    /// </summary>
    protected ParsedExpressionUnaryOperator()
        : base( typeof( TArg ) ) { }

    /// <summary>
    /// Attempts to extract a constant value of an argument.
    /// </summary>
    /// <param name="expression">Source constant expression.</param>
    /// <param name="result"><b>out</b> parameter that returns the underlying value.</param>
    /// <returns><b>true</b> if value was extracted successfully, otherwise <b>false</b>.</returns>
    protected static bool TryGetArgumentValue(ConstantExpression expression, [MaybeNullWhen( false )] out TArg result)
    {
        return expression.TryGetValue( out result );
    }
}
