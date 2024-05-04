using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Errors;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions;

/// <summary>
/// Represents a parsed expression factory.
/// </summary>
public interface IParsedExpressionFactory
{
    /// <summary>
    /// Configuration of this factory.
    /// </summary>
    IParsedExpressionFactoryConfiguration Configuration { get; }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all available construct symbols.
    /// </summary>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    IEnumerable<StringSegment> GetConstructSymbols();

    /// <summary>
    /// Returns the <see cref="ParsedExpressionConstructType"/> of the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Construct symbol to check.</param>
    /// <returns><see cref="ParsedExpressionConstructType"/> associated with the provided <paramref name="symbol"/>.</returns>
    [Pure]
    ParsedExpressionConstructType GetConstructType(StringSegment symbol);

    /// <summary>
    /// Returns the generic binary operator construct's type for the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Construct symbol to check.</param>
    /// <returns>Type of the generic binary operator construct's type if it exists.</returns>
    [Pure]
    Type? GetGenericBinaryOperatorType(StringSegment symbol);

    /// <summary>
    /// Returns a collection of all specialized binary operator constructs for the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Construct symbol to check.</param>
    /// <returns>Collection of all specialized binary operator constructs.</returns>
    [Pure]
    IEnumerable<ParsedExpressionBinaryOperatorInfo> GetSpecializedBinaryOperators(StringSegment symbol);

    /// <summary>
    /// Returns the generic prefix unary construct's type for the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Construct symbol to check.</param>
    /// <returns>Type of the generic prefix unary construct's type if it exists.</returns>
    [Pure]
    Type? GetGenericPrefixUnaryConstructType(StringSegment symbol);

    /// <summary>
    /// Returns a collection of all specialized prefix unary constructs for the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Construct symbol to check.</param>
    /// <returns>Collection of all specialized prefix unary constructs.</returns>
    [Pure]
    IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPrefixUnaryConstructs(StringSegment symbol);

    /// <summary>
    /// Returns the generic postfix unary construct's type for the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Construct symbol to check.</param>
    /// <returns>Type of the generic postfix unary construct's type if it exists.</returns>
    [Pure]
    Type? GetGenericPostfixUnaryConstructType(StringSegment symbol);

    /// <summary>
    /// Returns a collection of all specialized postfix unary constructs for the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Construct symbol to check.</param>
    /// <returns>Collection of all specialized postfix unary constructs.</returns>
    [Pure]
    IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPostfixUnaryConstructs(StringSegment symbol);

    /// <summary>
    /// Returns the type converter's target type associated with the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Symbol to check.</param>
    /// <returns>Type converter's target type if it exists.</returns>
    [Pure]
    Type? GetTypeConverterTargetType(StringSegment symbol);

    /// <summary>
    /// Returns the type declaration's type associated with the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Symbol to check.</param>
    /// <returns>Type declaration's type if it exists.</returns>
    [Pure]
    Type? GetTypeDeclarationType(StringSegment symbol);

    /// <summary>
    /// Returns the <see cref="ConstantExpression"/> associated with the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Symbol to check.</param>
    /// <returns><see cref="ConstantExpression"/> if it exists.</returns>
    [Pure]
    ConstantExpression? GetConstantExpression(StringSegment symbol);

    /// <summary>
    /// Returns a collection of all function expressions for the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Construct symbol to check.</param>
    /// <returns>Collection of all function expressions.</returns>
    [Pure]
    IEnumerable<LambdaExpression> GetFunctionExpressions(StringSegment symbol);

    /// <summary>
    /// Returns the type of the variadic function's construct associated with the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Symbol to check.</param>
    /// <returns>Type od the variadic function's construct if it exists.</returns>
    [Pure]
    Type? GetVariadicFunctionType(StringSegment symbol);

    /// <summary>
    /// Returns the binary operator precedence associated with the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Symbol to check.</param>
    /// <returns>Binary operator precedence if it exists.</returns>
    [Pure]
    int? GetBinaryOperatorPrecedence(StringSegment symbol);

    /// <summary>
    /// Returns the prefix unary construct precedence associated with the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Symbol to check.</param>
    /// <returns>Prefix unary construct precedence if it exists.</returns>
    [Pure]
    int? GetPrefixUnaryConstructPrecedence(StringSegment symbol);

    /// <summary>
    /// Returns the postfix unary construct precedence associated with the provided <paramref name="symbol"/>.
    /// </summary>
    /// <param name="symbol">Symbol to check.</param>
    /// <returns>Postfix unary construct precedence if it exists.</returns>
    [Pure]
    int? GetPostfixUnaryConstructPrecedence(StringSegment symbol);

    /// <summary>
    /// Creates a new <see cref="IParsedExpression{TArg,TResult}"/> instance.
    /// </summary>
    /// <param name="input">Input to parse.</param>
    /// <typeparam name="TArg">Argument type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IParsedExpression{TArg,TResult}"/> instance.</returns>
    /// <exception cref="ParsedExpressionCreationException">When <paramref name="input"/> parsing failed.</exception>
    [Pure]
    IParsedExpression<TArg, TResult> Create<TArg, TResult>(string input);

    /// <summary>
    /// Attempts to create a new <see cref="IParsedExpression{TArg,TResult}"/> instance.
    /// </summary>
    /// <param name="input">Input to parse.</param>
    /// <param name="result"><b>out</b> parameter that returns the created <see cref="IParsedExpression{TArg,TResult}"/> instance.</param>
    /// <param name="errors"><b>out</b> parameter that returns parsing errors.</param>
    /// <typeparam name="TArg">Argument type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns><b>true</b> when parsing was successful, otherwise <b>false</b>.</returns>
    bool TryCreate<TArg, TResult>(
        string input,
        [MaybeNullWhen( false )] out IParsedExpression<TArg, TResult> result,
        out Chain<ParsedExpressionBuilderError> errors);
}
