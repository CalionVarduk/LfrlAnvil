using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Errors;

public class MathExpressionBuilderError
{
    private readonly StringSlice? _token;

    public MathExpressionBuilderError()
        : this( MathExpressionBuilderErrorType.Error ) { }

    internal MathExpressionBuilderError(MathExpressionBuilderErrorType type, StringSlice? token = null)
    {
        _token = token;
        Type = type;
    }

    public MathExpressionBuilderErrorType Type { get; }
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
    internal static MathExpressionBuilderError CreateExpressionMustContainAtLeastOneOperand()
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.ExpressionMustContainAtLeastOneOperand );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateExpressionContainsInvalidOperandToOperatorRatio()
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.ExpressionContainsInvalidOperandToOperatorRatio );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateExpressionResultTypeIsNotCompatibleWithExpectedOutputType(
        Type resultType,
        Type expectedType)
    {
        return new MathExpressionBuilderResultTypeError(
            MathExpressionBuilderErrorType.ExpressionResultTypeIsNotCompatibleWithExpectedOutputType,
            resultType,
            expectedType );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateUnexpectedOperand(IntermediateToken token)
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.UnexpectedOperand, token.Symbol );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateUnexpectedConstruct(IntermediateToken token)
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.UnexpectedConstruct, token.Symbol );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateNumberConstantParsingFailure(IntermediateToken token)
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.NumberConstantParsingFailure, token.Symbol );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateStringConstantParsingFailure(IntermediateToken token)
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.StringConstantParsingFailure, token.Symbol );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateInvalidArgumentName(IntermediateToken token)
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.InvalidArgumentName, token.Symbol );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateUnexpectedOpenedParenthesis(IntermediateToken token)
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.UnexpectedOpenedParenthesis, token.Symbol );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateUnexpectedClosedParenthesis(IntermediateToken token)
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.UnexpectedClosedParenthesis, token.Symbol );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateConstructHasThrownException(
        IntermediateToken token,
        IMathExpressionConstruct construct,
        Exception exception)
    {
        return new MathExpressionBuilderConstructError(
            MathExpressionBuilderErrorType.ConstructHasThrownException,
            construct,
            token.Symbol,
            exception );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateConstructConsumedInvalidAmountOfOperands(
        IntermediateToken token,
        IMathExpressionConstruct construct)
    {
        return new MathExpressionBuilderConstructError(
            MathExpressionBuilderErrorType.ConstructConsumedInvalidAmountOfOperands,
            construct,
            token.Symbol );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateExpressionContainsUnclosedParentheses(
        IEnumerable<IntermediateToken> openedParenthesisTokens)
    {
        return new MathExpressionBuilderAggregateError(
            MathExpressionBuilderErrorType.ExpressionContainsUnclosedParentheses,
            openedParenthesisTokens.Select( CreateUnclosedParenthesis ).ToList() );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateOutputTypeConverterHasThrownException(
        MathExpressionTypeConverter converter,
        Exception exception)
    {
        return new MathExpressionBuilderConstructError(
            MathExpressionBuilderErrorType.OutputTypeConverterHasThrownException,
            converter,
            exception: exception );
    }

    [Pure]
    internal static MathExpressionBuilderError CreatePrefixUnaryOperatorDoesNotExist(IntermediateToken token, Type argumentType)
    {
        return new MathExpressionBuilderMissingUnaryOperatorError(
            MathExpressionBuilderErrorType.PrefixUnaryOperatorDoesNotExist,
            token.Symbol,
            argumentType );
    }

    [Pure]
    internal static MathExpressionBuilderError CreatePostfixUnaryOperatorDoesNotExist(IntermediateToken token, Type argumentType)
    {
        return new MathExpressionBuilderMissingUnaryOperatorError(
            MathExpressionBuilderErrorType.PostfixUnaryOperatorDoesNotExist,
            token.Symbol,
            argumentType );
    }

    [Pure]
    internal static MathExpressionBuilderError CreatePrefixTypeConverterDoesNotExist(IntermediateToken token, Type argumentType)
    {
        return new MathExpressionBuilderMissingUnaryOperatorError(
            MathExpressionBuilderErrorType.PrefixTypeConverterDoesNotExist,
            token.Symbol,
            argumentType );
    }

    [Pure]
    internal static MathExpressionBuilderError CreatePostfixTypeConverterDoesNotExist(IntermediateToken token, Type argumentType)
    {
        return new MathExpressionBuilderMissingUnaryOperatorError(
            MathExpressionBuilderErrorType.PostfixTypeConverterDoesNotExist,
            token.Symbol,
            argumentType );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateBinaryOperatorDoesNotExist(
        IntermediateToken token,
        Type leftArgumentType,
        Type rightArgumentType)
    {
        return new MathExpressionBuilderMissingBinaryOperatorError(
            MathExpressionBuilderErrorType.BinaryOperatorDoesNotExist,
            token.Symbol,
            leftArgumentType,
            rightArgumentType );
    }

    [Pure]
    internal static MathExpressionBuilderError CreatePrefixUnaryOperatorCollectionIsEmpty(IntermediateToken token)
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.PrefixUnaryOperatorCollectionIsEmpty, token.Symbol );
    }

    [Pure]
    internal static MathExpressionBuilderError CreatePrefixTypeConverterCollectionIsEmpty(IntermediateToken token)
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.PrefixTypeConverterCollectionIsEmpty, token.Symbol );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateBinaryOperatorCollectionIsEmpty(IntermediateToken token)
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.BinaryOperatorCollectionIsEmpty, token.Symbol );
    }

    [Pure]
    internal static MathExpressionBuilderError CreatePostfixUnaryOrBinaryConstructDoesNotExist(IntermediateToken token)
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.PostfixUnaryOrBinaryConstructDoesNotExist, token.Symbol );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateBinaryOrPrefixUnaryConstructDoesNotExist(IntermediateToken token)
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.BinaryOrPrefixUnaryConstructDoesNotExist, token.Symbol );
    }

    [Pure]
    internal static MathExpressionBuilderError CreateAmbiguousPostfixUnaryConstructResolutionFailure(IntermediateToken? token)
    {
        return new MathExpressionBuilderError(
            MathExpressionBuilderErrorType.AmbiguousPostfixUnaryConstructResolutionFailure,
            token?.Symbol );
    }

    [Pure]
    private static MathExpressionBuilderError CreateUnclosedParenthesis(IntermediateToken token)
    {
        return new MathExpressionBuilderError( MathExpressionBuilderErrorType.UnclosedParenthesis, token.Symbol );
    }
}
