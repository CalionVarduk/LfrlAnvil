using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Errors;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions;

public interface IParsedExpressionFactory
{
    IParsedExpressionFactoryConfiguration Configuration { get; }

    [Pure]
    IEnumerable<StringSlice> GetConstructSymbols();

    [Pure]
    ParsedExpressionConstructType GetConstructType(string symbol);

    [Pure]
    ParsedExpressionConstructType GetConstructType(StringSlice symbol);

    [Pure]
    Type? GetGenericBinaryOperatorType(string symbol);

    [Pure]
    Type? GetGenericBinaryOperatorType(StringSlice symbol);

    [Pure]
    IEnumerable<ParsedExpressionBinaryOperatorInfo> GetSpecializedBinaryOperators(string symbol);

    [Pure]
    IEnumerable<ParsedExpressionBinaryOperatorInfo> GetSpecializedBinaryOperators(StringSlice symbol);

    [Pure]
    Type? GetGenericPrefixUnaryConstructType(string symbol);

    [Pure]
    Type? GetGenericPrefixUnaryConstructType(StringSlice symbol);

    [Pure]
    IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPrefixUnaryConstructs(string symbol);

    [Pure]
    IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPrefixUnaryConstructs(StringSlice symbol);

    [Pure]
    Type? GetGenericPostfixUnaryConstructType(string symbol);

    [Pure]
    Type? GetGenericPostfixUnaryConstructType(StringSlice symbol);

    [Pure]
    IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPostfixUnaryConstructs(string symbol);

    [Pure]
    IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPostfixUnaryConstructs(StringSlice symbol);

    [Pure]
    Type? GetTypeConverterTargetType(string symbol);

    [Pure]
    Type? GetTypeConverterTargetType(StringSlice symbol);

    [Pure]
    Type? GetTypeDeclarationType(string symbol);

    [Pure]
    Type? GetTypeDeclarationType(StringSlice symbol);

    [Pure]
    ConstantExpression? GetConstantExpression(string symbol);

    [Pure]
    ConstantExpression? GetConstantExpression(StringSlice symbol);

    [Pure]
    IEnumerable<LambdaExpression> GetFunctionExpressions(string symbol);

    [Pure]
    IEnumerable<LambdaExpression> GetFunctionExpressions(StringSlice symbol);

    [Pure]
    Type? GetVariadicFunctionType(string symbol);

    [Pure]
    Type? GetVariadicFunctionType(StringSlice symbol);

    [Pure]
    int? GetBinaryOperatorPrecedence(string symbol);

    [Pure]
    int? GetBinaryOperatorPrecedence(StringSlice symbol);

    [Pure]
    int? GetPrefixUnaryConstructPrecedence(string symbol);

    [Pure]
    int? GetPrefixUnaryConstructPrecedence(StringSlice symbol);

    [Pure]
    int? GetPostfixUnaryConstructPrecedence(string symbol);

    [Pure]
    int? GetPostfixUnaryConstructPrecedence(StringSlice symbol);

    [Pure]
    IParsedExpression<TArg, TResult> Create<TArg, TResult>(string input);

    bool TryCreate<TArg, TResult>(
        string input,
        [MaybeNullWhen( false )] out IParsedExpression<TArg, TResult> result,
        out Chain<ParsedExpressionBuilderError> errors);
}
