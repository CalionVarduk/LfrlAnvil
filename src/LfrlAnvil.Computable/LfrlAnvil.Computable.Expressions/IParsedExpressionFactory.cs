using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Errors;

namespace LfrlAnvil.Computable.Expressions;

public interface IParsedExpressionFactory
{
    IParsedExpressionFactoryConfiguration Configuration { get; }

    [Pure]
    IEnumerable<ReadOnlyMemory<char>> GetConstructSymbols();

    [Pure]
    bool ContainsConstructSymbol(string symbol);

    [Pure]
    bool ContainsConstructSymbol(ReadOnlyMemory<char> symbol);

    [Pure]
    bool IsOperatorSymbol(string symbol);

    [Pure]
    bool IsOperatorSymbol(ReadOnlyMemory<char> symbol);

    [Pure]
    bool IsTypeConverterSymbol(string symbol);

    [Pure]
    bool IsTypeConverterSymbol(ReadOnlyMemory<char> symbol);

    [Pure]
    bool IsFunctionSymbol(string symbol);

    [Pure]
    bool IsFunctionSymbol(ReadOnlyMemory<char> symbol);

    [Pure]
    bool IsConstantSymbol(string symbol);

    [Pure]
    bool IsConstantSymbol(ReadOnlyMemory<char> symbol);

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
