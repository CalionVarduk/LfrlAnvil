using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        return $"{typeText} at index {token.StartIndex}, symbol '{token}', near \"{nearTokenPrefix}{nearToken}{nearTokenPostfix}\"";
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
    internal static ParsedExpressionBuilderError CreateMissingSubExpressionClosingSymbol(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.MissingSubExpressionClosingSymbol, token.Symbol );
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
    internal static ParsedExpressionBuilderError CreateUnexpectedFunctionCall(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedFunctionCall, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateUnexpectedTypeDeclaration(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedTypeDeclaration, token.Symbol );
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
    internal static ParsedExpressionBuilderError CreateUnexpectedMemberAccess(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedMemberAccess, token.Symbol );
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
            Chain.Create( openedParenthesisTokens.Select( CreateUnclosedParenthesis ) ) );
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
    internal static ParsedExpressionBuilderError CreatePrefixUnaryOperatorCouldNotBeResolved(IntermediateToken token, Type argumentType)
    {
        return new ParsedExpressionBuilderMissingUnaryOperatorError(
            ParsedExpressionBuilderErrorType.PrefixUnaryOperatorCouldNotBeResolved,
            token.Symbol,
            argumentType );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreatePostfixUnaryOperatorCouldNotBeResolved(IntermediateToken token, Type argumentType)
    {
        return new ParsedExpressionBuilderMissingUnaryOperatorError(
            ParsedExpressionBuilderErrorType.PostfixUnaryOperatorCouldNotBeResolved,
            token.Symbol,
            argumentType );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreatePrefixTypeConverterCouldNotBeResolved(IntermediateToken token, Type argumentType)
    {
        return new ParsedExpressionBuilderMissingUnaryOperatorError(
            ParsedExpressionBuilderErrorType.PrefixTypeConverterCouldNotBeResolved,
            token.Symbol,
            argumentType );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreatePostfixTypeConverterCouldNotBeResolved(IntermediateToken token, Type argumentType)
    {
        return new ParsedExpressionBuilderMissingUnaryOperatorError(
            ParsedExpressionBuilderErrorType.PostfixTypeConverterCouldNotBeResolved,
            token.Symbol,
            argumentType );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateBinaryOperatorCouldNotBeResolved(
        IntermediateToken token,
        Type leftArgumentType,
        Type rightArgumentType)
    {
        return new ParsedExpressionBuilderMissingBinaryOperatorError(
            ParsedExpressionBuilderErrorType.BinaryOperatorCouldNotBeResolved,
            token.Symbol,
            leftArgumentType,
            rightArgumentType );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateFunctionCouldNotBeResolved(
        IntermediateToken token,
        IReadOnlyList<Expression> parameters)
    {
        return new ParsedExpressionBuilderMissingFunctionError(
            ParsedExpressionBuilderErrorType.FunctionCouldNotBeResolved,
            token.Symbol,
            parameters.Select( e => e.Type ).ToList() );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateMemberCouldNotBeResolved(IntermediateToken token, Type targetType)
    {
        return new ParsedExpressionBuilderMissingMemberError(
            ParsedExpressionBuilderErrorType.MemberCouldNotBeResolved,
            token.Symbol,
            targetType );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateExpectedPrefixUnaryConstruct(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.ExpectedPrefixUnaryConstruct, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateExpectedBinaryOperator(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.ExpectedBinaryOperator, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateExpectedPostfixUnaryOrBinaryConstruct(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.ExpectedPostfixUnaryOrBinaryConstruct, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateExpectedBinaryOrPrefixUnaryConstruct(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.ExpectedBinaryOrPrefixUnaryConstruct, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateAmbiguousPostfixUnaryConstructResolutionFailure(IntermediateToken? token)
    {
        return new ParsedExpressionBuilderError(
            ParsedExpressionBuilderErrorType.AmbiguousPostfixUnaryConstructResolutionFailure,
            token?.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateAmbiguousMemberAccess(
        IntermediateToken token,
        Type targetType,
        IReadOnlyList<MemberInfo> members)
    {
        return new ParsedExpressionBuilderAmbiguousMemberAccessError(
            ParsedExpressionBuilderErrorType.AmbiguousMemberAccess,
            token.Symbol,
            targetType,
            members );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateUnexpectedElementSeparator(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedElementSeparator, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateNestedExpressionFailure(
        IntermediateToken token,
        Chain<ParsedExpressionBuilderError> nestedErrors)
    {
        return new ParsedExpressionBuilderAggregateError(
            ParsedExpressionBuilderErrorType.NestedExpressionFailure,
            nestedErrors,
            token.Symbol );
    }

    [Pure]
    private static ParsedExpressionBuilderError CreateUnclosedParenthesis(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnclosedParenthesis, token.Symbol );
    }
}
