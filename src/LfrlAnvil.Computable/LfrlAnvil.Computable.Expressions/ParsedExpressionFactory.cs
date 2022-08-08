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
    public bool IsConstantSymbol(string symbol)
    {
        return IsConstantSymbol( StringSlice.Create( symbol ) );
    }

    [Pure]
    public bool IsConstantSymbol(ReadOnlyMemory<char> symbol)
    {
        return IsConstantSymbol( StringSlice.Create( symbol ) );
    }

    [Pure]
    public bool IsTypeDeclarationSymbol(string symbol)
    {
        return IsTypeDeclarationSymbol( StringSlice.Create( symbol ) );
    }

    [Pure]
    public bool IsTypeDeclarationSymbol(ReadOnlyMemory<char> symbol)
    {
        return IsTypeDeclarationSymbol( StringSlice.Create( symbol ) );
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
        var state = ExpressionBuilderState.CreateRoot(
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
    private bool ContainsConstructSymbol(StringSlice symbol)
    {
        return _configuration.Constructs.ContainsKey( symbol );
    }

    [Pure]
    private bool IsOperatorSymbol(StringSlice symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( symbol )?.IsAny( ConstructTokenType.Operator ) == true;
    }

    [Pure]
    private bool IsTypeConverterSymbol(StringSlice symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( symbol )?.IsAny( ConstructTokenType.TypeConverter ) == true;
    }

    [Pure]
    private bool IsFunctionSymbol(StringSlice symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( symbol )?.Type == ConstructTokenType.Function;
    }

    [Pure]
    private bool IsConstantSymbol(StringSlice symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( symbol )?.Type == ConstructTokenType.Constant;
    }

    [Pure]
    private bool IsTypeDeclarationSymbol(StringSlice symbol)
    {
        return _configuration.Constructs.GetValueOrDefault( symbol )?.Type == ConstructTokenType.TypeDeclaration;
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
    IParsedExpression<TArg, TResult> IParsedExpressionFactory.Create<TArg, TResult>(string input)
    {
        if ( ((IParsedExpressionFactory)this).TryCreate<TArg, TResult>( input, out var result, out var errors ) )
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
