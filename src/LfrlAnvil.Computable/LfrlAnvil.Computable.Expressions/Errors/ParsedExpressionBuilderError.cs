using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Errors;

/// <summary>
/// Represents an error that occurred during <see cref="IParsedExpression{TArg,TResult}"/> creation.
/// </summary>
public class ParsedExpressionBuilderError
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionBuilderError"/> instance with <see cref="ParsedExpressionBuilderErrorType.Error"/> type.
    /// </summary>
    public ParsedExpressionBuilderError()
        : this( ParsedExpressionBuilderErrorType.Error ) { }

    internal ParsedExpressionBuilderError(ParsedExpressionBuilderErrorType type, StringSegment? token = null)
    {
        Token = token;
        Type = type;
    }

    /// <summary>
    /// Error's type.
    /// </summary>
    public ParsedExpressionBuilderErrorType Type { get; }

    /// <summary>
    /// Input's token associated with this error.
    /// </summary>
    public StringSegment? Token { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBuilderError"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var typeText = Type.ToString();
        if ( Token is null )
            return typeText;

        var token = Token.Value;
        var nearToken = token.Expand( count: 5 );
        var nearTokenPrefix = nearToken.StartIndex == 0 ? string.Empty : "...";
        var nearTokenPostfix = nearToken.EndIndex == nearToken.Source.Length ? string.Empty : "...";
        return $"{typeText} at index {token.StartIndex}, symbol '{token}', near \"{nearTokenPrefix}{nearToken}{nearTokenPostfix}\"";
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateExpressionMustContainAtLeastOneOperand(IntermediateToken? token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.ExpressionMustContainAtLeastOneOperand, token?.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateMacroMustContainAtLeastOneToken(StringSegment macroName)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.MacroMustContainAtLeastOneToken, macroName );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateMacroParameterMustContainAtLeastOneToken(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.MacroParameterMustContainAtLeastOneToken, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateExpressionContainsInvalidOperandToOperatorRatio(IntermediateToken? token)
    {
        return new ParsedExpressionBuilderError(
            ParsedExpressionBuilderErrorType.ExpressionContainsInvalidOperandToOperatorRatio,
            token?.Symbol );
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
    internal static ParsedExpressionBuilderError CreateUnexpectedLocalTermDeclaration(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedLocalTermDeclaration, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateUnexpectedAssignment(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedAssignment, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateUndeclaredLocalTermUsage(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UndeclaredLocalTermUsage, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateUnexpectedDelegateParameterName(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedDelegateParameterName, token.Symbol );
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
    internal static ParsedExpressionBuilderError CreateInvalidLocalTermName(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.InvalidLocalTermName, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateDuplicatedLocalTermName(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.DuplicatedLocalTermName, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateInvalidDelegateParameterName(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.InvalidDelegateParameterName, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateDuplicatedDelegateParameterName(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.DuplicatedDelegateParameterName, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateInvalidMacroParameterName(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.InvalidMacroParameterName, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateDuplicatedMacroParameterName(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.DuplicatedMacroParameterName, token.Symbol );
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
    internal static ParsedExpressionBuilderError CreateUnexpectedOpenedSquareBracket(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedOpenedSquareBracket, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateUnexpectedClosedSquareBracket(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket, token.Symbol );
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
        IntermediateToken? token,
        IEnumerable<IntermediateToken> openedParenthesisTokens)
    {
        return new ParsedExpressionBuilderAggregateError(
            ParsedExpressionBuilderErrorType.ExpressionContainsUnclosedParentheses,
            Chain.Create( openedParenthesisTokens.Select( CreateUnclosedParenthesis ) ),
            token?.Symbol );
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
    internal static ParsedExpressionBuilderError CreateInlineDelegateHasThrownException(IntermediateToken? token, Exception exception)
    {
        return new ParsedExpressionBuilderExceptionError(
            exception,
            ParsedExpressionBuilderErrorType.InlineDelegateError,
            token?.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateLocalTermHasThrownException(StringSegment name, Exception exception)
    {
        return new ParsedExpressionBuilderExceptionError(
            exception,
            ParsedExpressionBuilderErrorType.LocalTermError,
            name );
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
            parameters.Select( static e => e.Type ).ToList() );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateInvalidMacroParameterCount(IntermediateToken token, int actual, int expected)
    {
        return new ParsedExpressionBuilderParameterCountError(
            ParsedExpressionBuilderErrorType.InvalidMacroParameterCount,
            token.Symbol,
            actual,
            expected );
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
    internal static ParsedExpressionBuilderError CreateUnexpectedElementSeparator(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedElementSeparator, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateUnexpectedLineSeparator(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedLineSeparator, token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateUnexpectedEnd(IntermediateToken? token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnexpectedEnd, token?.Symbol );
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
    internal static Chain<ParsedExpressionBuilderError> CreateUnexpectedToken(IntermediateToken? token)
    {
        if ( token is null )
            return Chain<ParsedExpressionBuilderError>.Empty;

        var error = token.Value.Type switch
        {
            IntermediateTokenType.ClosedParenthesis => CreateUnexpectedClosedParenthesis( token.Value ),
            IntermediateTokenType.ClosedSquareBracket => CreateUnexpectedClosedSquareBracket( token.Value ),
            IntermediateTokenType.ElementSeparator => CreateUnexpectedElementSeparator( token.Value ),
            _ => CreateUnexpectedLineSeparator( token.Value )
        };

        return Chain.Create( error );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateMacroResolutionFailure(
        IntermediateToken token,
        Chain<ParsedExpressionBuilderError> nestedErrors)
    {
        return new ParsedExpressionBuilderAggregateError(
            ParsedExpressionBuilderErrorType.MacroResolutionFailure,
            nestedErrors,
            token.Symbol );
    }

    [Pure]
    internal static ParsedExpressionBuilderError CreateMacroParameterResolutionFailure(
        IntermediateToken token,
        Chain<ParsedExpressionBuilderError> nestedErrors)
    {
        return new ParsedExpressionBuilderAggregateError(
            ParsedExpressionBuilderErrorType.MacroParameterResolutionFailure,
            nestedErrors,
            token.Symbol );
    }

    [Pure]
    private static ParsedExpressionBuilderError CreateUnclosedParenthesis(IntermediateToken token)
    {
        return new ParsedExpressionBuilderError( ParsedExpressionBuilderErrorType.UnclosedParenthesis, token.Symbol );
    }
}
