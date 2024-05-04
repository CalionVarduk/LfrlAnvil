using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a constant construct.
/// </summary>
public class ParsedExpressionConstant
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionConstant"/> instance.
    /// </summary>
    /// <param name="type">Value's type.</param>
    /// <param name="value">Underlying value.</param>
    public ParsedExpressionConstant(Type type, object? value)
    {
        Expression = System.Linq.Expressions.Expression.Constant( value, type );
    }

    /// <summary>
    /// Underlying <see cref="System.Linq.Expressions.Expression"/>.
    /// </summary>
    public ConstantExpression Expression { get; }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionConstant{T}"/> instance.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    /// <typeparam name="T">value's type.</typeparam>
    /// <returns>New <see cref="ParsedExpressionConstant{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionConstant<T> Create<T>(T value)
    {
        return new ParsedExpressionConstant<T>( value );
    }
}

/// <summary>
/// Represents a constant construct.
/// </summary>
/// <typeparam name="T">Value's type.</typeparam>
public class ParsedExpressionConstant<T> : ParsedExpressionConstant
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionConstant{T}"/> instance.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    public ParsedExpressionConstant(T value)
        : base( typeof( T ), value ) { }
}
