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
    IEnumerable<StringSegment> GetConstructSymbols();

    [Pure]
    ParsedExpressionConstructType GetConstructType(StringSegment symbol);

    [Pure]
    Type? GetGenericBinaryOperatorType(StringSegment symbol);

    [Pure]
    IEnumerable<ParsedExpressionBinaryOperatorInfo> GetSpecializedBinaryOperators(StringSegment symbol);

    [Pure]
    Type? GetGenericPrefixUnaryConstructType(StringSegment symbol);

    [Pure]
    IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPrefixUnaryConstructs(StringSegment symbol);

    [Pure]
    Type? GetGenericPostfixUnaryConstructType(StringSegment symbol);

    [Pure]
    IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPostfixUnaryConstructs(StringSegment symbol);

    [Pure]
    Type? GetTypeConverterTargetType(StringSegment symbol);

    [Pure]
    Type? GetTypeDeclarationType(StringSegment symbol);

    [Pure]
    ConstantExpression? GetConstantExpression(StringSegment symbol);

    [Pure]
    IEnumerable<LambdaExpression> GetFunctionExpressions(StringSegment symbol);

    [Pure]
    Type? GetVariadicFunctionType(StringSegment symbol);

    [Pure]
    int? GetBinaryOperatorPrecedence(StringSegment symbol);

    [Pure]
    int? GetPrefixUnaryConstructPrecedence(StringSegment symbol);

    [Pure]
    int? GetPostfixUnaryConstructPrecedence(StringSegment symbol);

    [Pure]
    IParsedExpression<TArg, TResult> Create<TArg, TResult>(string input);

    bool TryCreate<TArg, TResult>(
        string input,
        [MaybeNullWhen( false )] out IParsedExpression<TArg, TResult> result,
        out Chain<ParsedExpressionBuilderError> errors);
}
