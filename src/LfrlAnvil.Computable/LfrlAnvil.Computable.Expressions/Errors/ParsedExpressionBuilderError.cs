using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Errors;

public class ParsedExpressionBuilderError
{
    private readonly StringSlice? _token;

    public ParsedExpressionBuilderError()
        : this( ParsedExpressionBuilderErrorType.Error ) { }

    internal ParsedExpressionBuilderError(ParsedExpressionBuilderErrorType type, StringSlice? token = null)
    {
        _token = token;
        Type = type;
    }

    public ParsedExpressionBuilderErrorType Type { get; }
    public ReadOnlyMemory<char>? Token => _token?.AsMemory();

    [Pure]
    public override string ToString()
    {
        var typeText = Type.ToString();
        if ( _token is null )
            return typeText;

        var token = _token.Value;
        var nearToken = token.Expand( count: 5 );
        var nearTokenPrefix = nearToken.StartIndex == 0 ? string.Empty : "...";
        var nearTokenPostfix = nearToken.EndIndex == nearToken.Source.Length ? string.Empty : "...";
        return $"{typeText} at index {token.StartIndex}, symbol '{token}', near {nearTokenPrefix}{nearToken}{nearTokenPostfix}";
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateExpressionMustContainAtLeastOneOperand()
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.ExpressionMustContainAtLeastOneOperand );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateExpressionContainsInvalidOperandToOperatorRatio()
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.ExpressionContainsInvalidOperandToOperatorRatio );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateExpressionResultTypeIsNotCompatibleWithExpectedOutputType(
        Type resultType,
        Type expectedType)
    {
        return new ParsedExpressionBuilderResultTypeError(
            ParsedExpressionBuilderErrorType.ExpressionResultTypeIsNotCompatibleWithExpectedOutputType,
            resultType,
            expectedType );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateUnexpectedOperand(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedOperand, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateUnexpectedConstruct(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedConstruct, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateNumberConstantParsingFailure(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.NumberConstantParsingFailure, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateStringConstantParsingFailure(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.StringConstantParsingFailure, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateInvalidArgumentName(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.InvalidArgumentName, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateUnexpectedOpenedParenthesis(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedOpenedParenthesis, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateUnexpectedClosedParenthesis(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedClosedParenthesis, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateConstructHasThrownException(
        IntermediateToken token,
        object construct,
        Exception exception)
    {
        return new ParsedExpressionBuilderConstructError(
            ParsedExpressionBuilderErrorType.ConstructHasThrownException,
            construct,
            token.Symbol,
            exception );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateExpressionContainsUnclosedParentheses(
        IEnumerable<IntermediateToken> openedParenthesisTokens)
    {
        return new ParsedExpressionBuilderAggregateError(
            ParsedExpressionBuilderErrorType.ExpressionContainsUnclosedParentheses,
            openedParenthesisTokens.Select( CreateUnclosedParenthesis ).ToList() );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateOutputTypeConverterHasThrownException(
        ParsedExpressionTypeConverter converter,
        Exception exception)
    {
        return new ParsedExpressionBuilderConstructError(
            ParsedExpressionBuilderErrorType.OutputTypeConverterHasThrownException,
            converter,
            exception: exception );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreatePrefixUnaryOperatorDoesNotExist(IntermediateToken token, Type argumentType)
    {
        return new ParsedExpressionBuilderMissingUnaryOperatorError(
            ParsedExpressionBuilderErrorType.PrefixUnaryOperatorDoesNotExist,
            token.Symbol,
            argumentType );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreatePostfixUnaryOperatorDoesNotExist(IntermediateToken token, Type argumentType)
    {
        return new ParsedExpressionBuilderMissingUnaryOperatorError(
            ParsedExpressionBuilderErrorType.PostfixUnaryOperatorDoesNotExist,
            token.Symbol,
            argumentType );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreatePrefixTypeConverterDoesNotExist(IntermediateToken token, Type argumentType)
    {
        return new ParsedExpressionBuilderMissingUnaryOperatorError(
            ParsedExpressionBuilderErrorType.PrefixTypeConverterDoesNotExist,
            token.Symbol,
            argumentType );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreatePostfixTypeConverterDoesNotExist(IntermediateToken token, Type argumentType)
    {
        return new ParsedExpressionBuilderMissingUnaryOperatorError(
            ParsedExpressionBuilderErrorType.PostfixTypeConverterDoesNotExist,
            token.Symbol,
            argumentType );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateBinaryOperatorDoesNotExist(
        IntermediateToken token,
        Type leftArgumentType,
        Type rightArgumentType)
    {
        return new ParsedExpressionBuilderMissingBinaryOperatorError(
            ParsedExpressionBuilderErrorType.BinaryOperatorDoesNotExist,
            token.Symbol,
            leftArgumentType,
            rightArgumentType );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreatePrefixUnaryOperatorCollectionIsEmpty(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.PrefixUnaryOperatorCollectionIsEmpty, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreatePrefixTypeConverterCollectionIsEmpty(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.PrefixTypeConverterCollectionIsEmpty, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateBinaryOperatorCollectionIsEmpty(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.BinaryOperatorCollectionIsEmpty, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreatePostfixUnaryOrBinaryConstructDoesNotExist(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.PostfixUnaryOrBinaryConstructDoesNotExist, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateBinaryOrPrefixUnaryConstructDoesNotExist(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.BinaryOrPrefixUnaryConstructDoesNotExist, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateAmbiguousPostfixUnaryConstructResolutionFailure(IntermediateToken? token)
    {
        return new ParsedExpressionBuilderError(
            ParsedExpressionBuilderErrorType.AmbiguousPostfixUnaryConstructResolutionFailure,
            token?.Symbol );
    }

    [Pure]
    private static ParsedExpressionBuilderError CreateUnclosedParenthesis(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnclosedParenthesis, token.Symbol );
    }
}
