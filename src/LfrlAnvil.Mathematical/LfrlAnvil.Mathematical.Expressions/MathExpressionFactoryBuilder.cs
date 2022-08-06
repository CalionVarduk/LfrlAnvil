using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Mathematical.Expressions.Constructs;
using LfrlAnvil.Mathematical.Expressions.Exceptions;
using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions;

public sealed class MathExpressionFactoryBuilder
{
    private readonly List<(StringSlice Symbol, ConstructType Type, IMathExpressionConstruct Construct)> _constructs;
    private readonly Dictionary<(StringSlice Symbol, ConstructType Type), int> _precedences;
    private IMathExpressionFactoryConfiguration? _configuration;
    private Func<MathExpressionNumberParserParams, IMathExpressionNumberParser>? _numberParserProvider;

    public MathExpressionFactoryBuilder()
    {
        _constructs = new List<(StringSlice, ConstructType, IMathExpressionConstruct)>();
        _precedences = new Dictionary<(StringSlice, ConstructType), int>();
        _configuration = null;
        _numberParserProvider = null;
    }

    public MathExpressionFactoryBuilder SetDefaultConfiguration()
    {
        _configuration = null;
        return this;
    }

    public MathExpressionFactoryBuilder SetConfiguration(IMathExpressionFactoryConfiguration configuration)
    {
        _configuration = configuration;
        return this;
    }

    public MathExpressionFactoryBuilder SetDefaultNumberParserProvider()
    {
        _numberParserProvider = null;
        return this;
    }

    public MathExpressionFactoryBuilder SetNumberParserProvider(
        Func<MathExpressionNumberParserParams, IMathExpressionNumberParser> numberParserProvider)
    {
        _numberParserProvider = numberParserProvider;
        return this;
    }

    public MathExpressionFactoryBuilder AddBinaryOperator(string symbol, MathExpressionBinaryOperator @operator)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ConstructType.BinaryOperator, @operator) );
        return this;
    }

    public MathExpressionFactoryBuilder AddBinaryOperator(ReadOnlyMemory<char> symbol, MathExpressionBinaryOperator @operator)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ConstructType.BinaryOperator, @operator) );
        return this;
    }

    public MathExpressionFactoryBuilder AddPrefixUnaryOperator(string symbol, MathExpressionUnaryOperator @operator)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ConstructType.PrefixUnaryConstruct, @operator) );
        return this;
    }

    public MathExpressionFactoryBuilder AddPrefixUnaryOperator(ReadOnlyMemory<char> symbol, MathExpressionUnaryOperator @operator)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ConstructType.PrefixUnaryConstruct, @operator) );
        return this;
    }

    public MathExpressionFactoryBuilder AddPostfixUnaryOperator(string symbol, MathExpressionUnaryOperator @operator)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ConstructType.PostfixUnaryConstruct, @operator) );
        return this;
    }

    public MathExpressionFactoryBuilder AddPostfixUnaryOperator(ReadOnlyMemory<char> symbol, MathExpressionUnaryOperator @operator)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ConstructType.PostfixUnaryConstruct, @operator) );
        return this;
    }

    public MathExpressionFactoryBuilder AddPrefixTypeConverter(string symbol, MathExpressionTypeConverter converter)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ConstructType.PrefixUnaryConstruct, converter) );
        return this;
    }

    public MathExpressionFactoryBuilder AddPrefixTypeConverter(ReadOnlyMemory<char> symbol, MathExpressionTypeConverter converter)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ConstructType.PrefixUnaryConstruct, converter) );
        return this;
    }

    public MathExpressionFactoryBuilder AddPostfixTypeConverter(string symbol, MathExpressionTypeConverter converter)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ConstructType.PostfixUnaryConstruct, converter) );
        return this;
    }

    public MathExpressionFactoryBuilder AddPostfixTypeConverter(ReadOnlyMemory<char> symbol, MathExpressionTypeConverter converter)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ConstructType.PostfixUnaryConstruct, converter) );
        return this;
    }

    public MathExpressionFactoryBuilder SetBinaryOperatorPrecedence(string symbol, int precedence)
    {
        _precedences[(StringSlice.Create( symbol ), ConstructType.BinaryOperator)] = precedence;
        return this;
    }

    public MathExpressionFactoryBuilder SetBinaryOperatorPrecedence(ReadOnlyMemory<char> symbol, int precedence)
    {
        _precedences[(StringSlice.Create( symbol ), ConstructType.BinaryOperator)] = precedence;
        return this;
    }

    public MathExpressionFactoryBuilder SetPrefixUnaryConstructPrecedence(string symbol, int precedence)
    {
        _precedences[(StringSlice.Create( symbol ), ConstructType.PrefixUnaryConstruct)] = precedence;
        return this;
    }

    public MathExpressionFactoryBuilder SetPrefixUnaryConstructPrecedence(ReadOnlyMemory<char> symbol, int precedence)
    {
        _precedences[(StringSlice.Create( symbol ), ConstructType.PrefixUnaryConstruct)] = precedence;
        return this;
    }

    public MathExpressionFactoryBuilder SetPostfixUnaryConstructPrecedence(string symbol, int precedence)
    {
        _precedences[(StringSlice.Create( symbol ), ConstructType.PostfixUnaryConstruct)] = precedence;
        return this;
    }

    public MathExpressionFactoryBuilder SetPostfixUnaryConstructPrecedence(ReadOnlyMemory<char> symbol, int precedence)
    {
        _precedences[(StringSlice.Create( symbol ), ConstructType.PostfixUnaryConstruct)] = precedence;
        return this;
    }

    [Pure]
    public IMathExpressionFactoryConfiguration? GetCurrentConfiguration()
    {
        return _configuration;
    }

    [Pure]
    public Func<MathExpressionNumberParserParams, IMathExpressionNumberParser>? GetCurrentNumberParserProvider()
    {
        return _numberParserProvider;
    }

    [Pure]
    public IEnumerable<KeyValuePair<ReadOnlyMemory<char>, IMathExpressionConstruct>> GetCurrentConstructs()
    {
        return _constructs.Select( x => KeyValuePair.Create( x.Symbol.AsMemory(), x.Construct ) );
    }

    [Pure]
    public IEnumerable<KeyValuePair<ReadOnlyMemory<char>, int>> GetCurrentBinaryOperatorPrecedences()
    {
        return _precedences
            .Where( kv => kv.Key.Type == ConstructType.BinaryOperator )
            .Select( kv => KeyValuePair.Create( kv.Key.Symbol.AsMemory(), kv.Value ) );
    }

    [Pure]
    public IEnumerable<KeyValuePair<ReadOnlyMemory<char>, int>> GetCurrentPrefixUnaryConstructPrecedences()
    {
        return _precedences
            .Where( kv => kv.Key.Type == ConstructType.PrefixUnaryConstruct )
            .Select( kv => KeyValuePair.Create( kv.Key.Symbol.AsMemory(), kv.Value ) );
    }

    [Pure]
    public IEnumerable<KeyValuePair<ReadOnlyMemory<char>, int>> GetCurrentPostfixUnaryConstructPrecedences()
    {
        return _precedences
            .Where( kv => kv.Key.Type == ConstructType.PostfixUnaryConstruct )
            .Select( kv => KeyValuePair.Create( kv.Key.Symbol.AsMemory(), kv.Value ) );
    }

    [Pure]
    public MathExpressionFactory Build()
    {
        var binaryOperators = new List<MathExpressionBinaryOperator>();
        var prefixUnaryOperators = new List<MathExpressionUnaryOperator>();
        var postfixUnaryOperators = new List<MathExpressionUnaryOperator>();
        var prefixTypeConverters = new List<MathExpressionTypeConverter>();
        var postfixTypeConverters = new List<MathExpressionTypeConverter>();
        var constructDefinitions = new Dictionary<StringSlice, MathExpressionConstructTokenDefinition>();

        var configuration = new MathExpressionFactoryInternalConfiguration(
            constructDefinitions,
            _configuration ?? new MathExpressionFactoryDefaultConfiguration() );

        var errorMessages = configuration.Validate();
        var symbolGroups = _constructs.GroupBy( c => c.Symbol );

        foreach ( var g in symbolGroups )
        {
            if ( ! TokenValidation.IsValidConstructSymbol( g.Key, configuration.StringDelimiter ) )
                errorMessages = errorMessages.Extend( Resources.InvalidConstructSymbol( g.Key ) );

            var firstConstruct = g.First().Construct;

            var definition = firstConstruct is MathExpressionTypeConverter
                ? CreateTypeConverterDefinition( g, prefixTypeConverters, postfixTypeConverters, ref errorMessages )
                : CreateOperatorDefinition( g, binaryOperators, prefixUnaryOperators, postfixUnaryOperators, ref errorMessages );

            constructDefinitions.Add( g.Key, definition );
        }

        if ( errorMessages.Count > 0 )
            throw new MathExpressionFactoryBuilderException( errorMessages );

        return new MathExpressionFactory( configuration, _numberParserProvider );
    }

    private MathExpressionConstructTokenDefinition CreateOperatorDefinition(
        IGrouping<StringSlice, (StringSlice Symbol, ConstructType Type, IMathExpressionConstruct Construct)> group,
        List<MathExpressionBinaryOperator> binaryBuffer,
        List<MathExpressionUnaryOperator> prefixUnaryBuffer,
        List<MathExpressionUnaryOperator> postfixUnaryBuffer,
        ref Chain<string> errorMessages)
    {
        string? definitionErrorMessage = null;

        foreach ( var (_, type, construct) in group )
        {
            if ( construct is MathExpressionBinaryOperator binaryOperator )
            {
                binaryBuffer.Add( binaryOperator );
                continue;
            }

            if ( construct is MathExpressionUnaryOperator unaryOperator )
            {
                var targetBuffer = type == ConstructType.PrefixUnaryConstruct ? prefixUnaryBuffer : postfixUnaryBuffer;
                targetBuffer.Add( unaryOperator );
                continue;
            }

            definitionErrorMessage ??= Resources.OperatorGroupContainsConstructsOfOtherType( group.Key );
        }

        if ( definitionErrorMessage is not null )
            errorMessages = errorMessages.Extend( definitionErrorMessage );

        var binaryCollection = CreateBinaryOperatorCollection( group.Key, binaryBuffer, ref errorMessages );

        var prefixCollection = CreateUnaryOperatorCollection(
            group.Key,
            prefixUnaryBuffer,
            ConstructType.PrefixUnaryConstruct,
            ref errorMessages );

        var postfixCollection = CreateUnaryOperatorCollection(
            group.Key,
            postfixUnaryBuffer,
            ConstructType.PostfixUnaryConstruct,
            ref errorMessages );

        binaryBuffer.Clear();
        prefixUnaryBuffer.Clear();
        postfixUnaryBuffer.Clear();
        return MathExpressionConstructTokenDefinition.CreateOperator( binaryCollection, prefixCollection, postfixCollection );
    }

    private MathExpressionBinaryOperatorCollection CreateBinaryOperatorCollection(
        StringSlice symbol,
        List<MathExpressionBinaryOperator> buffer,
        ref Chain<string> errorMessages)
    {
        if ( buffer.Count == 0 )
            return MathExpressionBinaryOperatorCollection.Empty;

        MathExpressionBinaryOperator? genericConstruct = null;
        Dictionary<(Type Left, Type Right), MathExpressionTypedBinaryOperator>? specializedConstructs = null;
        string? duplicateGenericConstructErrorMessage = null;

        foreach ( var @operator in buffer )
        {
            if ( @operator is MathExpressionTypedBinaryOperator typedOperator )
            {
                var specializedKey = (typedOperator.LeftArgumentType, typedOperator.RightArgumentType);
                specializedConstructs ??= new Dictionary<(Type, Type), MathExpressionTypedBinaryOperator>();
                if ( ! specializedConstructs.TryAdd( specializedKey, typedOperator ) )
                    errorMessages = errorMessages.Extend( Resources.FoundDuplicateTypedBinaryOperator( symbol, typedOperator ) );

                continue;
            }

            if ( genericConstruct is not null )
            {
                duplicateGenericConstructErrorMessage ??= Resources.FoundDuplicateGenericBinaryOperator( symbol );
                continue;
            }

            genericConstruct = @operator;
        }

        if ( duplicateGenericConstructErrorMessage is not null )
            errorMessages = errorMessages.Extend( duplicateGenericConstructErrorMessage );

        if ( _precedences.TryGetValue( (symbol, ConstructType.BinaryOperator), out var precedence ) )
            return new MathExpressionBinaryOperatorCollection( genericConstruct, specializedConstructs, precedence );

        errorMessages = errorMessages.Extend( Resources.UndefinedBinaryOperatorPrecedence( symbol ) );
        return new MathExpressionBinaryOperatorCollection( genericConstruct, specializedConstructs, int.MaxValue );
    }

    private MathExpressionUnaryOperatorCollection CreateUnaryOperatorCollection(
        StringSlice symbol,
        List<MathExpressionUnaryOperator> buffer,
        ConstructType type,
        ref Chain<string> errorMessages)
    {
        Debug.Assert( type != ConstructType.BinaryOperator, "Type should not be BinaryOperator." );

        if ( buffer.Count == 0 )
            return MathExpressionUnaryOperatorCollection.Empty;

        MathExpressionUnaryOperator? genericConstruct = null;
        Dictionary<Type, MathExpressionTypedUnaryOperator>? specializedConstructs = null;
        string? duplicateGenericConstructErrorMessage = null;

        foreach ( var @operator in buffer )
        {
            if ( @operator is MathExpressionTypedUnaryOperator typedOperator )
            {
                specializedConstructs ??= new Dictionary<Type, MathExpressionTypedUnaryOperator>();
                if ( ! specializedConstructs.TryAdd( typedOperator.ArgumentType, typedOperator ) )
                    errorMessages = errorMessages.Extend( Resources.FoundDuplicateTypedUnaryOperator( symbol, typedOperator, type ) );

                continue;
            }

            if ( genericConstruct is not null )
            {
                duplicateGenericConstructErrorMessage ??= Resources.FoundDuplicateGenericUnaryOperator( symbol, type );
                continue;
            }

            genericConstruct = @operator;
        }

        if ( duplicateGenericConstructErrorMessage is not null )
            errorMessages = errorMessages.Extend( duplicateGenericConstructErrorMessage );

        if ( _precedences.TryGetValue( (symbol, type), out var precedence ) )
            return new MathExpressionUnaryOperatorCollection( genericConstruct, specializedConstructs, precedence );

        errorMessages = errorMessages.Extend( Resources.UndefinedUnaryOperatorPrecedence( symbol, type ) );
        return new MathExpressionUnaryOperatorCollection( genericConstruct, specializedConstructs, int.MaxValue );
    }

    private MathExpressionConstructTokenDefinition CreateTypeConverterDefinition(
        IGrouping<StringSlice, (StringSlice Symbol, ConstructType Type, IMathExpressionConstruct Construct)> group,
        List<MathExpressionTypeConverter> prefixBuffer,
        List<MathExpressionTypeConverter> postfixBuffer,
        ref Chain<string> errorMessages)
    {
        string? definitionErrorMessage = null;

        foreach ( var (_, type, construct) in group )
        {
            if ( construct is MathExpressionTypeConverter converter )
            {
                var targetBuffer = type == ConstructType.PrefixUnaryConstruct ? prefixBuffer : postfixBuffer;
                targetBuffer.Add( converter );
                continue;
            }

            definitionErrorMessage ??= Resources.TypeConverterGroupContainsConstructsOfOtherType( group.Key );
        }

        if ( definitionErrorMessage is not null )
            errorMessages = errorMessages.Extend( definitionErrorMessage );

        var prefixCollection = CreateTypeConverterCollection(
            group.Key,
            prefixBuffer,
            ConstructType.PrefixUnaryConstruct,
            ref errorMessages );

        var postfixCollection = CreateTypeConverterCollection(
            group.Key,
            postfixBuffer,
            ConstructType.PostfixUnaryConstruct,
            ref errorMessages );

        prefixBuffer.Clear();
        postfixBuffer.Clear();

        if ( prefixCollection.TargetType is not null &&
            postfixCollection.TargetType is not null &&
            prefixCollection.TargetType != postfixCollection.TargetType )
        {
            errorMessages = errorMessages.Extend(
                Resources.TypeConverterCollectionsDoNotHaveTheSameTargetType(
                    group.Key,
                    prefixCollection.TargetType,
                    postfixCollection.TargetType ) );
        }

        return MathExpressionConstructTokenDefinition.CreateTypeConverter( prefixCollection, postfixCollection );
    }

    private MathExpressionTypeConverterCollection CreateTypeConverterCollection(
        StringSlice symbol,
        List<MathExpressionTypeConverter> buffer,
        ConstructType type,
        ref Chain<string> errorMessages)
    {
        Debug.Assert( type != ConstructType.BinaryOperator, "Type should not be BinaryOperator." );

        if ( buffer.Count == 0 )
            return MathExpressionTypeConverterCollection.Empty;

        Type? targetType = null;
        MathExpressionTypeConverter? genericConstruct = null;
        Dictionary<Type, MathExpressionTypeConverter>? specializedConstructs = null;
        string? duplicateGenericConstructErrorMessage = null;
        string? invalidTargetTypeErrorMessage = null;

        foreach ( var converter in buffer )
        {
            if ( converter.SourceType is null )
            {
                if ( genericConstruct is not null )
                {
                    duplicateGenericConstructErrorMessage ??= Resources.FoundDuplicateGenericTypeConverter( symbol, type );
                    continue;
                }

                genericConstruct = converter;
            }
            else
            {
                specializedConstructs ??= new Dictionary<Type, MathExpressionTypeConverter>();
                if ( ! specializedConstructs.TryAdd( converter.SourceType, converter ) )
                    errorMessages = errorMessages.Extend( Resources.FoundDuplicateTypedTypeConverter( symbol, converter, type ) );
            }

            if ( targetType is null )
            {
                targetType = converter.TargetType;
                continue;
            }

            if ( converter.TargetType != targetType )
                invalidTargetTypeErrorMessage ??= Resources.NotAllTypeConvertersHaveTheSameTargetType( symbol, targetType, type );
        }

        if ( duplicateGenericConstructErrorMessage is not null )
            errorMessages = errorMessages.Extend( duplicateGenericConstructErrorMessage );

        if ( invalidTargetTypeErrorMessage is not null )
            errorMessages = errorMessages.Extend( invalidTargetTypeErrorMessage );

        if ( _precedences.TryGetValue( (symbol, type), out var precedence ) )
            return new MathExpressionTypeConverterCollection( targetType, genericConstruct, specializedConstructs, precedence );

        errorMessages = errorMessages.Extend( Resources.UndefinedTypeConverterPrecedence( symbol, type ) );
        return new MathExpressionTypeConverterCollection( targetType, genericConstruct, specializedConstructs, int.MaxValue );
    }

    internal enum ConstructType : byte
    {
        BinaryOperator = 0,
        PrefixUnaryConstruct = 1,
        PostfixUnaryConstruct = 2
    }
}
