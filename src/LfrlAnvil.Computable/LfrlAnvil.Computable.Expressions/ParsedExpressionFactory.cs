using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Errors;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpressionFactory : IParsedExpressionFactory
{
    private readonly ParsedExpressionFactoryInternalConfiguration _configuration;
    private readonly Func<ParsedExpressionNumberParserParams, IParsedExpressionNumberParser>? _numberParserProvider;

    internal ParsedExpressionFactory(
        ParsedExpressionFactoryInternalConfiguration configuration,
        Func<ParsedExpressionNumberParserParams, IParsedExpressionNumberParser>? numberParserProvider)
    {
        _configuration = configuration;
        _numberParserProvider = numberParserProvider;
    }

    public IParsedExpressionFactoryConfiguration Configuration => _configuration;

    [Pure]
    public IEnumerable<StringSegment> GetConstructSymbols()
    {
        return _configuration.Constructs.Select( kv => kv.Key );
    }

    [Pure]
    public ParsedExpressionConstructType GetConstructType(StringSegment symbol)
    {
        return _configuration.Constructs.TryGetValue( symbol, out var definition )
            ? definition.Type
            : ParsedExpressionConstructType.None;
    }

    [Pure]
    public Type? GetGenericBinaryOperatorType(StringSegment symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( symbol )?.BinaryOperators.GenericConstruct?.GetType();
    }

    [Pure]
    public IEnumerable<ParsedExpressionBinaryOperatorInfo> GetSpecializedBinaryOperators(StringSegment symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( symbol )
                ?.BinaryOperators.SpecializedConstructs?.Select(
                    kv => new ParsedExpressionBinaryOperatorInfo( kv.Value.GetType(), kv.Key.Left, kv.Key.Right ) )
            ?? Enumerable.Empty<ParsedExpressionBinaryOperatorInfo>();
    }

    [Pure]
    public Type? GetGenericPrefixUnaryConstructType(StringSegment symbol)
    {
        return _configuration.Constructs.TryGetValue( symbol, out var definition )
            ? definition.PrefixUnaryOperators.GenericConstruct?.GetType() ?? definition.PrefixTypeConverters.GenericConstruct?.GetType()
            : null;
    }

    [Pure]
    public IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPrefixUnaryConstructs(StringSegment symbol)
    {
        return _configuration.Constructs.TryGetValue( symbol, out var definition )
            ? (definition.PrefixUnaryOperators.SpecializedConstructs?.Select(
                    kv => new ParsedExpressionUnaryConstructInfo( kv.Value.GetType(), kv.Key ) )
                ?? Enumerable.Empty<ParsedExpressionUnaryConstructInfo>())
            .Concat(
                definition.PrefixTypeConverters.SpecializedConstructs?.Select(
                    kv => new ParsedExpressionUnaryConstructInfo( kv.Value.GetType(), kv.Key ) )
                ?? Enumerable.Empty<ParsedExpressionUnaryConstructInfo>() )
            : Enumerable.Empty<ParsedExpressionUnaryConstructInfo>();
    }

    [Pure]
    public Type? GetGenericPostfixUnaryConstructType(StringSegment symbol)
    {
        return _configuration.Constructs.TryGetValue( symbol, out var definition )
            ? definition.PostfixUnaryOperators.GenericConstruct?.GetType() ?? definition.PostfixTypeConverters.GenericConstruct?.GetType()
            : null;
    }

    [Pure]
    public IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPostfixUnaryConstructs(StringSegment symbol)
    {
        return _configuration.Constructs.TryGetValue( symbol, out var definition )
            ? (definition.PostfixUnaryOperators.SpecializedConstructs?.Select(
                    kv => new ParsedExpressionUnaryConstructInfo( kv.Value.GetType(), kv.Key ) )
                ?? Enumerable.Empty<ParsedExpressionUnaryConstructInfo>())
            .Concat(
                definition.PostfixTypeConverters.SpecializedConstructs?.Select(
                    kv => new ParsedExpressionUnaryConstructInfo( kv.Value.GetType(), kv.Key ) )
                ?? Enumerable.Empty<ParsedExpressionUnaryConstructInfo>() )
            : Enumerable.Empty<ParsedExpressionUnaryConstructInfo>();
    }

    [Pure]
    public Type? GetTypeConverterTargetType(StringSegment symbol)
    {
        return _configuration.Constructs.TryGetValue( symbol, out var definition )
            ? definition.PrefixTypeConverters.TargetType ?? definition.PostfixTypeConverters.TargetType
            : null;
    }

    [Pure]
    public Type? GetTypeDeclarationType(StringSegment symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( symbol )?.TypeDeclaration;
    }

    [Pure]
    public ConstantExpression? GetConstantExpression(StringSegment symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( symbol )?.Constant;
    }

    [Pure]
    public IEnumerable<LambdaExpression> GetFunctionExpressions(StringSegment symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( symbol )
                ?.Functions.Functions.Select( kv => kv.Value.Lambda )
            ?? Enumerable.Empty<LambdaExpression>();
    }

    [Pure]
    public Type? GetVariadicFunctionType(StringSegment symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( symbol )?.VariadicFunction?.GetType();
    }

    [Pure]
    public int? GetBinaryOperatorPrecedence(StringSegment symbol)
    {
        var constructs = _configuration.Constructs.GetValueOrDefault( symbol )?.BinaryOperators;
        return constructs?.IsEmpty == false ? constructs.Precedence : null;
    }

    [Pure]
    public int? GetPrefixUnaryConstructPrecedence(StringSegment symbol)
    {
        var definition = _configuration.Constructs.GetValueOrDefault( symbol );
        if ( definition is null )
            return null;

        if ( definition.PrefixUnaryOperators.IsEmpty )
            return definition.PrefixTypeConverters.IsEmpty ? null : definition.PrefixTypeConverters.Precedence;

        return definition.PrefixUnaryOperators.Precedence;
    }

    [Pure]
    public int? GetPostfixUnaryConstructPrecedence(StringSegment symbol)
    {
        var definition = _configuration.Constructs.GetValueOrDefault( symbol );
        if ( definition is null )
            return null;

        if ( definition.PostfixUnaryOperators.IsEmpty )
            return definition.PostfixTypeConverters.IsEmpty ? null : definition.PostfixTypeConverters.Precedence;

        return definition.PostfixUnaryOperators.Precedence;
    }

    [Pure]
    public ParsedExpression<TArg, TResult> Create<TArg, TResult>(string input)
    {
        if ( TryCreate<TArg, TResult>( input, out var result, out var errors ) )
            return result;

        throw new ParsedExpressionCreationException( input, errors );
    }

    public bool TryCreate<TArg, TResult>(
        string input,
        [MaybeNullWhen( false )] out ParsedExpression<TArg, TResult> result,
        out Chain<ParsedExpressionBuilderError> errors)
    {
        try
        {
            return TryCreateInternal( input, bindingInfo: null, out result, out errors );
        }
        catch ( Exception exc )
        {
            errors = Chain.Create<ParsedExpressionBuilderError>( new ParsedExpressionBuilderExceptionError( exc ) );
            result = null;
            return false;
        }
    }

    internal bool TryCreateInternal<TArg, TResult>(
        string input,
        (ParsedExpression<TArg, TResult> Expression, Dictionary<StringSegment, TArg?> Arguments)? bindingInfo,
        [MaybeNullWhen( false )] out ParsedExpression<TArg, TResult> result,
        out Chain<ParsedExpressionBuilderError> errors)
    {
        var boundArguments = bindingInfo?.Arguments.ToDictionary(
            static kv => kv.Key,
            static kv => Expression.Constant( kv.Value, typeof( TArg ) ) );

        var state = new ExpressionBuilderRootState(
            typeof( TArg ),
            _configuration,
            CreateNumberParser( typeof( TArg ), typeof( TResult ) ),
            boundArguments );

        var tokenizer = new ExpressionTokenizer( input, _configuration );

        while ( tokenizer.ReadNextToken( out var token ) )
        {
            errors = state.HandleToken( token );
            if ( errors.Count == 0 )
                continue;

            result = null;
            return false;
        }

        var stateResult = state.GetResult( typeof( TResult ) );
        if ( stateResult.IsOk )
        {
            result = bindingInfo is null
                ? new ParsedExpression<TArg, TResult>(
                    this,
                    input,
                    stateResult.Result.BodyExpression,
                    stateResult.Result.ParameterExpression,
                    stateResult.Result.Delegates,
                    new ParsedExpressionUnboundArguments( stateResult.Result.ArgumentIndexes ),
                    ParsedExpressionBoundArguments<TArg>.Empty,
                    new ParsedExpressionDiscardedArguments( stateResult.Result.DiscardedArguments ) )
                : new ParsedExpression<TArg, TResult>(
                    this,
                    input,
                    stateResult.Result.BodyExpression,
                    stateResult.Result.ParameterExpression,
                    stateResult.Result.Delegates,
                    new ParsedExpressionUnboundArguments( stateResult.Result.ArgumentIndexes ),
                    new ParsedExpressionBoundArguments<TArg>( bindingInfo.Value.Arguments ),
                    bindingInfo.Value.Expression.DiscardedArguments.AddTo( stateResult.Result.DiscardedArguments ) );

            errors = Chain<ParsedExpressionBuilderError>.Empty;
            return true;
        }

        result = null;
        errors = stateResult.Errors;
        return false;
    }

    [Pure]
    private IParsedExpressionNumberParser CreateNumberParser(Type argumentType, Type resultType)
    {
        return _numberParserProvider is null
            ? ParsedExpressionNumberParser.CreateDefaultDecimal( _configuration )
            : _numberParserProvider( new ParsedExpressionNumberParserParams( _configuration, argumentType, resultType ) );
    }

    [Pure]
    IParsedExpression<TArg, TResult> IParsedExpressionFactory.Create<TArg, TResult>(string input)
    {
        if ( ReinterpretCast.To<IParsedExpressionFactory>( this ).TryCreate<TArg, TResult>( input, out var result, out var errors ) )
            return result;

        throw new ParsedExpressionCreationException( input, errors );
    }

    bool IParsedExpressionFactory.TryCreate<TArg, TResult>(
        string input,
        [MaybeNullWhen( false )] out IParsedExpression<TArg, TResult> result,
        out Chain<ParsedExpressionBuilderError> errors)
    {
        if ( TryCreate<TArg, TResult>( input, out var internalResult, out errors ) )
        {
            result = internalResult;
            return true;
        }

        result = null;
        return false;
    }
}
