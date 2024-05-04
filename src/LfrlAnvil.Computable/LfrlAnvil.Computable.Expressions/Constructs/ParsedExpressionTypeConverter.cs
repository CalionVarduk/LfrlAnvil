using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a type converter construct.
/// </summary>
public class ParsedExpressionTypeConverter
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypeConverter"/> instance.
    /// </summary>
    /// <param name="targetType">Target type.</param>
    /// <param name="sourceType">Optional source type. Equal to null by default.</param>
    public ParsedExpressionTypeConverter(Type targetType, Type? sourceType = null)
    {
        TargetType = targetType;
        SourceType = sourceType;
    }

    /// <summary>
    /// Target type.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Optional source type.
    /// </summary>
    public Type? SourceType { get; }

    [Pure]
    internal Expression Process(Expression operand)
    {
        var result = CreateResult( operand );

        if ( result.Type != TargetType )
        {
            if ( ! result.Type.IsAssignableTo( TargetType ) )
                throw new ParsedExpressionTypeConverterException(
                    Resources.InvalidTypeConverterResultType( result.Type, TargetType ),
                    this );

            result = Expression.Convert( result, TargetType );
        }

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
    protected virtual Expression CreateConversionExpression(Expression operand)
    {
        return Expression.Convert( operand, TargetType );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Expression CreateResult(Expression operand)
    {
        if ( operand.Type == TargetType )
            return operand;

        if ( operand is ConstantExpression constant )
            return TryCreateFromConstant( constant ) ?? CreateConversionExpression( operand );

        return CreateConversionExpression( operand );
    }
}

/// <summary>
/// Represents a type converter construct.
/// </summary>
/// <typeparam name="TTarget">Target type.</typeparam>
public class ParsedExpressionTypeConverter<TTarget> : ParsedExpressionTypeConverter
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypeConverter{TTarget}"/> instance.
    /// </summary>
    /// <param name="sourceType">Optional source type. Equal to null by default.</param>
    public ParsedExpressionTypeConverter(Type? sourceType = null)
        : base( typeof( TTarget ), sourceType ) { }
}

/// <summary>
/// Represents a type converter construct.
/// </summary>
/// <typeparam name="TTarget">Target type.</typeparam>
/// <typeparam name="TSource">Source type.</typeparam>
public class ParsedExpressionTypeConverter<TTarget, TSource> : ParsedExpressionTypeConverter<TTarget>
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypeConverter{TTarget,TSource}"/> instance.
    /// </summary>
    public ParsedExpressionTypeConverter()
        : base( typeof( TSource ) ) { }

    /// <summary>
    /// Attempts to extract a constant value of an argument.
    /// </summary>
    /// <param name="expression">Source constant expression.</param>
    /// <param name="result"><b>out</b> parameter that returns the underlying value.</param>
    /// <returns><b>true</b> if value was extracted successfully, otherwise <b>false</b>.</returns>
    protected static bool TryGetSourceValue(ConstantExpression expression, [MaybeNullWhen( false )] out TSource result)
    {
        return expression.TryGetValue( out result );
    }
}
