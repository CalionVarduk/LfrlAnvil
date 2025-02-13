using System.Linq;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Constructs.Boolean;
using LfrlAnvil.Computable.Expressions.Constructs.Decimal;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionFactoryBuilderTests;

public class ParsedExpressionFactoryBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyBuilder()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        Assertion.All(
                sut.GetConfiguration().TestNull(),
                sut.GetNumberParserProvider().TestNull(),
                sut.GetConstructs().TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetConfiguration_ShouldUpdateConfigurationToNewObject()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var configuration = new ParsedExpressionFactoryDefaultConfiguration();

        var result = sut.SetConfiguration( configuration );

        Assertion.All(
                sut.GetConfiguration().TestRefEquals( configuration ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetDefaultConfiguration_ShouldUpdateConfigurationToNull()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var configuration = new ParsedExpressionFactoryDefaultConfiguration();
        sut.SetConfiguration( configuration );

        var result = sut.SetDefaultConfiguration();

        Assertion.All(
                sut.GetConfiguration().TestNull(),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetNumberParserProvider_ShouldUpdateDelegateToNewObject()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of(
            (ParsedExpressionNumberParserParams p) => ParsedExpressionNumberParser.CreateDefaultDecimal( p.Configuration ) );

        var result = sut.SetNumberParserProvider( @delegate );

        Assertion.All(
                sut.GetNumberParserProvider().TestRefEquals( @delegate ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetDefaultNumberParserProvider_ShouldUpdateDelegateToNull()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of(
            (ParsedExpressionNumberParserParams p) => ParsedExpressionNumberParser.CreateDefaultDecimal( p.Configuration ) );

        sut.SetNumberParserProvider( @delegate );

        var result = sut.SetDefaultNumberParserProvider();

        Assertion.All(
                sut.GetNumberParserProvider().TestNull(),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetMemberAccessProvider_ShouldUpdateDelegateToNewObject()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of( (ParsedExpressionFactoryInternalConfiguration c) => new ParsedExpressionMemberAccess( c ) );

        var result = sut.SetMemberAccessProvider( @delegate );

        Assertion.All(
                sut.GetMemberAccessProvider().TestRefEquals( @delegate ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetDefaultMemberAccessProvider_ShouldUpdateDelegateToNull()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of( (ParsedExpressionFactoryInternalConfiguration c) => new ParsedExpressionMemberAccess( c ) );
        sut.SetMemberAccessProvider( @delegate );

        var result = sut.SetDefaultMemberAccessProvider();

        Assertion.All(
                sut.GetMemberAccessProvider().TestNull(),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetIndexerCallProvider_ShouldUpdateDelegateToNewObject()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of( (ParsedExpressionFactoryInternalConfiguration c) => new ParsedExpressionMemberAccess( c ) );

        var result = sut.SetIndexerCallProvider( @delegate );

        Assertion.All(
                sut.GetIndexerCallProvider().TestRefEquals( @delegate ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetDefaultIndexerCallProvider_ShouldUpdateDelegateToNull()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of( (ParsedExpressionFactoryInternalConfiguration c) => new ParsedExpressionMemberAccess( c ) );
        sut.SetIndexerCallProvider( @delegate );

        var result = sut.SetDefaultIndexerCallProvider();

        Assertion.All(
                sut.GetIndexerCallProvider().TestNull(),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetMethodCallProvider_ShouldUpdateDelegateToNewObject()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of( (ParsedExpressionFactoryInternalConfiguration c) => new ParsedExpressionMemberAccess( c ) );

        var result = sut.SetMethodCallProvider( @delegate );

        Assertion.All(
                sut.GetMethodCallProvider().TestRefEquals( @delegate ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetDefaultMethodCallProvider_ShouldUpdateDelegateToNull()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of( (ParsedExpressionFactoryInternalConfiguration c) => new ParsedExpressionMemberAccess( c ) );
        sut.SetMethodCallProvider( @delegate );

        var result = sut.SetDefaultMethodCallProvider();

        Assertion.All(
                sut.GetMethodCallProvider().TestNull(),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetCtorCallProvider_ShouldUpdateDelegateToNewObject()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of( (ParsedExpressionFactoryInternalConfiguration c) => new ParsedExpressionMemberAccess( c ) );

        var result = sut.SetCtorCallProvider( @delegate );

        Assertion.All(
                sut.GetCtorCallProvider().TestRefEquals( @delegate ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetDefaultCtorCallProvider_ShouldUpdateDelegateToNull()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of( (ParsedExpressionFactoryInternalConfiguration c) => new ParsedExpressionMemberAccess( c ) );
        sut.SetCtorCallProvider( @delegate );

        var result = sut.SetDefaultCtorCallProvider();

        Assertion.All(
                sut.GetCtorCallProvider().TestNull(),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetMakeArrayProvider_ShouldUpdateDelegateToNewObject()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of( (ParsedExpressionFactoryInternalConfiguration c) => new ParsedExpressionMemberAccess( c ) );

        var result = sut.SetMakeArrayProvider( @delegate );

        Assertion.All(
                sut.GetMakeArrayProvider().TestRefEquals( @delegate ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetDefaultMakeArrayProvider_ShouldUpdateDelegateToNull()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of( (ParsedExpressionFactoryInternalConfiguration c) => new ParsedExpressionMemberAccess( c ) );
        sut.SetMakeArrayProvider( @delegate );

        var result = sut.SetDefaultMakeArrayProvider();

        Assertion.All(
                sut.GetMakeArrayProvider().TestNull(),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetInvokeProvider_ShouldUpdateDelegateToNewObject()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of( (ParsedExpressionFactoryInternalConfiguration c) => new ParsedExpressionMemberAccess( c ) );

        var result = sut.SetInvokeProvider( @delegate );

        Assertion.All(
                sut.GetInvokeProvider().TestRefEquals( @delegate ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetDefaultInvokeProvider_ShouldUpdateDelegateToNull()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of( (ParsedExpressionFactoryInternalConfiguration c) => new ParsedExpressionMemberAccess( c ) );
        sut.SetInvokeProvider( @delegate );

        var result = sut.SetDefaultInvokeProvider();

        Assertion.All(
                sut.GetInvokeProvider().TestNull(),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void AddBinaryOperator_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var @operator = new ParsedExpressionAddOperator();

        var result = sut.AddBinaryOperator( symbol, @operator );

        Assertion.All(
                sut.GetConstructs().Count().TestEquals( 1 ),
                sut.GetConstructs().FirstOrDefault().Symbol.ToString().TestEquals( symbol ),
                sut.GetConstructs().FirstOrDefault().Type.TestEquals( ParsedExpressionConstructType.BinaryOperator ),
                sut.GetConstructs().FirstOrDefault().Construct.TestRefEquals( @operator ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void AddPrefixUnaryOperator_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var @operator = new ParsedExpressionNegateOperator();

        var result = sut.AddPrefixUnaryOperator( symbol, @operator );

        Assertion.All(
                sut.GetConstructs().Count().TestEquals( 1 ),
                sut.GetConstructs().FirstOrDefault().Symbol.ToString().TestEquals( symbol ),
                sut.GetConstructs().FirstOrDefault().Type.TestEquals( ParsedExpressionConstructType.PrefixUnaryOperator ),
                sut.GetConstructs().FirstOrDefault().Construct.TestRefEquals( @operator ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void AddPostfixUnaryOperator_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var @operator = new ParsedExpressionNegateOperator();

        var result = sut.AddPostfixUnaryOperator( symbol, @operator );

        Assertion.All(
                sut.GetConstructs().Count().TestEquals( 1 ),
                sut.GetConstructs().FirstOrDefault().Symbol.ToString().TestEquals( symbol ),
                sut.GetConstructs().FirstOrDefault().Type.TestEquals( ParsedExpressionConstructType.PostfixUnaryOperator ),
                sut.GetConstructs().FirstOrDefault().Construct.TestRefEquals( @operator ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void AddPrefixTypeConverter_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var converter = new ParsedExpressionTypeConverter<int>();

        var result = sut.AddPrefixTypeConverter( symbol, converter );

        Assertion.All(
                sut.GetConstructs().Count().TestEquals( 1 ),
                sut.GetConstructs().FirstOrDefault().Symbol.ToString().TestEquals( symbol ),
                sut.GetConstructs().FirstOrDefault().Type.TestEquals( ParsedExpressionConstructType.PrefixTypeConverter ),
                sut.GetConstructs().FirstOrDefault().Construct.TestRefEquals( converter ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void AddPostfixTypeConverter_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var converter = new ParsedExpressionTypeConverter<int>();

        var result = sut.AddPostfixTypeConverter( symbol, converter );

        Assertion.All(
                sut.GetConstructs().Count().TestEquals( 1 ),
                sut.GetConstructs().FirstOrDefault().Symbol.ToString().TestEquals( symbol ),
                sut.GetConstructs().FirstOrDefault().Type.TestEquals( ParsedExpressionConstructType.PostfixTypeConverter ),
                sut.GetConstructs().FirstOrDefault().Construct.TestRefEquals( converter ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void AddConstant_ShouldAddNewConstant()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var constant = new ParsedExpressionConstant<int>( Fixture.Create<int>() );

        var result = sut.AddConstant( symbol, constant );

        Assertion.All(
                sut.GetConstructs().Count().TestEquals( 1 ),
                sut.GetConstructs().FirstOrDefault().Symbol.ToString().TestEquals( symbol ),
                sut.GetConstructs().FirstOrDefault().Type.TestEquals( ParsedExpressionConstructType.Constant ),
                sut.GetConstructs().FirstOrDefault().Construct.TestRefEquals( constant ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void AddTypeDeclaration_ShouldAddNewTypeDeclaration()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();

        var result = sut.AddTypeDeclaration<int>( symbol );

        Assertion.All(
                sut.GetConstructs().Count().TestEquals( 1 ),
                sut.GetConstructs().FirstOrDefault().Symbol.ToString().TestEquals( symbol ),
                sut.GetConstructs().FirstOrDefault().Type.TestEquals( ParsedExpressionConstructType.TypeDeclaration ),
                sut.GetConstructs().FirstOrDefault().Construct.TestRefEquals( typeof( int ) ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void AddFunction_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var function = new ParsedExpressionFunction<int>( () => Fixture.Create<int>() );

        var result = sut.AddFunction( symbol, function );

        Assertion.All(
                sut.GetConstructs().Count().TestEquals( 1 ),
                sut.GetConstructs().FirstOrDefault().Symbol.ToString().TestEquals( symbol ),
                sut.GetConstructs().FirstOrDefault().Type.TestEquals( ParsedExpressionConstructType.Function ),
                sut.GetConstructs().FirstOrDefault().Construct.TestRefEquals( function ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void AddVariadicFunction_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var function = Substitute.ForPartsOf<ParsedExpressionVariadicFunction>();

        var result = sut.AddVariadicFunction( symbol, function );

        Assertion.All(
                sut.GetConstructs().Count().TestEquals( 1 ),
                sut.GetConstructs().FirstOrDefault().Symbol.ToString().TestEquals( symbol ),
                sut.GetConstructs().FirstOrDefault().Type.TestEquals( ParsedExpressionConstructType.VariadicFunction ),
                sut.GetConstructs().FirstOrDefault().Construct.TestRefEquals( function ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetBinaryOperatorPrecedence_ShouldRegisterPrecedence()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var value = Fixture.Create<int>();

        var result = sut.SetBinaryOperatorPrecedence( symbol, value );

        Assertion.All(
                sut.GetBinaryOperatorPrecedences().Count().TestEquals( 1 ),
                sut.GetBinaryOperatorPrecedences().FirstOrDefault().Key.ToString().TestEquals( symbol ),
                sut.GetBinaryOperatorPrecedences().FirstOrDefault().Value.TestEquals( value ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetPrefixUnaryConstructPrecedence_ShouldRegisterPrecedence()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var value = Fixture.Create<int>();

        var result = sut.SetPrefixUnaryConstructPrecedence( symbol, value );

        Assertion.All(
                sut.GetPrefixUnaryConstructPrecedences().Count().TestEquals( 1 ),
                sut.GetPrefixUnaryConstructPrecedences().FirstOrDefault().Key.ToString().TestEquals( symbol ),
                sut.GetPrefixUnaryConstructPrecedences().FirstOrDefault().Value.TestEquals( value ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void SetPostfixUnaryConstructPrecedence_ShouldRegisterPrecedence()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var value = Fixture.Create<int>();

        var result = sut.SetPostfixUnaryConstructPrecedence( symbol, value );

        Assertion.All(
                sut.GetPostfixUnaryConstructPrecedences().Count().TestEquals( 1 ),
                sut.GetPostfixUnaryConstructPrecedences().FirstOrDefault().Key.ToString().TestEquals( symbol ),
                sut.GetPostfixUnaryConstructPrecedences().FirstOrDefault().Value.TestEquals( value ),
                result.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderIsEmpty()
    {
        var nonExistingSymbol = Fixture.Create<string>();
        var internalSymbols = new[]
        {
            ParsedExpressionConstructDefaults.MemberAccessSymbol,
            ParsedExpressionConstructDefaults.IndexerCallSymbol,
            ParsedExpressionConstructDefaults.MethodCallSymbol,
            ParsedExpressionConstructDefaults.CtorCallSymbol,
            ParsedExpressionConstructDefaults.MakeArraySymbol,
            ParsedExpressionConstructDefaults.InvokeSymbol
        };

        var sut = new ParsedExpressionFactoryBuilder();

        var result = sut.Build();

        Assertion.All(
                result.Configuration.TestNotNull(),
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSetEqual( internalSymbols ),
                result.GetConstructType( nonExistingSymbol ).TestEquals( ParsedExpressionConstructType.None ),
                result.GetGenericBinaryOperatorType( nonExistingSymbol ).TestNull(),
                result.GetSpecializedBinaryOperators( nonExistingSymbol ).TestEmpty(),
                result.GetGenericPrefixUnaryConstructType( nonExistingSymbol ).TestNull(),
                result.GetSpecializedPrefixUnaryConstructs( nonExistingSymbol ).TestEmpty(),
                result.GetGenericPostfixUnaryConstructType( nonExistingSymbol ).TestNull(),
                result.GetSpecializedPostfixUnaryConstructs( nonExistingSymbol ).TestEmpty(),
                result.GetTypeConverterTargetType( nonExistingSymbol ).TestNull(),
                result.GetTypeDeclarationType( nonExistingSymbol ).TestNull(),
                result.GetConstantExpression( nonExistingSymbol ).TestNull(),
                result.GetFunctionExpressions( nonExistingSymbol ).TestEmpty(),
                result.GetVariadicFunctionType( nonExistingSymbol ).TestNull(),
                result.GetBinaryOperatorPrecedence( nonExistingSymbol ).TestNull(),
                result.GetPrefixUnaryConstructPrecedence( nonExistingSymbol ).TestNull(),
                result.GetPostfixUnaryConstructPrecedence( nonExistingSymbol ).TestNull(),
                internalSymbols.Select( s => result.GetConstructType( s ) )
                    .TestAll( (e, _) => e.TestEquals( ParsedExpressionConstructType.VariadicFunction ) ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneGenericBinaryOperatorWithPrecedence()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var precedence = Fixture.Create<int>();
        var @operator = new ParsedExpressionAddOperator();
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( symbol, @operator )
            .SetBinaryOperatorPrecedence( symbol, precedence );

        var result = sut.Build();

        Assertion.All(
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSupersetOf( [ symbol ] ),
                result.GetConstructType( symbol ).TestEquals( ParsedExpressionConstructType.BinaryOperator ),
                result.GetGenericBinaryOperatorType( symbol ).TestEquals( typeof( ParsedExpressionAddOperator ) ),
                result.GetSpecializedBinaryOperators( symbol ).TestEmpty(),
                result.GetGenericPrefixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPrefixUnaryConstructs( symbol ).TestEmpty(),
                result.GetGenericPostfixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPostfixUnaryConstructs( symbol ).TestEmpty(),
                result.GetTypeConverterTargetType( symbol ).TestNull(),
                result.GetTypeDeclarationType( symbol ).TestNull(),
                result.GetConstantExpression( symbol ).TestNull(),
                result.GetFunctionExpressions( symbol ).TestEmpty(),
                result.GetVariadicFunctionType( symbol ).TestNull(),
                result.GetBinaryOperatorPrecedence( symbol ).TestEquals( precedence ),
                result.GetPrefixUnaryConstructPrecedence( symbol ).TestNull(),
                result.GetPostfixUnaryConstructPrecedence( symbol ).TestNull() )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneSpecializedBinaryOperatorWithPrecedence()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var precedence = Fixture.Create<int>();
        var @operator = new ParsedExpressionAndOperator();
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( symbol, @operator )
            .SetBinaryOperatorPrecedence( symbol, precedence );

        var result = sut.Build();

        Assertion.All(
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSupersetOf( [ symbol ] ),
                result.GetConstructType( symbol ).TestEquals( ParsedExpressionConstructType.BinaryOperator ),
                result.GetGenericBinaryOperatorType( symbol ).TestNull(),
                result.GetSpecializedBinaryOperators( symbol )
                    .TestSetEqual(
                    [
                        new ParsedExpressionBinaryOperatorInfo( typeof( ParsedExpressionAndOperator ), typeof( bool ), typeof( bool ) )
                    ] ),
                result.GetGenericPrefixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPrefixUnaryConstructs( symbol ).TestEmpty(),
                result.GetGenericPostfixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPostfixUnaryConstructs( symbol ).TestEmpty(),
                result.GetTypeConverterTargetType( symbol ).TestNull(),
                result.GetTypeDeclarationType( symbol ).TestNull(),
                result.GetConstantExpression( symbol ).TestNull(),
                result.GetFunctionExpressions( symbol ).TestEmpty(),
                result.GetVariadicFunctionType( symbol ).TestNull(),
                result.GetBinaryOperatorPrecedence( symbol ).TestEquals( precedence ),
                result.GetPrefixUnaryConstructPrecedence( symbol ).TestNull(),
                result.GetPostfixUnaryConstructPrecedence( symbol ).TestNull() )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneGenericPrefixUnaryOperatorWithPrecedence()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var precedence = Fixture.Create<int>();
        var @operator = new ParsedExpressionNegateOperator();
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( symbol, @operator )
            .SetPrefixUnaryConstructPrecedence( symbol, precedence );

        var result = sut.Build();

        Assertion.All(
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSupersetOf( [ symbol ] ),
                result.GetConstructType( symbol ).TestEquals( ParsedExpressionConstructType.PrefixUnaryOperator ),
                result.GetGenericBinaryOperatorType( symbol ).TestNull(),
                result.GetSpecializedBinaryOperators( symbol ).TestEmpty(),
                result.GetGenericPrefixUnaryConstructType( symbol ).TestEquals( typeof( ParsedExpressionNegateOperator ) ),
                result.GetSpecializedPrefixUnaryConstructs( symbol ).TestEmpty(),
                result.GetGenericPostfixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPostfixUnaryConstructs( symbol ).TestEmpty(),
                result.GetTypeConverterTargetType( symbol ).TestNull(),
                result.GetTypeDeclarationType( symbol ).TestNull(),
                result.GetConstantExpression( symbol ).TestNull(),
                result.GetFunctionExpressions( symbol ).TestEmpty(),
                result.GetVariadicFunctionType( symbol ).TestNull(),
                result.GetBinaryOperatorPrecedence( symbol ).TestNull(),
                result.GetPrefixUnaryConstructPrecedence( symbol ).TestEquals( precedence ),
                result.GetPostfixUnaryConstructPrecedence( symbol ).TestNull() )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneSpecializedPrefixUnaryOperatorWithPrecedence()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var precedence = Fixture.Create<int>();
        var @operator = new ParsedExpressionNotOperator();
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( symbol, @operator )
            .SetPrefixUnaryConstructPrecedence( symbol, precedence );

        var result = sut.Build();

        Assertion.All(
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSupersetOf( [ symbol ] ),
                result.GetConstructType( symbol ).TestEquals( ParsedExpressionConstructType.PrefixUnaryOperator ),
                result.GetGenericBinaryOperatorType( symbol ).TestNull(),
                result.GetSpecializedBinaryOperators( symbol ).TestEmpty(),
                result.GetGenericPrefixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPrefixUnaryConstructs( symbol )
                    .TestSetEqual( [ new ParsedExpressionUnaryConstructInfo( typeof( ParsedExpressionNotOperator ), typeof( bool ) ) ] ),
                result.GetGenericPostfixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPostfixUnaryConstructs( symbol ).TestEmpty(),
                result.GetTypeConverterTargetType( symbol ).TestNull(),
                result.GetTypeDeclarationType( symbol ).TestNull(),
                result.GetConstantExpression( symbol ).TestNull(),
                result.GetFunctionExpressions( symbol ).TestEmpty(),
                result.GetVariadicFunctionType( symbol ).TestNull(),
                result.GetBinaryOperatorPrecedence( symbol ).TestNull(),
                result.GetPrefixUnaryConstructPrecedence( symbol ).TestEquals( precedence ),
                result.GetPostfixUnaryConstructPrecedence( symbol ).TestNull() )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneGenericPostfixUnaryOperatorWithPrecedence()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var precedence = Fixture.Create<int>();
        var @operator = new ParsedExpressionNegateOperator();
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( symbol, @operator )
            .SetPostfixUnaryConstructPrecedence( symbol, precedence );

        var result = sut.Build();

        Assertion.All(
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSupersetOf( [ symbol ] ),
                result.GetConstructType( symbol ).TestEquals( ParsedExpressionConstructType.PostfixUnaryOperator ),
                result.GetGenericBinaryOperatorType( symbol ).TestNull(),
                result.GetSpecializedBinaryOperators( symbol ).TestEmpty(),
                result.GetGenericPrefixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPrefixUnaryConstructs( symbol ).TestEmpty(),
                result.GetGenericPostfixUnaryConstructType( symbol ).TestEquals( typeof( ParsedExpressionNegateOperator ) ),
                result.GetSpecializedPostfixUnaryConstructs( symbol ).TestEmpty(),
                result.GetTypeConverterTargetType( symbol ).TestNull(),
                result.GetTypeDeclarationType( symbol ).TestNull(),
                result.GetConstantExpression( symbol ).TestNull(),
                result.GetFunctionExpressions( symbol ).TestEmpty(),
                result.GetVariadicFunctionType( symbol ).TestNull(),
                result.GetBinaryOperatorPrecedence( symbol ).TestNull(),
                result.GetPrefixUnaryConstructPrecedence( symbol ).TestNull(),
                result.GetPostfixUnaryConstructPrecedence( symbol ).TestEquals( precedence ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneSpecializedPostfixUnaryOperatorWithPrecedence()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var precedence = Fixture.Create<int>();
        var @operator = new ParsedExpressionNotOperator();
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( symbol, @operator )
            .SetPostfixUnaryConstructPrecedence( symbol, precedence );

        var result = sut.Build();

        Assertion.All(
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSupersetOf( [ symbol ] ),
                result.GetConstructType( symbol ).TestEquals( ParsedExpressionConstructType.PostfixUnaryOperator ),
                result.GetGenericBinaryOperatorType( symbol ).TestNull(),
                result.GetSpecializedBinaryOperators( symbol ).TestEmpty(),
                result.GetGenericPrefixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPrefixUnaryConstructs( symbol ).TestEmpty(),
                result.GetGenericPostfixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPostfixUnaryConstructs( symbol )
                    .TestSetEqual( [ new ParsedExpressionUnaryConstructInfo( typeof( ParsedExpressionNotOperator ), typeof( bool ) ) ] ),
                result.GetTypeConverterTargetType( symbol ).TestNull(),
                result.GetTypeDeclarationType( symbol ).TestNull(),
                result.GetConstantExpression( symbol ).TestNull(),
                result.GetFunctionExpressions( symbol ).TestEmpty(),
                result.GetVariadicFunctionType( symbol ).TestNull(),
                result.GetBinaryOperatorPrecedence( symbol ).TestNull(),
                result.GetPrefixUnaryConstructPrecedence( symbol ).TestNull(),
                result.GetPostfixUnaryConstructPrecedence( symbol ).TestEquals( precedence ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneGenericPrefixTypeConverterWithPrecedence()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var precedence = Fixture.Create<int>();
        var converter = new ParsedExpressionTypeConverter<int>();
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( symbol, converter )
            .SetPrefixUnaryConstructPrecedence( symbol, precedence );

        var result = sut.Build();

        Assertion.All(
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSupersetOf( [ symbol ] ),
                result.GetConstructType( symbol ).TestEquals( ParsedExpressionConstructType.PrefixTypeConverter ),
                result.GetGenericBinaryOperatorType( symbol ).TestNull(),
                result.GetSpecializedBinaryOperators( symbol ).TestEmpty(),
                result.GetGenericPrefixUnaryConstructType( symbol ).TestEquals( typeof( ParsedExpressionTypeConverter<int> ) ),
                result.GetSpecializedPrefixUnaryConstructs( symbol ).TestEmpty(),
                result.GetGenericPostfixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPostfixUnaryConstructs( symbol ).TestEmpty(),
                result.GetTypeConverterTargetType( symbol ).TestEquals( typeof( int ) ),
                result.GetTypeDeclarationType( symbol ).TestNull(),
                result.GetConstantExpression( symbol ).TestNull(),
                result.GetFunctionExpressions( symbol ).TestEmpty(),
                result.GetVariadicFunctionType( symbol ).TestNull(),
                result.GetBinaryOperatorPrecedence( symbol ).TestNull(),
                result.GetPrefixUnaryConstructPrecedence( symbol ).TestEquals( precedence ),
                result.GetPostfixUnaryConstructPrecedence( symbol ).TestNull() )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneSpecializedPrefixTypeConverterWithPrecedence()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var precedence = Fixture.Create<int>();
        var converter = new ParsedExpressionTypeConverter<int, long>();
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( symbol, converter )
            .SetPrefixUnaryConstructPrecedence( symbol, precedence );

        var result = sut.Build();

        Assertion.All(
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSupersetOf( [ symbol ] ),
                result.GetConstructType( symbol ).TestEquals( ParsedExpressionConstructType.PrefixTypeConverter ),
                result.GetGenericBinaryOperatorType( symbol ).TestNull(),
                result.GetSpecializedBinaryOperators( symbol ).TestEmpty(),
                result.GetGenericPrefixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPrefixUnaryConstructs( symbol )
                    .TestSetEqual(
                        [ new ParsedExpressionUnaryConstructInfo( typeof( ParsedExpressionTypeConverter<int, long> ), typeof( long ) ) ] ),
                result.GetGenericPostfixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPostfixUnaryConstructs( symbol ).TestEmpty(),
                result.GetTypeConverterTargetType( symbol ).TestEquals( typeof( int ) ),
                result.GetTypeDeclarationType( symbol ).TestNull(),
                result.GetConstantExpression( symbol ).TestNull(),
                result.GetFunctionExpressions( symbol ).TestEmpty(),
                result.GetVariadicFunctionType( symbol ).TestNull(),
                result.GetBinaryOperatorPrecedence( symbol ).TestNull(),
                result.GetPrefixUnaryConstructPrecedence( symbol ).TestEquals( precedence ),
                result.GetPostfixUnaryConstructPrecedence( symbol ).TestNull() )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneGenericPostfixTypeConverterWithPrecedence()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var precedence = Fixture.Create<int>();
        var converter = new ParsedExpressionTypeConverter<int>();
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( symbol, converter )
            .SetPostfixUnaryConstructPrecedence( symbol, precedence );

        var result = sut.Build();

        Assertion.All(
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSupersetOf( [ symbol ] ),
                result.GetConstructType( symbol ).TestEquals( ParsedExpressionConstructType.PostfixTypeConverter ),
                result.GetGenericBinaryOperatorType( symbol ).TestNull(),
                result.GetSpecializedBinaryOperators( symbol ).TestEmpty(),
                result.GetGenericPrefixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPrefixUnaryConstructs( symbol ).TestEmpty(),
                result.GetGenericPostfixUnaryConstructType( symbol ).TestEquals( typeof( ParsedExpressionTypeConverter<int> ) ),
                result.GetSpecializedPostfixUnaryConstructs( symbol ).TestEmpty(),
                result.GetTypeConverterTargetType( symbol ).TestEquals( typeof( int ) ),
                result.GetTypeDeclarationType( symbol ).TestNull(),
                result.GetConstantExpression( symbol ).TestNull(),
                result.GetFunctionExpressions( symbol ).TestEmpty(),
                result.GetVariadicFunctionType( symbol ).TestNull(),
                result.GetBinaryOperatorPrecedence( symbol ).TestNull(),
                result.GetPrefixUnaryConstructPrecedence( symbol ).TestNull(),
                result.GetPostfixUnaryConstructPrecedence( symbol ).TestEquals( precedence ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneSpecializedPostfixTypeConverterWithPrecedence()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var precedence = Fixture.Create<int>();
        var converter = new ParsedExpressionTypeConverter<int, long>();
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( symbol, converter )
            .SetPostfixUnaryConstructPrecedence( symbol, precedence );

        var result = sut.Build();

        Assertion.All(
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSupersetOf( [ symbol ] ),
                result.GetConstructType( symbol ).TestEquals( ParsedExpressionConstructType.PostfixTypeConverter ),
                result.GetGenericBinaryOperatorType( symbol ).TestNull(),
                result.GetSpecializedBinaryOperators( symbol ).TestEmpty(),
                result.GetGenericPrefixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPrefixUnaryConstructs( symbol ).TestEmpty(),
                result.GetGenericPostfixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPostfixUnaryConstructs( symbol )
                    .TestSetEqual(
                        [ new ParsedExpressionUnaryConstructInfo( typeof( ParsedExpressionTypeConverter<int, long> ), typeof( long ) ) ] ),
                result.GetTypeConverterTargetType( symbol ).TestEquals( typeof( int ) ),
                result.GetTypeDeclarationType( symbol ).TestNull(),
                result.GetConstantExpression( symbol ).TestNull(),
                result.GetFunctionExpressions( symbol ).TestEmpty(),
                result.GetVariadicFunctionType( symbol ).TestNull(),
                result.GetBinaryOperatorPrecedence( symbol ).TestNull(),
                result.GetPrefixUnaryConstructPrecedence( symbol ).TestNull(),
                result.GetPostfixUnaryConstructPrecedence( symbol ).TestEquals( precedence ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneConstant()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var constant = new ParsedExpressionConstant<int>( Fixture.Create<int>() );
        var sut = new ParsedExpressionFactoryBuilder()
            .AddConstant( symbol, constant );

        var result = sut.Build();

        Assertion.All(
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSupersetOf( [ symbol ] ),
                result.GetConstructType( symbol ).TestEquals( ParsedExpressionConstructType.Constant ),
                result.GetGenericBinaryOperatorType( symbol ).TestNull(),
                result.GetSpecializedBinaryOperators( symbol ).TestEmpty(),
                result.GetGenericPrefixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPrefixUnaryConstructs( symbol ).TestEmpty(),
                result.GetGenericPostfixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPostfixUnaryConstructs( symbol ).TestEmpty(),
                result.GetTypeConverterTargetType( symbol ).TestNull(),
                result.GetTypeDeclarationType( symbol ).TestNull(),
                result.GetConstantExpression( symbol ).TestRefEquals( constant.Expression ),
                result.GetFunctionExpressions( symbol ).TestEmpty(),
                result.GetVariadicFunctionType( symbol ).TestNull(),
                result.GetBinaryOperatorPrecedence( symbol ).TestNull(),
                result.GetPrefixUnaryConstructPrecedence( symbol ).TestNull(),
                result.GetPostfixUnaryConstructPrecedence( symbol ).TestNull() )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneTypeDeclaration()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var sut = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<int>( symbol );

        var result = sut.Build();

        Assertion.All(
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSupersetOf( [ symbol ] ),
                result.GetConstructType( symbol ).TestEquals( ParsedExpressionConstructType.TypeDeclaration ),
                result.GetGenericBinaryOperatorType( symbol ).TestNull(),
                result.GetSpecializedBinaryOperators( symbol ).TestEmpty(),
                result.GetGenericPrefixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPrefixUnaryConstructs( symbol ).TestEmpty(),
                result.GetGenericPostfixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPostfixUnaryConstructs( symbol ).TestEmpty(),
                result.GetTypeConverterTargetType( symbol ).TestNull(),
                result.GetTypeDeclarationType( symbol ).TestEquals( typeof( int ) ),
                result.GetConstantExpression( symbol ).TestNull(),
                result.GetFunctionExpressions( symbol ).TestEmpty(),
                result.GetVariadicFunctionType( symbol ).TestNull(),
                result.GetBinaryOperatorPrecedence( symbol ).TestNull(),
                result.GetPrefixUnaryConstructPrecedence( symbol ).TestNull(),
                result.GetPostfixUnaryConstructPrecedence( symbol ).TestNull() )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneFunction()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var function = new ParsedExpressionFunction<int>( () => Fixture.Create<int>() );
        var sut = new ParsedExpressionFactoryBuilder()
            .AddFunction( symbol, function );

        var result = sut.Build();

        Assertion.All(
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSupersetOf( [ symbol ] ),
                result.GetConstructType( symbol ).TestEquals( ParsedExpressionConstructType.Function ),
                result.GetGenericBinaryOperatorType( symbol ).TestNull(),
                result.GetSpecializedBinaryOperators( symbol ).TestEmpty(),
                result.GetGenericPrefixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPrefixUnaryConstructs( symbol ).TestEmpty(),
                result.GetGenericPostfixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPostfixUnaryConstructs( symbol ).TestEmpty(),
                result.GetTypeConverterTargetType( symbol ).TestNull(),
                result.GetTypeDeclarationType( symbol ).TestNull(),
                result.GetConstantExpression( symbol ).TestNull(),
                result.GetFunctionExpressions( symbol ).Count().TestEquals( 1 ),
                result.GetFunctionExpressions( symbol ).FirstOrDefault().TestRefEquals( function.Lambda ),
                result.GetVariadicFunctionType( symbol ).TestNull(),
                result.GetBinaryOperatorPrecedence( symbol ).TestNull(),
                result.GetPrefixUnaryConstructPrecedence( symbol ).TestNull(),
                result.GetPostfixUnaryConstructPrecedence( symbol ).TestNull() )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneVariadicFunction()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var function = Substitute.ForPartsOf<ParsedExpressionVariadicFunction>();
        var sut = new ParsedExpressionFactoryBuilder()
            .AddVariadicFunction( symbol, function );

        var result = sut.Build();

        Assertion.All(
                result.GetConstructSymbols().Select( s => s.ToString() ).TestSupersetOf( [ symbol ] ),
                result.GetConstructType( symbol ).TestEquals( ParsedExpressionConstructType.VariadicFunction ),
                result.GetGenericBinaryOperatorType( symbol ).TestNull(),
                result.GetSpecializedBinaryOperators( symbol ).TestEmpty(),
                result.GetGenericPrefixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPrefixUnaryConstructs( symbol ).TestEmpty(),
                result.GetGenericPostfixUnaryConstructType( symbol ).TestNull(),
                result.GetSpecializedPostfixUnaryConstructs( symbol ).TestEmpty(),
                result.GetTypeConverterTargetType( symbol ).TestNull(),
                result.GetTypeDeclarationType( symbol ).TestNull(),
                result.GetConstantExpression( symbol ).TestNull(),
                result.GetFunctionExpressions( symbol ).TestEmpty(),
                result.GetVariadicFunctionType( symbol ).TestEquals( function.GetType() ),
                result.GetBinaryOperatorPrecedence( symbol ).TestNull(),
                result.GetPrefixUnaryConstructPrecedence( symbol ).TestNull(),
                result.GetPostfixUnaryConstructPrecedence( symbol ).TestNull() )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneOfEachConstructsWithPrecedence()
    {
        var (operatorSymbol, typeConverterSymbol, constantSymbol, typeDeclarationSymbol, functionSymbol, variadicFunctionSymbol) =
            Fixture.CreateManyDistinct<string>( count: 6 ).Select( s => $"_{s}" ).ToList();

        var precedence = Fixture.Create<int>();
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( operatorSymbol, new ParsedExpressionAddOperator() )
            .AddBinaryOperator( operatorSymbol, new ParsedExpressionAddDecimalOperator() )
            .AddPrefixUnaryOperator( operatorSymbol, new ParsedExpressionNegateOperator() )
            .AddPrefixUnaryOperator( operatorSymbol, new ParsedExpressionNegateDecimalOperator() )
            .AddPostfixUnaryOperator( operatorSymbol, new ParsedExpressionNegateOperator() )
            .AddPostfixUnaryOperator( operatorSymbol, new ParsedExpressionNegateDecimalOperator() )
            .AddPrefixTypeConverter( typeConverterSymbol, new ParsedExpressionTypeConverter<int>() )
            .AddPrefixTypeConverter( typeConverterSymbol, new ParsedExpressionTypeConverter<int, long>() )
            .AddPostfixTypeConverter( typeConverterSymbol, new ParsedExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( typeConverterSymbol, new ParsedExpressionTypeConverter<int, long>() )
            .AddConstant( constantSymbol, new ParsedExpressionConstant<int>( Fixture.Create<int>() ) )
            .AddFunction( functionSymbol, new ParsedExpressionFunction<int>( () => Fixture.Create<int>() ) )
            .AddFunction( functionSymbol, new ParsedExpressionFunction<int, int>( a => a ) )
            .AddFunction( functionSymbol, new ParsedExpressionFunction<double, int>( a => ( int )a ) )
            .AddVariadicFunction( variadicFunctionSymbol, Substitute.ForPartsOf<ParsedExpressionVariadicFunction>() )
            .AddTypeDeclaration<int>( typeDeclarationSymbol )
            .SetBinaryOperatorPrecedence( operatorSymbol, precedence )
            .SetPrefixUnaryConstructPrecedence( operatorSymbol, precedence )
            .SetPostfixUnaryConstructPrecedence( operatorSymbol, precedence )
            .SetPrefixUnaryConstructPrecedence( typeConverterSymbol, precedence )
            .SetPostfixUnaryConstructPrecedence( typeConverterSymbol, precedence );

        var result = sut.Build();

        result.GetConstructSymbols()
            .Select( s => s.ToString() )
            .TestSupersetOf(
                [ operatorSymbol, typeConverterSymbol, constantSymbol, typeDeclarationSymbol, functionSymbol, variadicFunctionSymbol ] )
            .Go();
    }

    [Theory]
    [InlineData( '0', '_', "eE", '\'', true )]
    [InlineData( '.', '0', "eE", '\'', true )]
    [InlineData( '.', '_', "e0", '\'', true )]
    [InlineData( '.', '_', "eE", '0', true )]
    [InlineData( 'x', 'x', "eE", '\'', true )]
    [InlineData( 'x', '_', "eE", 'x', true )]
    [InlineData( 'x', '_', "ex", '\'', true )]
    [InlineData( '.', 'x', "eE", 'x', true )]
    [InlineData( '.', 'x', "ex", '\'', true )]
    [InlineData( '.', '_', "ex", 'x', true )]
    [InlineData( '.', '_', "", '\'', true )]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenConfigurationIsInvalid(
        char decimalPoint,
        char integerDigitSeparator,
        string scientificNotationExponents,
        char stringDelimiter,
        bool allowScientificNotation)
    {
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( decimalPoint );
        configuration.IntegerDigitSeparator.Returns( integerDigitSeparator );
        configuration.ScientificNotationExponents.Returns( scientificNotationExponents );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.AllowScientificNotation.Returns( allowScientificNotation );
        configuration.StringDelimiter.Returns( stringDelimiter );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( true );

        var sut = new ParsedExpressionFactoryBuilder()
            .SetConfiguration( configuration );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenAtLeastOneConstructSymbolIsInvalid()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( string.Empty, new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( string.Empty, 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenOperatorDefinitionContainsNonOperatorConstruct()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .SetBinaryOperatorPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericBinaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenSpecializedBinaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddDecimalOperator() )
            .AddBinaryOperator( "+", new ParsedExpressionAddDecimalOperator() )
            .SetBinaryOperatorPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenBinaryOperatorPrecedenceIsUndefined()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericPrefixUnaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "+", new ParsedExpressionNegateOperator() )
            .AddPrefixUnaryOperator( "+", new ParsedExpressionNegateOperator() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void
        Build_ShouldThrowMathExpressionFactoryBuilderException_WhenSpecializedPrefixUnaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "+", new ParsedExpressionNegateDecimalOperator() )
            .AddPrefixUnaryOperator( "+", new ParsedExpressionNegateDecimalOperator() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPrefixUnaryOperatorPrecedenceIsUndefined()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "+", new ParsedExpressionNegateOperator() );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericPostfixUnaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "+", new ParsedExpressionNegateOperator() )
            .AddPostfixUnaryOperator( "+", new ParsedExpressionNegateOperator() )
            .SetPostfixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void
        Build_ShouldThrowMathExpressionFactoryBuilderException_WhenSpecializedPostfixUnaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "+", new ParsedExpressionNegateDecimalOperator() )
            .AddPostfixUnaryOperator( "+", new ParsedExpressionNegateDecimalOperator() )
            .SetPostfixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPostfixUnaryOperatorPrecedenceIsUndefined()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "+", new ParsedExpressionNegateOperator() );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenTypeConverterDefinitionContainsNonTypeConverterConstruct()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void
        Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPrefixAndPostfixTypeConverterCollectionsHaveDifferentTargetTypeWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( "+", new ParsedExpressionTypeConverter<decimal>() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 )
            .SetPostfixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericPrefixTypeConverterIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void
        Build_ShouldThrowMathExpressionFactoryBuilderException_WhenSpecializedPrefixTypeConverterIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int, long>() )
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int, long>() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPrefixTypeConverterPrecedenceIsUndefined()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void
        Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPrefixTypeConvertersHaveDifferentTargetTypeWithinTheSameCollection()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<long, int>() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericPostfixTypeConverterIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .SetPostfixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void
        Build_ShouldThrowMathExpressionFactoryBuilderException_WhenSpecializedPostfixTypeConverterIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "+", new ParsedExpressionTypeConverter<int, long>() )
            .AddPostfixTypeConverter( "+", new ParsedExpressionTypeConverter<int, long>() )
            .SetPostfixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPostfixTypeConverterPrecedenceIsUndefined()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void
        Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPostfixTypeConvertersHaveDifferentTargetTypeWithinTheSameCollection()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( "+", new ParsedExpressionTypeConverter<long, int>() )
            .SetPostfixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenConstantDefinitionContainsMoreThanOneConstant()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddConstant( "e", new ParsedExpressionConstant<int>( Fixture.Create<int>() ) )
            .AddConstant( "e", new ParsedExpressionConstant<int>( Fixture.Create<int>() ) );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenConstantDefinitionContainsNonConstantConstruct()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddConstant( "e", new ParsedExpressionConstant<int>( Fixture.Create<int>() ) )
            .AddBinaryOperator( "e", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "e", 1 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenTypeDeclarationDefinitionContainsMoreThanOneTypeDeclaration()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<int>( "e" )
            .AddTypeDeclaration<long>( "e" );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenTypeDeclarationDefinitionContainsNonTypeDeclarationConstruct()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<int>( "e" )
            .AddBinaryOperator( "e", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "e", 1 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenFunctionSignatureIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddFunction( "f", new ParsedExpressionFunction<int, int>( x => x ) )
            .AddFunction( "f", new ParsedExpressionFunction<int, int>( x => x ) );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenFunctionDefinitionContainsNonFunctionConstruct()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddFunction( "f", new ParsedExpressionFunction<int>( () => Fixture.Create<int>() ) )
            .AddBinaryOperator( "f", new ParsedExpressionAddOperator() );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenVariadicFunctionDefinitionContainsMoreThanOneFunction()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddVariadicFunction( "e", Substitute.ForPartsOf<ParsedExpressionVariadicFunction>() )
            .AddVariadicFunction( "e", Substitute.ForPartsOf<ParsedExpressionVariadicFunction>() );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenVariadicFunctionDefinitionContainsNonVariadicFunctionConstruct()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddVariadicFunction( "e", Substitute.ForPartsOf<ParsedExpressionVariadicFunction>() )
            .AddBinaryOperator( "e", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "e", 1 );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenAnyConstructHasMemberAccessSymbol()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddFunction(
                ParsedExpressionConstructDefaults.MemberAccessSymbol,
                new ParsedExpressionFunction<int>( () => Fixture.Create<int>() ) );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenAnyConstructHasIndexerCallSymbol()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddFunction(
                ParsedExpressionConstructDefaults.IndexerCallSymbol,
                new ParsedExpressionFunction<int>( () => Fixture.Create<int>() ) );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenAnyConstructHasMethodCallSymbol()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddFunction(
                ParsedExpressionConstructDefaults.MethodCallSymbol,
                new ParsedExpressionFunction<int>( () => Fixture.Create<int>() ) );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenAnyConstructHasCtorCallSymbol()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddFunction(
                ParsedExpressionConstructDefaults.CtorCallSymbol,
                new ParsedExpressionFunction<int>( () => Fixture.Create<int>() ) );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenAnyConstructHasMakeArraySymbol()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddFunction(
                ParsedExpressionConstructDefaults.MakeArraySymbol,
                new ParsedExpressionFunction<int>( () => Fixture.Create<int>() ) );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenAnyConstructHasInvokeSymbol()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddFunction(
                ParsedExpressionConstructDefaults.InvokeSymbol,
                new ParsedExpressionFunction<int>( () => Fixture.Create<int>() ) );

        var action = Lambda.Of( () => sut.Build() );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionFactoryBuilderException>() ).Go();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WithMultipleMessages_WhenMultipleErrorsOccur()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() );

        var action = Lambda.Of( () => sut.Build() );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<ParsedExpressionFactoryBuilderException>(),
                    exc.TestIf().OfType<ParsedExpressionFactoryBuilderException>( e => e.Messages.Count.TestGreaterThan( 1 ) ) ) )
            .Go();
    }
}
