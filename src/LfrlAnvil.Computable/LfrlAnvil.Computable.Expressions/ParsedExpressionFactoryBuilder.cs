using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions;

/// <summary>
/// Represents a builder of <see cref="IParsedExpressionFactory"/> instances.
/// </summary>
public sealed class ParsedExpressionFactoryBuilder
{
    private readonly List<(StringSegment Symbol, ParsedExpressionConstructType Type, object Construct)> _constructs;
    private readonly Dictionary<(StringSegment Symbol, ParsedExpressionConstructType Type), int> _precedences;
    private IParsedExpressionFactoryConfiguration? _configuration;
    private Func<ParsedExpressionNumberParserParams, IParsedExpressionNumberParser>? _numberParserProvider;
    private Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? _memberAccessProvider;
    private Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? _indexerCallProvider;
    private Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? _methodCallProvider;
    private Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? _ctorCallProvider;
    private Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? _makeArrayProvider;
    private Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? _invokeProvider;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFactoryBuilder"/> instance.
    /// </summary>
    public ParsedExpressionFactoryBuilder()
    {
        _constructs = new List<(StringSegment, ParsedExpressionConstructType, object)>();
        _precedences = new Dictionary<(StringSegment, ParsedExpressionConstructType), int>();
        _configuration = null;
        _numberParserProvider = null;
        _memberAccessProvider = null;
        _indexerCallProvider = null;
        _methodCallProvider = null;
        _ctorCallProvider = null;
        _makeArrayProvider = null;
        _invokeProvider = null;
    }

    /// <summary>
    /// Sets the <see cref="IParsedExpressionFactoryConfiguration"/> instance to default value.
    /// </summary>
    /// <returns><b>this</b>.</returns>
    /// <remarks>See <see cref="ParsedExpressionFactoryDefaultConfiguration"/> for more information.</remarks>
    public ParsedExpressionFactoryBuilder SetDefaultConfiguration()
    {
        _configuration = null;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="IParsedExpressionFactoryConfiguration"/> instance to the provided value.
    /// </summary>
    /// <param name="configuration">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder SetConfiguration(IParsedExpressionFactoryConfiguration configuration)
    {
        _configuration = configuration;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="IParsedExpressionNumberParser"/> provider instance to default value.
    /// </summary>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Default provider will parse all numbers as <see cref="Decimal"/> type.</remarks>
    public ParsedExpressionFactoryBuilder SetDefaultNumberParserProvider()
    {
        _numberParserProvider = null;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="IParsedExpressionNumberParser"/> provider instance to the provided value.
    /// </summary>
    /// <returns><b>this</b>.</returns>
    /// <param name="numberParserProvider">Value to set.</param>
    public ParsedExpressionFactoryBuilder SetNumberParserProvider(
        Func<ParsedExpressionNumberParserParams, IParsedExpressionNumberParser> numberParserProvider)
    {
        _numberParserProvider = numberParserProvider;
        return this;
    }

    /// <summary>
    /// Sets the member access provider instance to default value.
    /// </summary>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Default provider will use the <see cref="ParsedExpressionMemberAccess"/> construct.</remarks>
    public ParsedExpressionFactoryBuilder SetDefaultMemberAccessProvider()
    {
        _memberAccessProvider = null;
        return this;
    }

    /// <summary>
    /// Sets the member access provider instance to the provided value.
    /// </summary>
    /// <param name="memberAccessProvider">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder SetMemberAccessProvider(
        Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction> memberAccessProvider)
    {
        _memberAccessProvider = memberAccessProvider;
        return this;
    }

    /// <summary>
    /// Sets the indexer call provider instance to default value.
    /// </summary>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Default provider will use the <see cref="ParsedExpressionIndexerCall"/> construct.</remarks>
    public ParsedExpressionFactoryBuilder SetDefaultIndexerCallProvider()
    {
        _indexerCallProvider = null;
        return this;
    }

    /// <summary>
    /// Sets the indexer call provider instance to the provided value.
    /// </summary>
    /// <param name="indexerCallProvider">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder SetIndexerCallProvider(
        Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction> indexerCallProvider)
    {
        _indexerCallProvider = indexerCallProvider;
        return this;
    }

    /// <summary>
    /// Sets the method call provider instance to default value.
    /// </summary>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Default provider will use the <see cref="ParsedExpressionMethodCall"/> construct.</remarks>
    public ParsedExpressionFactoryBuilder SetDefaultMethodCallProvider()
    {
        _methodCallProvider = null;
        return this;
    }

    /// <summary>
    /// Sets the method call provider instance to the provided value.
    /// </summary>
    /// <param name="methodCallProvider">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder SetMethodCallProvider(
        Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction> methodCallProvider)
    {
        _methodCallProvider = methodCallProvider;
        return this;
    }

    /// <summary>
    /// Sets the constructor call provider instance to default value.
    /// </summary>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Default provider will use the <see cref="ParsedExpressionConstructorCall"/> construct.</remarks>
    public ParsedExpressionFactoryBuilder SetDefaultCtorCallProvider()
    {
        _ctorCallProvider = null;
        return this;
    }

    /// <summary>
    /// Sets the constructor call provider instance to the provided value.
    /// </summary>
    /// <param name="ctorCallProvider">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder SetCtorCallProvider(
        Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction> ctorCallProvider)
    {
        _ctorCallProvider = ctorCallProvider;
        return this;
    }

    /// <summary>
    /// Sets the make array provider instance to default value.
    /// </summary>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Default provider will use the <see cref="ParsedExpressionMakeArray"/> construct.</remarks>
    public ParsedExpressionFactoryBuilder SetDefaultMakeArrayProvider()
    {
        _makeArrayProvider = null;
        return this;
    }

    /// <summary>
    /// Sets the make array provider instance to the provided value.
    /// </summary>
    /// <param name="makeArrayProvider">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder SetMakeArrayProvider(
        Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction> makeArrayProvider)
    {
        _makeArrayProvider = makeArrayProvider;
        return this;
    }

    /// <summary>
    /// Sets the invoke provider instance to default value.
    /// </summary>
    /// <returns><b>this</b>.</returns>
    /// <remarks>Default provider will use the <see cref="ParsedExpressionInvoke"/> construct.</remarks>
    public ParsedExpressionFactoryBuilder SetDefaultInvokeProvider()
    {
        _invokeProvider = null;
        return this;
    }

    /// <summary>
    /// Sets the invoke provider instance to the provided value.
    /// </summary>
    /// <param name="invokeProvider">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder SetInvokeProvider(
        Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction> invokeProvider)
    {
        _invokeProvider = invokeProvider;
        return this;
    }

    /// <summary>
    /// Adds a binary operator construct.
    /// </summary>
    /// <param name="symbol">Construct's symbol.</param>
    /// <param name="operator">Operator to add.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder AddBinaryOperator(StringSegment symbol, ParsedExpressionBinaryOperator @operator)
    {
        _constructs.Add( (symbol, ParsedExpressionConstructType.BinaryOperator, @operator) );
        return this;
    }

    /// <summary>
    /// Adds a prefix unary operator construct.
    /// </summary>
    /// <param name="symbol">Construct's symbol.</param>
    /// <param name="operator">Operator to add.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder AddPrefixUnaryOperator(StringSegment symbol, ParsedExpressionUnaryOperator @operator)
    {
        _constructs.Add( (symbol, ParsedExpressionConstructType.PrefixUnaryOperator, @operator) );
        return this;
    }

    /// <summary>
    /// Adds a postfix unary operator construct.
    /// </summary>
    /// <param name="symbol">Construct's symbol.</param>
    /// <param name="operator">Operator to add.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder AddPostfixUnaryOperator(StringSegment symbol, ParsedExpressionUnaryOperator @operator)
    {
        _constructs.Add( (symbol, ParsedExpressionConstructType.PostfixUnaryOperator, @operator) );
        return this;
    }

    /// <summary>
    /// Adds a prefix type converter construct.
    /// </summary>
    /// <param name="symbol">Construct's symbol.</param>
    /// <param name="converter">Converter to add.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder AddPrefixTypeConverter(StringSegment symbol, ParsedExpressionTypeConverter converter)
    {
        _constructs.Add( (symbol, ParsedExpressionConstructType.PrefixTypeConverter, converter) );
        return this;
    }

    /// <summary>
    /// Adds a postfix type converter construct.
    /// </summary>
    /// <param name="symbol">Construct's symbol.</param>
    /// <param name="converter">Converter to add.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder AddPostfixTypeConverter(StringSegment symbol, ParsedExpressionTypeConverter converter)
    {
        _constructs.Add( (symbol, ParsedExpressionConstructType.PostfixTypeConverter, converter) );
        return this;
    }

    /// <summary>
    /// Adds a constant construct.
    /// </summary>
    /// <param name="symbol">Construct's symbol.</param>
    /// <param name="constant">Constant to add.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder AddConstant(StringSegment symbol, ParsedExpressionConstant constant)
    {
        _constructs.Add( (symbol, ParsedExpressionConstructType.Constant, constant) );
        return this;
    }

    /// <summary>
    /// Adds a type declaration construct.
    /// </summary>
    /// <param name="name">Construct's symbol.</param>
    /// <typeparam name="T">Type to add.</typeparam>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder AddTypeDeclaration<T>(StringSegment name)
    {
        return AddTypeDeclaration( name, typeof( T ) );
    }

    /// <summary>
    /// Adds a type declaration construct.
    /// </summary>
    /// <param name="name">Construct's symbol.</param>
    /// <param name="type">Type to add.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder AddTypeDeclaration(StringSegment name, Type type)
    {
        _constructs.Add( (name, ParsedExpressionConstructType.TypeDeclaration, type) );
        return this;
    }

    /// <summary>
    /// Adds a function construct.
    /// </summary>
    /// <param name="symbol">Construct's symbol.</param>
    /// <param name="function">Function to add.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder AddFunction(StringSegment symbol, ParsedExpressionFunction function)
    {
        _constructs.Add( (symbol, ParsedExpressionConstructType.Function, function) );
        return this;
    }

    /// <summary>
    /// Adds a variadic function construct.
    /// </summary>
    /// <param name="symbol">Construct's symbol.</param>
    /// <param name="function">Function to add.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder AddVariadicFunction(StringSegment symbol, ParsedExpressionVariadicFunction function)
    {
        _constructs.Add( (symbol, ParsedExpressionConstructType.VariadicFunction, function) );
        return this;
    }

    /// <summary>
    /// Sets binary operator's precedence.
    /// </summary>
    /// <param name="symbol">Construct's symbol.</param>
    /// <param name="precedence">Precedence value to set.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder SetBinaryOperatorPrecedence(StringSegment symbol, int precedence)
    {
        _precedences[(symbol, ParsedExpressionConstructType.BinaryOperator)] = precedence;
        return this;
    }

    /// <summary>
    /// Sets prefix unary construct's precedence.
    /// </summary>
    /// <param name="symbol">Construct's symbol.</param>
    /// <param name="precedence">Precedence value to set.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder SetPrefixUnaryConstructPrecedence(StringSegment symbol, int precedence)
    {
        _precedences[(symbol, ParsedExpressionConstructType.PrefixUnaryConstruct)] = precedence;
        return this;
    }

    /// <summary>
    /// Sets postfix unary construct's precedence.
    /// </summary>
    /// <param name="symbol">Construct's symbol.</param>
    /// <param name="precedence">Precedence value to set.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionFactoryBuilder SetPostfixUnaryConstructPrecedence(StringSegment symbol, int precedence)
    {
        _precedences[(symbol, ParsedExpressionConstructType.PostfixUnaryConstruct)] = precedence;
        return this;
    }

    /// <summary>
    /// Returns the current <see cref="IParsedExpressionFactoryConfiguration"/> instance.
    /// </summary>
    /// <returns>Current <see cref="IParsedExpressionFactoryConfiguration"/> instance.</returns>
    [Pure]
    public IParsedExpressionFactoryConfiguration? GetConfiguration()
    {
        return _configuration;
    }

    /// <summary>
    /// Returns the current <see cref="IParsedExpressionNumberParser"/> provider instance.
    /// </summary>
    /// <returns>Current <see cref="IParsedExpressionNumberParser"/> provider instance.</returns>
    [Pure]
    public Func<ParsedExpressionNumberParserParams, IParsedExpressionNumberParser>? GetNumberParserProvider()
    {
        return _numberParserProvider;
    }

    /// <summary>
    /// Returns the current member access provider instance.
    /// </summary>
    /// <returns>Current member access provider instance.</returns>
    [Pure]
    public Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? GetMemberAccessProvider()
    {
        return _memberAccessProvider;
    }

    /// <summary>
    /// Returns the current indexer call provider instance.
    /// </summary>
    /// <returns>Current indexer call provider instance.</returns>
    [Pure]
    public Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? GetIndexerCallProvider()
    {
        return _indexerCallProvider;
    }

    /// <summary>
    /// Returns the current method call provider instance.
    /// </summary>
    /// <returns>Current method call provider instance.</returns>
    [Pure]
    public Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? GetMethodCallProvider()
    {
        return _methodCallProvider;
    }

    /// <summary>
    /// Returns the current constructor call provider instance.
    /// </summary>
    /// <returns>Current constructor call provider instance.</returns>
    [Pure]
    public Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? GetCtorCallProvider()
    {
        return _ctorCallProvider;
    }

    /// <summary>
    /// Returns the current make array provider instance.
    /// </summary>
    /// <returns>Current make array provider instance.</returns>
    [Pure]
    public Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? GetMakeArrayProvider()
    {
        return _makeArrayProvider;
    }

    /// <summary>
    /// Returns the current invoke provider instance.
    /// </summary>
    /// <returns>Current invoke provider instance.</returns>
    [Pure]
    public Func<ParsedExpressionFactoryInternalConfiguration, ParsedExpressionVariadicFunction>? GetInvokeProvider()
    {
        return _invokeProvider;
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains information about all registered constructs.
    /// </summary>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public IEnumerable<ParsedExpressionConstructInfo> GetConstructs()
    {
        return _constructs.Select( x => new ParsedExpressionConstructInfo( x.Symbol, x.Type, x.Construct ) );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains information about all registered binary operator precedences.
    /// </summary>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public IEnumerable<KeyValuePair<StringSegment, int>> GetBinaryOperatorPrecedences()
    {
        return _precedences
            .Where( kv => kv.Key.Type == ParsedExpressionConstructType.BinaryOperator )
            .Select( kv => KeyValuePair.Create( kv.Key.Symbol, kv.Value ) );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains information about
    /// all registered prefix unary construct precedences.
    /// </summary>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public IEnumerable<KeyValuePair<StringSegment, int>> GetPrefixUnaryConstructPrecedences()
    {
        return _precedences
            .Where( kv => kv.Key.Type == ParsedExpressionConstructType.PrefixUnaryConstruct )
            .Select( kv => KeyValuePair.Create( kv.Key.Symbol, kv.Value ) );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains information about
    /// all registered postfix unary construct precedences.
    /// </summary>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public IEnumerable<KeyValuePair<StringSegment, int>> GetPostfixUnaryConstructPrecedences()
    {
        return _precedences
            .Where( kv => kv.Key.Type == ParsedExpressionConstructType.PostfixUnaryConstruct )
            .Select( kv => KeyValuePair.Create( kv.Key.Symbol, kv.Value ) );
    }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFactory"/> instance.
    /// </summary>
    /// <returns>New <see cref="ParsedExpressionFactory"/> instance.</returns>
    /// <exception cref="ParsedExpressionFactoryBuilderException">When configuration is invalid.</exception>
    [Pure]
    public ParsedExpressionFactory Build()
    {
        var binaryOperators = new List<ParsedExpressionBinaryOperator>();
        var prefixUnaryOperators = new List<ParsedExpressionUnaryOperator>();
        var postfixUnaryOperators = new List<ParsedExpressionUnaryOperator>();
        var prefixTypeConverters = new List<ParsedExpressionTypeConverter>();
        var postfixTypeConverters = new List<ParsedExpressionTypeConverter>();
        var constructDefinitions = new Dictionary<StringSegment, ConstructTokenDefinition>();

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
    private IEnumerable<(StringSegment Symbol, ParsedExpressionConstructType Type, object Construct)> CreateInternalVariadicFunctions(
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

        var ctorCall = _ctorCallProvider is null
            ? new ParsedExpressionConstructorCall( configuration )
            : _ctorCallProvider( configuration );

        var makeArray = _makeArrayProvider is null
            ? new ParsedExpressionMakeArray()
            : _makeArrayProvider( configuration );

        var invoke = _invokeProvider is null
            ? new ParsedExpressionInvoke()
            : _invokeProvider( configuration );

        return new (StringSegment, ParsedExpressionConstructType, object)[]
        {
            (new StringSegment( ParsedExpressionConstructDefaults.MemberAccessSymbol ), type, memberAccess),
            (new StringSegment( ParsedExpressionConstructDefaults.IndexerCallSymbol ), type, indexerCall),
            (new StringSegment( ParsedExpressionConstructDefaults.MethodCallSymbol ), type, methodCall),
            (new StringSegment( ParsedExpressionConstructDefaults.CtorCallSymbol ), type, ctorCall),
            (new StringSegment( ParsedExpressionConstructDefaults.MakeArraySymbol ), type, makeArray),
            (new StringSegment( ParsedExpressionConstructDefaults.InvokeSymbol ), type, invoke)
        };
    }

    private ConstructTokenDefinition CreateOperatorDefinition(
        IGrouping<StringSegment, (StringSegment Symbol, ParsedExpressionConstructType Type, object Object)> group,
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
        StringSegment symbol,
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
        StringSegment symbol,
        List<ParsedExpressionUnaryOperator> buffer,
        ParsedExpressionConstructType type,
        ref Chain<string> errorMessages)
    {
        Assume.NotEquals( type, ParsedExpressionConstructType.BinaryOperator );

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
        IGrouping<StringSegment, (StringSegment Symbol, ParsedExpressionConstructType Type, object Object)> group,
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

        if ( prefixCollection.TargetType is not null
            && postfixCollection.TargetType is not null
            && prefixCollection.TargetType != postfixCollection.TargetType )
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
        StringSegment symbol,
        List<ParsedExpressionTypeConverter> buffer,
        ParsedExpressionConstructType type,
        ref Chain<string> errorMessages)
    {
        Assume.NotEquals( type, ParsedExpressionConstructType.BinaryOperator );

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
        IGrouping<StringSegment, (StringSegment Symbol, ParsedExpressionConstructType Type, object Object)> group,
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
        IGrouping<StringSegment, (StringSegment Symbol, ParsedExpressionConstructType Type, object Object)> group,
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
        IGrouping<StringSegment, (StringSegment Symbol, ParsedExpressionConstructType Type, object Object)> group,
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
        IGrouping<StringSegment, (StringSegment Symbol, ParsedExpressionConstructType Type, object Object)> group,
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
