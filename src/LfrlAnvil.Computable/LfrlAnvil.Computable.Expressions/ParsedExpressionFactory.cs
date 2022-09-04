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
    public IEnumerable<ReadOnlyMemory<char>> GetConstructSymbols()
    {
        return _configuration.Constructs.Select( kv => kv.Key.AsMemory() );
    }

    [Pure]
    public ParsedExpressionConstructType GetConstructType(string symbol)
    {
        return GetConstructType( symbol.AsMemory() );
    }

    [Pure]
    public ParsedExpressionConstructType GetConstructType(ReadOnlyMemory<char> symbol)
    {
        return _configuration.Constructs.TryGetValue( StringSlice.Create( symbol ), out var definition )
            ? definition.Type
            : ParsedExpressionConstructType.None;
    }

    [Pure]
    public Type? GetGenericBinaryOperatorType(string symbol)
    {
        return GetGenericBinaryOperatorType( symbol.AsMemory() );
    }

    [Pure]
    public Type? GetGenericBinaryOperatorType(ReadOnlyMemory<char> symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( StringSlice.Create( symbol ) )?.BinaryOperators.GenericConstruct?.GetType();
    }

    [Pure]
    public IEnumerable<ParsedExpressionBinaryOperatorInfo> GetSpecializedBinaryOperators(string symbol)
    {
        return GetSpecializedBinaryOperators( symbol.AsMemory() );
    }

    [Pure]
    public IEnumerable<ParsedExpressionBinaryOperatorInfo> GetSpecializedBinaryOperators(ReadOnlyMemory<char> symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( StringSlice.Create( symbol ) )
                ?.BinaryOperators.SpecializedConstructs?.Select(
                    kv => new ParsedExpressionBinaryOperatorInfo( kv.Value.GetType(), kv.Key.Left, kv.Key.Right ) ) ??
            Enumerable.Empty<ParsedExpressionBinaryOperatorInfo>();
    }

    [Pure]
    public Type? GetGenericPrefixUnaryConstructType(string symbol)
    {
        return GetGenericPrefixUnaryConstructType( symbol.AsMemory() );
    }

    [Pure]
    public Type? GetGenericPrefixUnaryConstructType(ReadOnlyMemory<char> symbol)
    {
        return _configuration.Constructs.TryGetValue( StringSlice.Create( symbol ), out var definition )
            ? definition.PrefixUnaryOperators.GenericConstruct?.GetType() ?? definition.PrefixTypeConverters.GenericConstruct?.GetType()
            : null;
    }

    [Pure]
    public IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPrefixUnaryConstructs(string symbol)
    {
        return GetSpecializedPrefixUnaryConstructs( symbol.AsMemory() );
    }

    [Pure]
    public IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPrefixUnaryConstructs(ReadOnlyMemory<char> symbol)
    {
        return _configuration.Constructs.TryGetValue( StringSlice.Create( symbol ), out var definition )
            ? (definition.PrefixUnaryOperators.SpecializedConstructs?.Select(
                    kv => new ParsedExpressionUnaryConstructInfo( kv.Value.GetType(), kv.Key ) ) ??
                Enumerable.Empty<ParsedExpressionUnaryConstructInfo>())
            .Concat(
                definition.PrefixTypeConverters.SpecializedConstructs?.Select(
                    kv => new ParsedExpressionUnaryConstructInfo( kv.Value.GetType(), kv.Key ) ) ??
                Enumerable.Empty<ParsedExpressionUnaryConstructInfo>() )
            : Enumerable.Empty<ParsedExpressionUnaryConstructInfo>();
    }

    [Pure]
    public Type? GetGenericPostfixUnaryConstructType(string symbol)
    {
        return GetGenericPostfixUnaryConstructType( symbol.AsMemory() );
    }

    [Pure]
    public Type? GetGenericPostfixUnaryConstructType(ReadOnlyMemory<char> symbol)
    {
        return _configuration.Constructs.TryGetValue( StringSlice.Create( symbol ), out var definition )
            ? definition.PostfixUnaryOperators.GenericConstruct?.GetType() ?? definition.PostfixTypeConverters.GenericConstruct?.GetType()
            : null;
    }

    [Pure]
    public IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPostfixUnaryConstructs(string symbol)
    {
        return GetSpecializedPostfixUnaryConstructs( symbol.AsMemory() );
    }

    [Pure]
    public IEnumerable<ParsedExpressionUnaryConstructInfo> GetSpecializedPostfixUnaryConstructs(ReadOnlyMemory<char> symbol)
    {
        return _configuration.Constructs.TryGetValue( StringSlice.Create( symbol ), out var definition )
            ? (definition.PostfixUnaryOperators.SpecializedConstructs?.Select(
                    kv => new ParsedExpressionUnaryConstructInfo( kv.Value.GetType(), kv.Key ) ) ??
                Enumerable.Empty<ParsedExpressionUnaryConstructInfo>())
            .Concat(
                definition.PostfixTypeConverters.SpecializedConstructs?.Select(
                    kv => new ParsedExpressionUnaryConstructInfo( kv.Value.GetType(), kv.Key ) ) ??
                Enumerable.Empty<ParsedExpressionUnaryConstructInfo>() )
            : Enumerable.Empty<ParsedExpressionUnaryConstructInfo>();
    }

    [Pure]
    public Type? GetTypeConverterTargetType(string symbol)
    {
        return GetTypeConverterTargetType( symbol.AsMemory() );
    }

    [Pure]
    public Type? GetTypeConverterTargetType(ReadOnlyMemory<char> symbol)
    {
        return _configuration.Constructs.TryGetValue( StringSlice.Create( symbol ), out var definition )
            ? definition.PrefixTypeConverters.TargetType ?? definition.PostfixTypeConverters.TargetType
            : null;
    }

    [Pure]
    public Type? GetTypeDeclarationType(string symbol)
    {
        return GetTypeDeclarationType( symbol.AsMemory() );
    }

    [Pure]
    public Type? GetTypeDeclarationType(ReadOnlyMemory<char> symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( StringSlice.Create( symbol ) )?.TypeDeclaration;
    }

    [Pure]
    public ConstantExpression? GetConstantExpression(string symbol)
    {
        return GetConstantExpression( symbol.AsMemory() );
    }

    [Pure]
    public ConstantExpression? GetConstantExpression(ReadOnlyMemory<char> symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( StringSlice.Create( symbol ) )?.Constant;
    }

    [Pure]
    public IEnumerable<LambdaExpression> GetFunctionExpressions(string symbol)
    {
        return GetFunctionExpressions( symbol.AsMemory() );
    }

    [Pure]
    public IEnumerable<LambdaExpression> GetFunctionExpressions(ReadOnlyMemory<char> symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( StringSlice.Create( symbol ) )
                ?.Functions.Functions.Select( kv => kv.Value.Lambda ) ??
            Enumerable.Empty<LambdaExpression>();
    }

    [Pure]
    public Type? GetVariadicFunctionType(string symbol)
    {
        return GetVariadicFunctionType( symbol.AsMemory() );
    }

    [Pure]
    public Type? GetVariadicFunctionType(ReadOnlyMemory<char> symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( StringSlice.Create( symbol ) )?.VariadicFunction?.GetType();
    }

    [Pure]
    public int? GetBinaryOperatorPrecedence(string symbol)
    {
        return GetBinaryOperatorPrecedence( symbol.AsMemory() );
    }

    [Pure]
    public int? GetBinaryOperatorPrecedence(ReadOnlyMemory<char> symbol)
    {
        var constructs = _configuration.Constructs.GetValueOrDefault( StringSlice.Create( symbol ) )?.BinaryOperators;
        return constructs?.IsEmpty == false ? constructs.Precedence : null;
    }

    [Pure]
    public int? GetPrefixUnaryConstructPrecedence(string symbol)
    {
        return GetPrefixUnaryConstructPrecedence( symbol.AsMemory() );
    }

    [Pure]
    public int? GetPrefixUnaryConstructPrecedence(ReadOnlyMemory<char> symbol)
    {
        var definition = _configuration.Constructs.GetValueOrDefault( StringSlice.Create( symbol ) );
        if ( definition is null )
            return null;

        if ( definition.PrefixUnaryOperators.IsEmpty )
            return definition.PrefixTypeConverters.IsEmpty ? null : definition.PrefixTypeConverters.Precedence;

        return definition.PrefixUnaryOperators.Precedence;
    }

    [Pure]
    public int? GetPostfixUnaryConstructPrecedence(string symbol)
    {
        return GetPostfixUnaryConstructPrecedence( symbol.AsMemory() );
    }

    [Pure]
    public int? GetPostfixUnaryConstructPrecedence(ReadOnlyMemory<char> symbol)
    {
        var definition = _configuration.Constructs.GetValueOrDefault( StringSlice.Create( symbol ) );
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
            return TryCreateInternal( input, out result, out errors );
        }
        catch ( Exception exc )
        {
            errors = Chain.Create<ParsedExpressionBuilderError>( new ParsedExpressionBuilderExceptionError( exc ) );
            result = null;
            return false;
        }
    }

    private bool TryCreateInternal<TArg, TResult>(
        string input,
        [MaybeNullWhen( false )] out ParsedExpression<TArg, TResult> result,
        out Chain<ParsedExpressionBuilderError> errors)
    {
        var state = new ExpressionBuilderRootState(
            typeof( TArg ),
            _configuration,
            CreateNumberParser( typeof( TArg ), typeof( TResult ) ) );

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
            var expression = Expression.Lambda<Func<TArg?[], TResult>>(
                stateResult.Result.BodyExpression,
                stateResult.Result.ParameterExpression );

            result = new ParsedExpression<TArg, TResult>(
                input,
                expression,
                stateResult.Result.Delegates,
                stateResult.Result.ArgumentIndexes,
                boundArguments: new Dictionary<StringSlice, TArg?>() );

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
