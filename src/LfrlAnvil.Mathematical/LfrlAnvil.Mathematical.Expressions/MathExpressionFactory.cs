using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Mathematical.Expressions.Errors;
using LfrlAnvil.Mathematical.Expressions.Exceptions;
using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions;

public sealed class MathExpressionFactory : IMathExpressionFactory
{
    private readonly MathExpressionFactoryInternalConfiguration _configuration;
    private readonly Func<MathExpressionNumberParserParams, IMathExpressionNumberParser>? _numberParserProvider;

    internal MathExpressionFactory(
        MathExpressionFactoryInternalConfiguration configuration,
        Func<MathExpressionNumberParserParams, IMathExpressionNumberParser>? numberParserProvider)
    {
        _configuration = configuration;
        _numberParserProvider = numberParserProvider;
    }

    public IMathExpressionFactoryConfiguration Configuration => _configuration;

    [Pure]
    public IEnumerable<ReadOnlyMemory<char>> GetConstructSymbols()
    {
        return _configuration.Constructs.Select( kv => kv.Key.AsMemory() );
    }

    [Pure]
    public bool ContainsConstructSymbol(string symbol)
    {
        return ContainsConstructSymbol( StringSlice.Create( symbol ) );
    }

    [Pure]
    public bool ContainsConstructSymbol(ReadOnlyMemory<char> symbol)
    {
        return ContainsConstructSymbol( StringSlice.Create( symbol ) );
    }

    [Pure]
    public bool IsOperatorSymbol(string symbol)
    {
        return IsOperatorSymbol( StringSlice.Create( symbol ) );
    }

    [Pure]
    public bool IsOperatorSymbol(ReadOnlyMemory<char> symbol)
    {
        return IsOperatorSymbol( StringSlice.Create( symbol ) );
    }

    [Pure]
    public bool IsTypeConverterSymbol(string symbol)
    {
        return IsTypeConverterSymbol( StringSlice.Create( symbol ) );
    }

    [Pure]
    public bool IsTypeConverterSymbol(ReadOnlyMemory<char> symbol)
    {
        return IsTypeConverterSymbol( StringSlice.Create( symbol ) );
    }

    [Pure]
    public bool IsFunctionSymbol(string symbol)
    {
        return IsFunctionSymbol( StringSlice.Create( symbol ) );
    }

    [Pure]
    public bool IsFunctionSymbol(ReadOnlyMemory<char> symbol)
    {
        return IsFunctionSymbol( StringSlice.Create( symbol ) );
    }

    [Pure]
    public int? GetBinaryOperatorPrecedence(string symbol)
    {
        return GetBinaryOperatorPrecedence( StringSlice.Create( symbol ) );
    }

    [Pure]
    public int? GetBinaryOperatorPrecedence(ReadOnlyMemory<char> symbol)
    {
        return GetBinaryOperatorPrecedence( StringSlice.Create( symbol ) );
    }

    [Pure]
    public int? GetPrefixUnaryConstructPrecedence(string symbol)
    {
        return GetPrefixUnaryConstructPrecedence( StringSlice.Create( symbol ) );
    }

    [Pure]
    public int? GetPrefixUnaryConstructPrecedence(ReadOnlyMemory<char> symbol)
    {
        return GetPrefixUnaryConstructPrecedence( StringSlice.Create( symbol ) );
    }

    [Pure]
    public int? GetPostfixUnaryConstructPrecedence(string symbol)
    {
        return GetPostfixUnaryConstructPrecedence( StringSlice.Create( symbol ) );
    }

    [Pure]
    public int? GetPostfixUnaryConstructPrecedence(ReadOnlyMemory<char> symbol)
    {
        return GetPostfixUnaryConstructPrecedence( StringSlice.Create( symbol ) );
    }

    [Pure]
    public MathExpression<TArg, TResult> Create<TArg, TResult>(string input)
    {
        if ( TryCreate<TArg, TResult>( input, out var result, out var errors ) )
            return result;

        throw new MathExpressionCreationException( input, errors );
    }

    public bool TryCreate<TArg, TResult>(
        string input,
        [MaybeNullWhen( false )] out MathExpression<TArg, TResult> result,
        out Chain<MathExpressionBuilderError> errors)
    {
        try
        {
            return TryCreateInternal( input, out result, out errors );
        }
        catch ( Exception exc )
        {
            errors = Chain.Create<MathExpressionBuilderError>( new MathExpressionBuilderExceptionError( exc ) );
            result = null;
            return false;
        }
    }

    private bool TryCreateInternal<TArg, TResult>(
        string input,
        [MaybeNullWhen( false )] out MathExpression<TArg, TResult> result,
        out Chain<MathExpressionBuilderError> errors)
    {
        var state = MathExpressionBuilderState.CreateRoot(
            typeof( TArg ),
            _configuration,
            CreateNumberParser( typeof( TArg ), typeof( TResult ) ) );

        var tokenizer = new MathExpressionTokenizer( input, _configuration );

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

            result = new MathExpression<TArg, TResult>(
                input,
                expression,
                stateResult.Result.ArgumentIndexes,
                boundArguments: new Dictionary<StringSlice, TArg?>() );

            errors = Chain<MathExpressionBuilderError>.Empty;
            return true;
        }

        result = null;
        errors = stateResult.Errors;
        return false;
    }

    [Pure]
    private IMathExpressionNumberParser CreateNumberParser(Type argumentType, Type resultType)
    {
        return _numberParserProvider is null
            ? MathExpressionNumberParser.CreateDefaultDecimal( _configuration )
            : _numberParserProvider( new MathExpressionNumberParserParams( _configuration, argumentType, resultType ) );
    }

    [Pure]
    private bool ContainsConstructSymbol(StringSlice symbol)
    {
        return _configuration.Constructs.ContainsKey( symbol );
    }

    [Pure]
    private bool IsOperatorSymbol(StringSlice symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( symbol )?.Type == MathExpressionConstructTokenType.Operator;
    }

    [Pure]
    private bool IsTypeConverterSymbol(StringSlice symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( symbol )?.Type == MathExpressionConstructTokenType.TypeConverter;
    }

    [Pure]
    private bool IsFunctionSymbol(StringSlice symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( symbol )?.Type == MathExpressionConstructTokenType.Function;
    }

    [Pure]
    private int? GetBinaryOperatorPrecedence(StringSlice symbol)
    {
        var constructs = _configuration.Constructs.GetValueOrDefault( symbol )?.BinaryOperators;
        return constructs?.IsEmpty == false ? constructs.Precedence : null;
    }

    [Pure]
    private int? GetPrefixUnaryConstructPrecedence(StringSlice symbol)
    {
        var definition = _configuration.Constructs.GetValueOrDefault( symbol );
        if ( definition is null )
            return null;

        if ( definition.PrefixUnaryOperators.IsEmpty )
            return definition.PrefixTypeConverters.IsEmpty ? null : definition.PrefixTypeConverters.Precedence;

        return definition.PrefixUnaryOperators.Precedence;
    }

    [Pure]
    private int? GetPostfixUnaryConstructPrecedence(StringSlice symbol)
    {
        var definition = _configuration.Constructs.GetValueOrDefault( symbol );
        if ( definition is null )
            return null;

        if ( definition.PostfixUnaryOperators.IsEmpty )
            return definition.PostfixTypeConverters.IsEmpty ? null : definition.PostfixTypeConverters.Precedence;

        return definition.PostfixUnaryOperators.Precedence;
    }

    [Pure]
    IMathExpression<TArg, TResult> IMathExpressionFactory.Create<TArg, TResult>(string input)
    {
        if ( ((IMathExpressionFactory)this).TryCreate<TArg, TResult>( input, out var result, out var errors ) )
            return result;

        throw new MathExpressionCreationException( input, errors );
    }

    bool IMathExpressionFactory.TryCreate<TArg, TResult>(
        string input,
        [MaybeNullWhen( false )] out IMathExpression<TArg, TResult> result,
        out Chain<MathExpressionBuilderError> errors)
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
