using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpressionFactoryBuilder
{
    private readonly List<(StringSlice Symbol, ParsedExpressionConstructType Type, object Construct)> _constructs;
    private readonly Dictionary<(StringSlice Symbol, ParsedExpressionConstructType Type), int> _precedences;
    private IParsedExpressionFactoryConfiguration? _configuration;
    private Func<ParsedExpressionNumberParserParams, IParsedExpressionNumberParser>? _numberParserProvider;
    private Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? _memberAccessProvider;
    private Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? _indexerCallProvider;
    private Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? _methodCallProvider;
    private Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? _makeArrayProvider;
    private Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? _invokeProvider;

    public ParsedExpressionFactoryBuilder()
    {
        _constructs = new List<(StringSlice, ParsedExpressionConstructType, object)>();
        _precedences = new Dictionary<(StringSlice, ParsedExpressionConstructType), int>();
        _configuration = null;
        _numberParserProvider = null;
        _memberAccessProvider = null;
        _indexerCallProvider = null;
        _methodCallProvider = null;
        _makeArrayProvider = null;
        _invokeProvider = null;
    }

    public ParsedExpressionFactoryBuilder SetDefaultConfiguration()
    {
        _configuration = null;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetConfiguration(IParsedExpressionFactoryConfiguration configuration)
    {
        _configuration = configuration;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetDefaultNumberParserProvider()
    {
        _numberParserProvider = null;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetNumberParserProvider(
        Func<ParsedExpressionNumberParserParams, IParsedExpressionNumberParser> numberParserProvider)
    {
        _numberParserProvider = numberParserProvider;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetDefaultMemberAccessProvider()
    {
        _memberAccessProvider = null;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetMemberAccessProvider(
        Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction> memberAccessProvider)
    {
        _memberAccessProvider = memberAccessProvider;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetDefaultIndexerCallProvider()
    {
        _indexerCallProvider = null;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetIndexerCallProvider(
        Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction> indexerCallProvider)
    {
        _indexerCallProvider = indexerCallProvider;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetDefaultMethodCallProvider()
    {
        _methodCallProvider = null;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetMethodCallProvider(
        Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction> methodCallProvider)
    {
        _methodCallProvider = methodCallProvider;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetDefaultMakeArrayProvider()
    {
        _makeArrayProvider = null;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetMakeArrayProvider(
        Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction> makeArrayProvider)
    {
        _makeArrayProvider = makeArrayProvider;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetDefaultInvokeProvider()
    {
        _invokeProvider = null;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetInvokeProvider(
        Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction> invokeProvider)
    {
        _invokeProvider = invokeProvider;
        return this;
    }

    public ParsedExpressionFactoryBuilder AddBinaryOperator(string symbol, ParsedExpressionBinaryOperator @operator)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.BinaryOperator, @operator) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddBinaryOperator(ReadOnlyMemory<char> symbol, ParsedExpressionBinaryOperator @operator)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.BinaryOperator, @operator) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddPrefixUnaryOperator(string symbol, ParsedExpressionUnaryOperator @operator)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.PrefixUnaryOperator, @operator) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddPrefixUnaryOperator(ReadOnlyMemory<char> symbol, ParsedExpressionUnaryOperator @operator)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.PrefixUnaryOperator, @operator) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddPostfixUnaryOperator(string symbol, ParsedExpressionUnaryOperator @operator)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.PostfixUnaryOperator, @operator) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddPostfixUnaryOperator(ReadOnlyMemory<char> symbol, ParsedExpressionUnaryOperator @operator)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.PostfixUnaryOperator, @operator) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddPrefixTypeConverter(string symbol, ParsedExpressionTypeConverter converter)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.PrefixTypeConverter, converter) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddPrefixTypeConverter(ReadOnlyMemory<char> symbol, ParsedExpressionTypeConverter converter)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.PrefixTypeConverter, converter) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddPostfixTypeConverter(string symbol, ParsedExpressionTypeConverter converter)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.PostfixTypeConverter, converter) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddPostfixTypeConverter(ReadOnlyMemory<char> symbol, ParsedExpressionTypeConverter converter)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.PostfixTypeConverter, converter) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddConstant(string symbol, ParsedExpressionConstant constant)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.Constant, constant) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddConstant(ReadOnlyMemory<char> symbol, ParsedExpressionConstant constant)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.Constant, constant) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddTypeDeclaration<T>(string name)
    {
        return AddTypeDeclaration( name, typeof( T ) );
    }

    public ParsedExpressionFactoryBuilder AddTypeDeclaration<T>(ReadOnlyMemory<char> name)
    {
        return AddTypeDeclaration( name, typeof( T ) );
    }

    public ParsedExpressionFactoryBuilder AddTypeDeclaration(string name, Type type)
    {
        _constructs.Add( (StringSlice.Create( name ), ParsedExpressionConstructType.TypeDeclaration, type) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddTypeDeclaration(ReadOnlyMemory<char> name, Type type)
    {
        _constructs.Add( (StringSlice.Create( name ), ParsedExpressionConstructType.TypeDeclaration, type) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddFunction(string symbol, ParsedExpressionFunction function)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.Function, function) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddFunction(ReadOnlyMemory<char> symbol, ParsedExpressionFunction function)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.Function, function) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddVariadicFunction(string symbol, ParsedExpressionVariadicFunction function)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.VariadicFunction, function) );
        return this;
    }

    public ParsedExpressionFactoryBuilder AddVariadicFunction(ReadOnlyMemory<char> symbol, ParsedExpressionVariadicFunction function)
    {
        _constructs.Add( (StringSlice.Create( symbol ), ParsedExpressionConstructType.VariadicFunction, function) );
        return this;
    }

    public ParsedExpressionFactoryBuilder SetBinaryOperatorPrecedence(string symbol, int precedence)
    {
        _precedences[(StringSlice.Create( symbol ), ParsedExpressionConstructType.BinaryOperator)] = precedence;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetBinaryOperatorPrecedence(ReadOnlyMemory<char> symbol, int precedence)
    {
        _precedences[(StringSlice.Create( symbol ), ParsedExpressionConstructType.BinaryOperator)] = precedence;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetPrefixUnaryConstructPrecedence(string symbol, int precedence)
    {
        _precedences[(StringSlice.Create( symbol ), ParsedExpressionConstructType.PrefixUnaryConstruct)] = precedence;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetPrefixUnaryConstructPrecedence(ReadOnlyMemory<char> symbol, int precedence)
    {
        _precedences[(StringSlice.Create( symbol ), ParsedExpressionConstructType.PrefixUnaryConstruct)] = precedence;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetPostfixUnaryConstructPrecedence(string symbol, int precedence)
    {
        _precedences[(StringSlice.Create( symbol ), ParsedExpressionConstructType.PostfixUnaryConstruct)] = precedence;
        return this;
    }

    public ParsedExpressionFactoryBuilder SetPostfixUnaryConstructPrecedence(ReadOnlyMemory<char> symbol, int precedence)
    {
        _precedences[(StringSlice.Create( symbol ), ParsedExpressionConstructType.PostfixUnaryConstruct)] = precedence;
        return this;
    }

    [Pure]
    public IParsedExpressionFactoryConfiguration? GetConfiguration()
    {
        return _configuration;
    }

    [Pure]
    public Func<ParsedExpressionNumberParserParams, IParsedExpressionNumberParser>? GetNumberParserProvider()
    {
        return _numberParserProvider;
    }

    [Pure]
    public Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? GetMemberAccessProvider()
    {
        return _memberAccessProvider;
    }

    [Pure]
    public Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? GetIndexerCallProvider()
    {
        return _indexerCallProvider;
    }

    [Pure]
    public Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? GetMethodCallProvider()
    {
        return _methodCallProvider;
    }

    [Pure]
    public Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? GetMakeArrayProvider()
    {
        return _makeArrayProvider;
    }

    [Pure]
    public Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? GetInvokeProvider()
    {
        return _invokeProvider;
    }

    [Pure]
    public IEnumerable<ParsedExpressionConstructInfo> GetConstructs()
    {
        return _constructs.Select( x => new ParsedExpressionConstructInfo( x.Symbol.AsMemory(), x.Type, x.Construct ) );
    }

    [Pure]
    public IEnumerable<KeyValuePair<ReadOnlyMemory<char>, int>> GetBinaryOperatorPrecedences()
    {
        return _precedences
            .Where( kv => kv.Key.Type == ParsedExpressionConstructType.BinaryOperator )
            .Select( kv => KeyValuePair.Create( kv.Key.Symbol.AsMemory(), kv.Value ) );
    }

    [Pure]
    public IEnumerable<KeyValuePair<ReadOnlyMemory<char>, int>> GetPrefixUnaryConstructPrecedences()
    {
        return _precedences
            .Where( kv => kv.Key.Type == ParsedExpressionConstructType.PrefixUnaryConstruct )
            .Select( kv => KeyValuePair.Create( kv.Key.Symbol.AsMemory(), kv.Value ) );
    }

    [Pure]
    public IEnumerable<KeyValuePair<ReadOnlyMemory<char>, int>> GetPostfixUnaryConstructPrecedences()
    {
        return _precedences
            .Where( kv => kv.Key.Type == ParsedExpressionConstructType.PostfixUnaryConstruct )
            .Select( kv => KeyValuePair.Create( kv.Key.Symbol.AsMemory(), kv.Value ) );
    }

    [Pure]
    public ParsedExpressionFactory Build()
    {
        var binaryOperators = new List<ParsedExpressionBinaryOperator>();
        var prefixUnaryOperators = new List<ParsedExpressionUnaryOperator>();
        var postfixUnaryOperators = new List<ParsedExpressionUnaryOperator>();
        var prefixTypeConverters = new List<ParsedExpressionTypeConverter>();
        var postfixTypeConverters = new List<ParsedExpressionTypeConverter>();
        var constructDefinitions = new Dictionary<StringSlice, ConstructTokenDefinition>();

        var configuration = new ParsedExpressionFactoryInternalConfiguration(
            constructDefinitions,
            _configuration ?? new ParsedExpressionFactoryDefaultConfiguration() );

        var errorMessages = configuration.Validate();

        var internalVariadicFunctions = CreateInternalVariadicFunctions( configuration );
        var symbolGroups = _constructs.Concat( internalVariadicFunctions ).GroupBy( c => c.Symbol );

        foreach ( var g in symbolGroups )
        {
            if ( ! TokenValidation.IsValidConstructSymbol( g.Key, configuration.StringDelimiter ) )
                errorMessages = errorMessages.Extend( Resources.InvalidConstructSymbol( g.Key ) );

            var firstConstruct = g.First().Construct;

            var definition = firstConstruct switch
            {
                ParsedExpressionTypeConverter =>
                    CreateTypeConverterDefinition( g, prefixTypeConverters, postfixTypeConverters, ref errorMessages ),
                ParsedExpressionConstant => CreateConstantDefinition( g, ref errorMessages ),
                ParsedExpressionFunction => CreateFunctionDefinition( g, ref errorMessages ),
                ParsedExpressionVariadicFunction => CreateVariadicFunctionDefinition( g, ref errorMessages ),
                Type => CreateTypeDeclarationDefinition( g, ref errorMessages ),
                _ => CreateOperatorDefinition( g, binaryOperators, prefixUnaryOperators, postfixUnaryOperators, ref errorMessages )
            };

            constructDefinitions.Add( g.Key, definition );
        }

        if ( errorMessages.Count > 0 )
            throw new ParsedExpressionFactoryBuilderException( errorMessages );

        return new ParsedExpressionFactory( configuration, _numberParserProvider );
    }

    [Pure]
    private IEnumerable<(StringSlice Symbol, ParsedExpressionConstructType Type, object Construct)> CreateInternalVariadicFunctions(
        ParsedExpressionFactoryInternalConfiguration configuration)
    {
        const ParsedExpressionConstructType type = ParsedExpressionConstructType.VariadicFunction;

        var memberAccess = _memberAccessProvider is null
            ? new ParsedExpressionMemberAccess( configuration )
            : _memberAccessProvider( configuration );

        var indexerCall = _indexerCallProvider is null
            ? new ParsedExpressionIndexerCall( configuration )
            : _indexerCallProvider( configuration );

        var methodCall = _methodCallProvider is null
            ? new ParsedExpressionMethodCall( configuration )
            : _methodCallProvider( configuration );

        var makeArray = _makeArrayProvider is null
            ? new ParsedExpressionMakeArray()
            : _makeArrayProvider( configuration );

        var invoke = _invokeProvider is null
            ? new ParsedExpressionInvoke()
            : _invokeProvider( configuration );

        return new (StringSlice, ParsedExpressionConstructType, object)[]
        {
            (StringSlice.Create( ParsedExpressionConstructDefaults.MemberAccessSymbol ), type, memberAccess),
            (StringSlice.Create( ParsedExpressionConstructDefaults.IndexerCallSymbol ), type, indexerCall),
            (StringSlice.Create( ParsedExpressionConstructDefaults.MethodCallSymbol ), type, methodCall),
            (StringSlice.Create( ParsedExpressionConstructDefaults.MakeArraySymbol ), type, makeArray),
            (StringSlice.Create( ParsedExpressionConstructDefaults.InvokeSymbol ), type, invoke)
        };
    }

    private ConstructTokenDefinition CreateOperatorDefinition(
        IGrouping<StringSlice, (StringSlice Symbol, ParsedExpressionConstructType Type, object Object)> group,
        List<ParsedExpressionBinaryOperator> binaryBuffer,
        List<ParsedExpressionUnaryOperator> prefixUnaryBuffer,
        List<ParsedExpressionUnaryOperator> postfixUnaryBuffer,
        ref Chain<string> errorMessages)
    {
        string? definitionErrorMessage = null;

        foreach ( var (_, type, construct) in group )
        {
            if ( construct is ParsedExpressionBinaryOperator binaryOperator )
            {
                binaryBuffer.Add( binaryOperator );
                continue;
            }

            if ( construct is ParsedExpressionUnaryOperator unaryOperator )
            {
                var targetBuffer = type == ParsedExpressionConstructType.PrefixUnaryOperator ? prefixUnaryBuffer : postfixUnaryBuffer;
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
            ParsedExpressionConstructType.PrefixUnaryOperator,
            ref errorMessages );

        var postfixCollection = CreateUnaryOperatorCollection(
            group.Key,
            postfixUnaryBuffer,
            ParsedExpressionConstructType.PostfixUnaryOperator,
            ref errorMessages );

        binaryBuffer.Clear();
        prefixUnaryBuffer.Clear();
        postfixUnaryBuffer.Clear();
        return ConstructTokenDefinition.CreateOperator( binaryCollection, prefixCollection, postfixCollection );
    }

    private BinaryOperatorCollection CreateBinaryOperatorCollection(
        StringSlice symbol,
        List<ParsedExpressionBinaryOperator> buffer,
        ref Chain<string> errorMessages)
    {
        if ( buffer.Count == 0 )
            return BinaryOperatorCollection.Empty;

        ParsedExpressionBinaryOperator? genericConstruct = null;
        Dictionary<(Type Left, Type Right), ParsedExpressionTypedBinaryOperator>? specializedConstructs = null;
        string? duplicateGenericConstructErrorMessage = null;

        foreach ( var @operator in buffer )
        {
            if ( @operator is ParsedExpressionTypedBinaryOperator typedOperator )
            {
                var specializedKey = (typedOperator.LeftArgumentType, typedOperator.RightArgumentType);
                specializedConstructs ??= new Dictionary<(Type, Type), ParsedExpressionTypedBinaryOperator>();
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

        if ( _precedences.TryGetValue( (symbol, ParsedExpressionConstructType.BinaryOperator), out var precedence ) )
            return new BinaryOperatorCollection( genericConstruct, specializedConstructs, precedence );

        errorMessages = errorMessages.Extend( Resources.UndefinedBinaryOperatorPrecedence( symbol ) );
        return new BinaryOperatorCollection( genericConstruct, specializedConstructs, int.MaxValue );
    }

    private UnaryOperatorCollection CreateUnaryOperatorCollection(
        StringSlice symbol,
        List<ParsedExpressionUnaryOperator> buffer,
        ParsedExpressionConstructType type,
        ref Chain<string> errorMessages)
    {
        Assume.NotEquals( type, ParsedExpressionConstructType.BinaryOperator, nameof( type ) );

        if ( buffer.Count == 0 )
            return UnaryOperatorCollection.Empty;

        ParsedExpressionUnaryOperator? genericConstruct = null;
        Dictionary<Type, ParsedExpressionTypedUnaryOperator>? specializedConstructs = null;
        string? duplicateGenericConstructErrorMessage = null;

        foreach ( var @operator in buffer )
        {
            if ( @operator is ParsedExpressionTypedUnaryOperator typedOperator )
            {
                specializedConstructs ??= new Dictionary<Type, ParsedExpressionTypedUnaryOperator>();
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

        var precedenceType = (type & ParsedExpressionConstructType.PrefixUnaryConstruct) != ParsedExpressionConstructType.None
            ? ParsedExpressionConstructType.PrefixUnaryConstruct
            : ParsedExpressionConstructType.PostfixUnaryConstruct;

        if ( _precedences.TryGetValue( (symbol, precedenceType), out var precedence ) )
            return new UnaryOperatorCollection( genericConstruct, specializedConstructs, precedence );

        errorMessages = errorMessages.Extend( Resources.UndefinedUnaryOperatorPrecedence( symbol, type ) );
        return new UnaryOperatorCollection( genericConstruct, specializedConstructs, int.MaxValue );
    }

    private ConstructTokenDefinition CreateTypeConverterDefinition(
        IGrouping<StringSlice, (StringSlice Symbol, ParsedExpressionConstructType Type, object Object)> group,
        List<ParsedExpressionTypeConverter> prefixBuffer,
        List<ParsedExpressionTypeConverter> postfixBuffer,
        ref Chain<string> errorMessages)
    {
        string? definitionErrorMessage = null;

        foreach ( var (_, type, construct) in group )
        {
            if ( construct is ParsedExpressionTypeConverter converter )
            {
                var targetBuffer = type == ParsedExpressionConstructType.PrefixTypeConverter ? prefixBuffer : postfixBuffer;
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
            ParsedExpressionConstructType.PrefixTypeConverter,
            ref errorMessages );

        var postfixCollection = CreateTypeConverterCollection(
            group.Key,
            postfixBuffer,
            ParsedExpressionConstructType.PostfixTypeConverter,
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

        return ConstructTokenDefinition.CreateTypeConverter( prefixCollection, postfixCollection );
    }

    private TypeConverterCollection CreateTypeConverterCollection(
        StringSlice symbol,
        List<ParsedExpressionTypeConverter> buffer,
        ParsedExpressionConstructType type,
        ref Chain<string> errorMessages)
    {
        Assume.NotEquals( type, ParsedExpressionConstructType.BinaryOperator, nameof( type ) );

        if ( buffer.Count == 0 )
            return TypeConverterCollection.Empty;

        Type? targetType = null;
        ParsedExpressionTypeConverter? genericConstruct = null;
        Dictionary<Type, ParsedExpressionTypeConverter>? specializedConstructs = null;
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
                specializedConstructs ??= new Dictionary<Type, ParsedExpressionTypeConverter>();
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

        var precedenceType = (type & ParsedExpressionConstructType.PrefixUnaryConstruct) != ParsedExpressionConstructType.None
            ? ParsedExpressionConstructType.PrefixUnaryConstruct
            : ParsedExpressionConstructType.PostfixUnaryConstruct;

        if ( _precedences.TryGetValue( (symbol, precedenceType), out var precedence ) )
            return new TypeConverterCollection( targetType, genericConstruct, specializedConstructs, precedence );

        errorMessages = errorMessages.Extend( Resources.UndefinedTypeConverterPrecedence( symbol, type ) );
        return new TypeConverterCollection( targetType, genericConstruct, specializedConstructs, int.MaxValue );
    }

    private static ConstructTokenDefinition CreateConstantDefinition(
        IGrouping<StringSlice, (StringSlice Symbol, ParsedExpressionConstructType Type, object Object)> group,
        ref Chain<string> errorMessages)
    {
        ParsedExpressionConstant? result = null;
        string? definitionErrorMessage = null;

        foreach ( var (_, _, construct) in group )
        {
            if ( construct is ParsedExpressionConstant constant )
            {
                if ( result is not null )
                {
                    errorMessages = errorMessages.Extend( Resources.ConstantGroupContainsMoreThanOneConstant( group.Key ) );
                    break;
                }

                result = constant;
                continue;
            }

            definitionErrorMessage ??= Resources.ConstantGroupContainsConstructsOfOtherType( group.Key );
        }

        if ( definitionErrorMessage is not null )
            errorMessages = errorMessages.Extend( definitionErrorMessage );

        return ConstructTokenDefinition.CreateConstant( result );
    }

    private static ConstructTokenDefinition CreateTypeDeclarationDefinition(
        IGrouping<StringSlice, (StringSlice Symbol, ParsedExpressionConstructType Type, object Object)> group,
        ref Chain<string> errorMessages)
    {
        Type? result = null;
        string? definitionErrorMessage = null;

        foreach ( var (_, _, construct) in group )
        {
            if ( construct is Type type )
            {
                if ( result is not null )
                {
                    errorMessages = errorMessages.Extend( Resources.TypeDeclarationGroupContainsMoreThanOneType( group.Key ) );
                    break;
                }

                result = type;
                continue;
            }

            definitionErrorMessage ??= Resources.TypeDeclarationGroupContainsConstructsOfOtherType( group.Key );
        }

        if ( definitionErrorMessage is not null )
            errorMessages = errorMessages.Extend( definitionErrorMessage );

        return ConstructTokenDefinition.CreateTypeDeclaration( result );
    }

    private static ConstructTokenDefinition CreateFunctionDefinition(
        IGrouping<StringSlice, (StringSlice Symbol, ParsedExpressionConstructType Type, object Object)> group,
        ref Chain<string> errorMessages)
    {
        Dictionary<FunctionSignatureKey, ParsedExpressionFunction>? functions = null;
        string? definitionErrorMessage = null;

        foreach ( var (_, _, construct) in group )
        {
            if ( construct is ParsedExpressionFunction function )
            {
                var parameters = function.Lambda.Parameters;
                functions ??= new Dictionary<FunctionSignatureKey, ParsedExpressionFunction>();
                if ( ! functions.TryAdd( new FunctionSignatureKey( parameters ), function ) )
                    errorMessages = errorMessages.Extend( Resources.FoundDuplicateFunctionSignature( group.Key, parameters ) );

                continue;
            }

            definitionErrorMessage ??= Resources.FunctionGroupContainsConstructsOfOtherType( group.Key );
        }

        if ( definitionErrorMessage is not null )
            errorMessages = errorMessages.Extend( definitionErrorMessage );

        var collection = functions is null ? FunctionCollection.Empty : new FunctionCollection( functions );
        return ConstructTokenDefinition.CreateFunction( collection );
    }

    private static ConstructTokenDefinition CreateVariadicFunctionDefinition(
        IGrouping<StringSlice, (StringSlice Symbol, ParsedExpressionConstructType Type, object Object)> group,
        ref Chain<string> errorMessages)
    {
        ParsedExpressionVariadicFunction? result = null;
        string? definitionErrorMessage = null;

        foreach ( var (_, _, construct) in group )
        {
            if ( construct is ParsedExpressionVariadicFunction function )
            {
                if ( result is not null )
                {
                    errorMessages = errorMessages.Extend( Resources.VariadicFunctionGroupContainsMoreThanOneFunction( group.Key ) );
                    break;
                }

                result = function;
                continue;
            }

            definitionErrorMessage ??= Resources.VariadicFunctionGroupContainsConstructsOfOtherType( group.Key );
        }

        if ( definitionErrorMessage is not null )
            errorMessages = errorMessages.Extend( definitionErrorMessage );

        return ConstructTokenDefinition.CreateVariadicFunction( result );
    }
}
