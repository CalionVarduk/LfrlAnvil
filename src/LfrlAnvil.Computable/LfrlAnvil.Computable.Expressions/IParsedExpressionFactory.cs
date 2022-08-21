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
    IEnumerable<ReadOnlyMemory<char>> GetConstructSymbols();

    [Pure]
    ParsedExpressionConstructType GetConstructType(string symbol);

    [Pure]
    ParsedExpressionConstructType GetConstructType(ReadOnlyMemory<char> symbol);

    [Pure]
    Type? GetGenericBinaryOperatorType(string symbol);

    [Pure]
    Type? GetGenericBinaryOperatorType(ReadOnlyMemory<char> symbol);

    [Pure]
    IEnumerable<ParsedExpressionBinaryOperatorInfo> GetSpecializedBinaryOperators(string symbol);

    [Pure]
    IEnumerable<ParsedExpressionBinaryOperatorInfo> GetSpecializedBinaryOperators(ReadOnlyMemory<char> symbol);

    [Pure]
    Type? GetGenericPrefixUnaryConstructType(string symbol);

    [Pure]
    Type? GetGenericPrefixUnaryConstructType(ReadOnlyMemory<char> symbol);

    [Pure]
    IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPrefixUnaryConstructs(string symbol);

    [Pure]
    IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPrefixUnaryConstructs(ReadOnlyMemory<char> symbol);

    [Pure]
    Type? GetGenericPostfixUnaryConstructType(string symbol);

    [Pure]
    Type? GetGenericPostfixUnaryConstructType(ReadOnlyMemory<char> symbol);

    [Pure]
    IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPostfixUnaryConstructs(string symbol);

    [Pure]
    IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPostfixUnaryConstructs(ReadOnlyMemory<char> symbol);

    [Pure]
    Type? GetTypeConverterTargetType(string symbol);

    [Pure]
    Type? GetTypeConverterTargetType(ReadOnlyMemory<char> symbol);

    [Pure]
    Type? GetTypeDeclarationType(string symbol);

    [Pure]
    Type? GetTypeDeclarationType(ReadOnlyMemory<char> symbol);

    [Pure]
    ConstantExpression? GetConstantExpression(string symbol);

    [Pure]
    ConstantExpression? GetConstantExpression(ReadOnlyMemory<char> symbol);

    [Pure]
    IEnumerable<LambdaExpression> GetFunctionExpressions(string symbol);

    [Pure]
    IEnumerable<LambdaExpression> GetFunctionExpressions(ReadOnlyMemory<char> symbol);

    [Pure]
    Type? GetVariadicFunctionType(string symbol);

    [Pure]
    Type? GetVariadicFunctionType(ReadOnlyMemory<char> symbol);

    [Pure]
    int? GetBinaryOperatorPrecedence(string symbol);

    [Pure]
    int? GetBinaryOperatorPrecedence(ReadOnlyMemory<char> symbol);

    [Pure]
    int? GetPrefixUnaryConstructPrecedence(string symbol);

    [Pure]
    int? GetPrefixUnaryConstructPrecedence(ReadOnlyMemory<char> symbol);

    [Pure]
    int? GetPostfixUnaryConstructPrecedence(string symbol);

    [Pure]
    int? GetPostfixUnaryConstructPrecedence(ReadOnlyMemory<char> symbol);

    [Pure]
    IParsedExpression<TArg, TResult> Create<TArg, TResult>(string input);

    bool TryCreate<TArg, TResult>(
        string input,
        [MaybeNullWhen( false )] out IParsedExpression<TArg, TResult> result,
        out Chain<ParsedExpressionBuilderError> errors);
}
