using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Constructs.Boolean;
using LfrlAnvil.Computable.Expressions.Constructs.Decimal;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionFactoryBuilderTests;

public class ParsedExpressionFactoryBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyBuilder()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        using ( new AssertionScope() )
        {
            sut.GetConfiguration().Should().BeNull();
            sut.GetNumberParserProvider().Should().BeNull();
            sut.GetConstructs().Should().BeEmpty();
        }
    }

    [Fact]
    public void SetConfiguration_ShouldUpdateConfigurationToNewObject()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var configuration = new ParsedExpressionFactoryDefaultConfiguration();

        var result = sut.SetConfiguration( configuration );

        using ( new AssertionScope() )
        {
            sut.GetConfiguration().Should().BeSameAs( configuration );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void SetDefaultConfiguration_ShouldUpdateConfigurationToNull()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var configuration = new ParsedExpressionFactoryDefaultConfiguration();
        sut.SetConfiguration( configuration );

        var result = sut.SetDefaultConfiguration();

        using ( new AssertionScope() )
        {
            sut.GetConfiguration().Should().BeNull();
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void SetNumberParserProvider_ShouldUpdateDelegateToNewObject()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of(
            (ParsedExpressionNumberParserParams p) => ParsedExpressionNumberParser.CreateDefaultDecimal( p.Configuration ) );

        var result = sut.SetNumberParserProvider( @delegate );

        using ( new AssertionScope() )
        {
            sut.GetNumberParserProvider().Should().BeSameAs( @delegate );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void SetDefaultNumberParserProvider_ShouldUpdateDelegateToNull()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var @delegate = Lambda.Of(
            (ParsedExpressionNumberParserParams p) => ParsedExpressionNumberParser.CreateDefaultDecimal( p.Configuration ) );

        sut.SetNumberParserProvider( @delegate );

        var result = sut.SetDefaultNumberParserProvider();

        using ( new AssertionScope() )
        {
            sut.GetNumberParserProvider().Should().BeNull();
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddBinaryOperator_WithString_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var @operator = new ParsedExpressionAddOperator();

        var result = sut.AddBinaryOperator( symbol, @operator );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.BinaryOperator );
            entry.Construct.Should().BeSameAs( @operator );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddBinaryOperator_WithMemory_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var @operator = new ParsedExpressionAddOperator();

        var result = sut.AddBinaryOperator( symbol.AsMemory(), @operator );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.BinaryOperator );
            entry.Construct.Should().BeSameAs( @operator );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddPrefixUnaryOperator_WithString_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var @operator = new ParsedExpressionNegateOperator();

        var result = sut.AddPrefixUnaryOperator( symbol, @operator );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.PrefixUnaryOperator );
            entry.Construct.Should().BeSameAs( @operator );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddPrefixUnaryOperator_WithMemory_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var @operator = new ParsedExpressionNegateOperator();

        var result = sut.AddPrefixUnaryOperator( symbol.AsMemory(), @operator );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.PrefixUnaryOperator );
            entry.Construct.Should().BeSameAs( @operator );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddPostfixUnaryOperator_WithString_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var @operator = new ParsedExpressionNegateOperator();

        var result = sut.AddPostfixUnaryOperator( symbol, @operator );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.PostfixUnaryOperator );
            entry.Construct.Should().BeSameAs( @operator );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddPostfixUnaryOperator_WithMemory_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var @operator = new ParsedExpressionNegateOperator();

        var result = sut.AddPostfixUnaryOperator( symbol.AsMemory(), @operator );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.PostfixUnaryOperator );
            entry.Construct.Should().BeSameAs( @operator );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddPrefixTypeConverter_WithString_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var converter = new ParsedExpressionTypeConverter<int>();

        var result = sut.AddPrefixTypeConverter( symbol, converter );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.PrefixTypeConverter );
            entry.Construct.Should().BeSameAs( converter );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddPrefixTypeConverter_WithMemory_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var converter = new ParsedExpressionTypeConverter<int>();

        var result = sut.AddPrefixTypeConverter( symbol.AsMemory(), converter );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.PrefixTypeConverter );
            entry.Construct.Should().BeSameAs( converter );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddPostfixTypeConverter_WithString_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var converter = new ParsedExpressionTypeConverter<int>();

        var result = sut.AddPostfixTypeConverter( symbol, converter );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.PostfixTypeConverter );
            entry.Construct.Should().BeSameAs( converter );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddPostfixTypeConverter_WithMemory_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var converter = new ParsedExpressionTypeConverter<int>();

        var result = sut.AddPostfixTypeConverter( symbol.AsMemory(), converter );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.PostfixTypeConverter );
            entry.Construct.Should().BeSameAs( converter );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddConstant_WithString_ShouldAddNewConstant()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var constant = new ParsedExpressionConstant<int>( Fixture.Create<int>() );

        var result = sut.AddConstant( symbol, constant );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.Constant );
            entry.Construct.Should().BeSameAs( constant );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddConstant_WithMemory_ShouldAddNewConstant()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var constant = new ParsedExpressionConstant<int>( Fixture.Create<int>() );

        var result = sut.AddConstant( symbol.AsMemory(), constant );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.Constant );
            entry.Construct.Should().BeSameAs( constant );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddTypeDeclaration_WithString_ShouldAddNewTypeDeclaration()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();

        var result = sut.AddTypeDeclaration<int>( symbol );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.TypeDeclaration );
            entry.Construct.Should().BeSameAs( typeof( int ) );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddTypeDeclaration_WithMemory_ShouldAddNewTypeDeclaration()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();

        var result = sut.AddTypeDeclaration<int>( symbol.AsMemory() );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.TypeDeclaration );
            entry.Construct.Should().BeSameAs( typeof( int ) );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddFunction_WithString_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var function = new ParsedExpressionFunction<int>( () => Fixture.Create<int>() );

        var result = sut.AddFunction( symbol, function );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.Function );
            entry.Construct.Should().BeSameAs( function );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddFunction_WithMemory_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var function = new ParsedExpressionFunction<int>( () => Fixture.Create<int>() );

        var result = sut.AddFunction( symbol.AsMemory(), function );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.Function );
            entry.Construct.Should().BeSameAs( function );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddVariadicFunction_WithString_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var function = Substitute.ForPartsOf<ParsedExpressionVariadicFunction>();

        var result = sut.AddVariadicFunction( symbol, function );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.VariadicFunction );
            entry.Construct.Should().BeSameAs( function );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void AddVariadicFunction_WithMemory_ShouldAddNewConstruct()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var function = Substitute.ForPartsOf<ParsedExpressionVariadicFunction>();

        var result = sut.AddVariadicFunction( symbol.AsMemory(), function );

        using ( new AssertionScope() )
        {
            var entry = sut.GetConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Symbol.ToString().Should().Be( symbol );
            entry.Type.Should().Be( ParsedExpressionConstructType.VariadicFunction );
            entry.Construct.Should().BeSameAs( function );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void SetBinaryOperatorPrecedence_WithString_ShouldRegisterPrecedence()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var value = Fixture.Create<int>();

        var result = sut.SetBinaryOperatorPrecedence( symbol, value );

        using ( new AssertionScope() )
        {
            var entry = sut.GetBinaryOperatorPrecedences().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().Be( value );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void SetBinaryOperatorPrecedence_WithMemory_ShouldRegisterPrecedence()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var value = Fixture.Create<int>();

        var result = sut.SetBinaryOperatorPrecedence( symbol.AsMemory(), value );

        using ( new AssertionScope() )
        {
            var entry = sut.GetBinaryOperatorPrecedences().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().Be( value );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void SetPrefixUnaryConstructPrecedence_WithString_ShouldRegisterPrecedence()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var value = Fixture.Create<int>();

        var result = sut.SetPrefixUnaryConstructPrecedence( symbol, value );

        using ( new AssertionScope() )
        {
            var entry = sut.GetPrefixUnaryConstructPrecedences().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().Be( value );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void SetPrefixUnaryConstructPrecedence_WithMemory_ShouldRegisterPrecedence()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var value = Fixture.Create<int>();

        var result = sut.SetPrefixUnaryConstructPrecedence( symbol.AsMemory(), value );

        using ( new AssertionScope() )
        {
            var entry = sut.GetPrefixUnaryConstructPrecedences().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().Be( value );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void SetPostfixUnaryConstructPrecedence_WithString_ShouldRegisterPrecedence()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var value = Fixture.Create<int>();

        var result = sut.SetPostfixUnaryConstructPrecedence( symbol, value );

        using ( new AssertionScope() )
        {
            var entry = sut.GetPostfixUnaryConstructPrecedences().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().Be( value );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void SetPostfixUnaryConstructPrecedence_WithMemory_ShouldRegisterPrecedence()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();
        var value = Fixture.Create<int>();

        var result = sut.SetPostfixUnaryConstructPrecedence( symbol.AsMemory(), value );

        using ( new AssertionScope() )
        {
            var entry = sut.GetPostfixUnaryConstructPrecedences().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().Be( value );
            result.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderIsEmpty()
    {
        var nonExistingSymbol = Fixture.Create<string>();
        var sut = new ParsedExpressionFactoryBuilder();

        var result = sut.Build();

        using ( new AssertionScope() )
        {
            result.Configuration.Should().NotBeNull();
            result.GetConstructSymbols().Should().BeEmpty();
            result.GetConstructType( nonExistingSymbol ).Should().Be( ParsedExpressionConstructType.None );
            result.GetGenericBinaryOperatorType( nonExistingSymbol ).Should().BeNull();
            result.GetSpecializedBinaryOperators( nonExistingSymbol ).Should().BeEmpty();
            result.GetGenericPrefixUnaryConstructType( nonExistingSymbol ).Should().BeNull();
            result.GetSpecializedPrefixUnaryConstructs( nonExistingSymbol ).Should().BeEmpty();
            result.GetGenericPostfixUnaryConstructType( nonExistingSymbol ).Should().BeNull();
            result.GetSpecializedPostfixUnaryConstructs( nonExistingSymbol ).Should().BeEmpty();
            result.GetTypeConverterTargetType( nonExistingSymbol ).Should().BeNull();
            result.GetTypeDeclarationType( nonExistingSymbol ).Should().BeNull();
            result.GetConstantExpression( nonExistingSymbol ).Should().BeNull();
            result.GetFunctionExpressions( nonExistingSymbol ).Should().BeEmpty();
            result.GetVariadicFunctionType( nonExistingSymbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( nonExistingSymbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( nonExistingSymbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( nonExistingSymbol ).Should().BeNull();
        }
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

        using ( new AssertionScope() )
        {
            result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeSequentiallyEqualTo( symbol );
            result.GetConstructType( symbol ).Should().Be( ParsedExpressionConstructType.BinaryOperator );
            result.GetGenericBinaryOperatorType( symbol ).Should().Be( typeof( ParsedExpressionAddOperator ) );
            result.GetSpecializedBinaryOperators( symbol ).Should().BeEmpty();
            result.GetGenericPrefixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPrefixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetGenericPostfixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPostfixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetTypeConverterTargetType( symbol ).Should().BeNull();
            result.GetTypeDeclarationType( symbol ).Should().BeNull();
            result.GetConstantExpression( symbol ).Should().BeNull();
            result.GetFunctionExpressions( symbol ).Should().BeEmpty();
            result.GetVariadicFunctionType( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol ).Should().Be( precedence );
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
        }
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

        using ( new AssertionScope() )
        {
            result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeSequentiallyEqualTo( symbol );
            result.GetConstructType( symbol ).Should().Be( ParsedExpressionConstructType.BinaryOperator );
            result.GetGenericBinaryOperatorType( symbol ).Should().BeNull();
            result.GetSpecializedBinaryOperators( symbol )
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        LeftArgumentType = typeof( bool ),
                        RightArgumentType = typeof( bool ),
                        OperatorType = typeof( ParsedExpressionAndOperator )
                    } );

            result.GetGenericPrefixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPrefixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetGenericPostfixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPostfixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetTypeConverterTargetType( symbol ).Should().BeNull();
            result.GetTypeDeclarationType( symbol ).Should().BeNull();
            result.GetConstantExpression( symbol ).Should().BeNull();
            result.GetFunctionExpressions( symbol ).Should().BeEmpty();
            result.GetVariadicFunctionType( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol ).Should().Be( precedence );
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
        }
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

        using ( new AssertionScope() )
        {
            result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeSequentiallyEqualTo( symbol );
            result.GetConstructType( symbol ).Should().Be( ParsedExpressionConstructType.PrefixUnaryOperator );
            result.GetGenericBinaryOperatorType( symbol ).Should().BeNull();
            result.GetSpecializedBinaryOperators( symbol ).Should().BeEmpty();
            result.GetGenericPrefixUnaryConstructType( symbol ).Should().Be( typeof( ParsedExpressionNegateOperator ) );
            result.GetSpecializedPrefixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetGenericPostfixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPostfixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetTypeConverterTargetType( symbol ).Should().BeNull();
            result.GetTypeDeclarationType( symbol ).Should().BeNull();
            result.GetConstantExpression( symbol ).Should().BeNull();
            result.GetFunctionExpressions( symbol ).Should().BeEmpty();
            result.GetVariadicFunctionType( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
        }
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

        using ( new AssertionScope() )
        {
            result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeSequentiallyEqualTo( symbol );
            result.GetConstructType( symbol ).Should().Be( ParsedExpressionConstructType.PrefixUnaryOperator );
            result.GetGenericBinaryOperatorType( symbol ).Should().BeNull();
            result.GetSpecializedBinaryOperators( symbol ).Should().BeEmpty();
            result.GetGenericPrefixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPrefixUnaryConstructs( symbol )
                .Should()
                .BeEquivalentTo( new { ArgumentType = typeof( bool ), ConstructType = typeof( ParsedExpressionNotOperator ) } );

            result.GetGenericPostfixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPostfixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetTypeConverterTargetType( symbol ).Should().BeNull();
            result.GetTypeDeclarationType( symbol ).Should().BeNull();
            result.GetConstantExpression( symbol ).Should().BeNull();
            result.GetFunctionExpressions( symbol ).Should().BeEmpty();
            result.GetVariadicFunctionType( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
        }
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

        using ( new AssertionScope() )
        {
            result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeSequentiallyEqualTo( symbol );
            result.GetConstructType( symbol ).Should().Be( ParsedExpressionConstructType.PostfixUnaryOperator );
            result.GetGenericBinaryOperatorType( symbol ).Should().BeNull();
            result.GetSpecializedBinaryOperators( symbol ).Should().BeEmpty();
            result.GetGenericPrefixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPrefixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetGenericPostfixUnaryConstructType( symbol ).Should().Be( typeof( ParsedExpressionNegateOperator ) );
            result.GetSpecializedPostfixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetTypeConverterTargetType( symbol ).Should().BeNull();
            result.GetTypeDeclarationType( symbol ).Should().BeNull();
            result.GetConstantExpression( symbol ).Should().BeNull();
            result.GetFunctionExpressions( symbol ).Should().BeEmpty();
            result.GetVariadicFunctionType( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
        }
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

        using ( new AssertionScope() )
        {
            result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeSequentiallyEqualTo( symbol );
            result.GetConstructType( symbol ).Should().Be( ParsedExpressionConstructType.PostfixUnaryOperator );
            result.GetGenericBinaryOperatorType( symbol ).Should().BeNull();
            result.GetSpecializedBinaryOperators( symbol ).Should().BeEmpty();
            result.GetGenericPrefixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPrefixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetGenericPostfixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPostfixUnaryConstructs( symbol )
                .Should()
                .BeEquivalentTo( new { ArgumentType = typeof( bool ), ConstructType = typeof( ParsedExpressionNotOperator ) } );

            result.GetTypeConverterTargetType( symbol ).Should().BeNull();
            result.GetTypeDeclarationType( symbol ).Should().BeNull();
            result.GetConstantExpression( symbol ).Should().BeNull();
            result.GetFunctionExpressions( symbol ).Should().BeEmpty();
            result.GetVariadicFunctionType( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
        }
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

        using ( new AssertionScope() )
        {
            result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeSequentiallyEqualTo( symbol );
            result.GetConstructType( symbol ).Should().Be( ParsedExpressionConstructType.PrefixTypeConverter );
            result.GetGenericBinaryOperatorType( symbol ).Should().BeNull();
            result.GetSpecializedBinaryOperators( symbol ).Should().BeEmpty();
            result.GetGenericPrefixUnaryConstructType( symbol ).Should().Be( typeof( ParsedExpressionTypeConverter<int> ) );
            result.GetSpecializedPrefixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetGenericPostfixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPostfixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetTypeConverterTargetType( symbol ).Should().Be( typeof( int ) );
            result.GetTypeDeclarationType( symbol ).Should().BeNull();
            result.GetConstantExpression( symbol ).Should().BeNull();
            result.GetFunctionExpressions( symbol ).Should().BeEmpty();
            result.GetVariadicFunctionType( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
        }
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

        using ( new AssertionScope() )
        {
            result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeSequentiallyEqualTo( symbol );
            result.GetConstructType( symbol ).Should().Be( ParsedExpressionConstructType.PrefixTypeConverter );
            result.GetGenericBinaryOperatorType( symbol ).Should().BeNull();
            result.GetSpecializedBinaryOperators( symbol ).Should().BeEmpty();
            result.GetGenericPrefixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPrefixUnaryConstructs( symbol )
                .Should()
                .BeEquivalentTo(
                    new { ArgumentType = typeof( long ), ConstructType = typeof( ParsedExpressionTypeConverter<int, long> ) } );

            result.GetGenericPostfixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPostfixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetTypeConverterTargetType( symbol ).Should().Be( typeof( int ) );
            result.GetTypeDeclarationType( symbol ).Should().BeNull();
            result.GetConstantExpression( symbol ).Should().BeNull();
            result.GetFunctionExpressions( symbol ).Should().BeEmpty();
            result.GetVariadicFunctionType( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
        }
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

        using ( new AssertionScope() )
        {
            result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeSequentiallyEqualTo( symbol );
            result.GetConstructType( symbol ).Should().Be( ParsedExpressionConstructType.PostfixTypeConverter );
            result.GetGenericBinaryOperatorType( symbol ).Should().BeNull();
            result.GetSpecializedBinaryOperators( symbol ).Should().BeEmpty();
            result.GetGenericPrefixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPrefixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetGenericPostfixUnaryConstructType( symbol ).Should().Be( typeof( ParsedExpressionTypeConverter<int> ) );
            result.GetSpecializedPostfixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetTypeConverterTargetType( symbol ).Should().Be( typeof( int ) );
            result.GetTypeDeclarationType( symbol ).Should().BeNull();
            result.GetConstantExpression( symbol ).Should().BeNull();
            result.GetFunctionExpressions( symbol ).Should().BeEmpty();
            result.GetVariadicFunctionType( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
        }
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

        using ( new AssertionScope() )
        {
            result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeSequentiallyEqualTo( symbol );
            result.GetConstructType( symbol ).Should().Be( ParsedExpressionConstructType.PostfixTypeConverter );
            result.GetGenericBinaryOperatorType( symbol ).Should().BeNull();
            result.GetSpecializedBinaryOperators( symbol ).Should().BeEmpty();
            result.GetGenericPrefixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPrefixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetGenericPostfixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPostfixUnaryConstructs( symbol )
                .Should()
                .BeEquivalentTo(
                    new { ArgumentType = typeof( long ), ConstructType = typeof( ParsedExpressionTypeConverter<int, long> ) } );

            result.GetTypeConverterTargetType( symbol ).Should().Be( typeof( int ) );
            result.GetTypeDeclarationType( symbol ).Should().BeNull();
            result.GetConstantExpression( symbol ).Should().BeNull();
            result.GetFunctionExpressions( symbol ).Should().BeEmpty();
            result.GetVariadicFunctionType( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
        }
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneConstant()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var constant = new ParsedExpressionConstant<int>( Fixture.Create<int>() );
        var sut = new ParsedExpressionFactoryBuilder()
            .AddConstant( symbol, constant );

        var result = sut.Build();

        using ( new AssertionScope() )
        {
            result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeSequentiallyEqualTo( symbol );
            result.GetConstructType( symbol ).Should().Be( ParsedExpressionConstructType.Constant );
            result.GetGenericBinaryOperatorType( symbol ).Should().BeNull();
            result.GetSpecializedBinaryOperators( symbol ).Should().BeEmpty();
            result.GetGenericPrefixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPrefixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetGenericPostfixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPostfixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetTypeConverterTargetType( symbol ).Should().BeNull();
            result.GetTypeDeclarationType( symbol ).Should().BeNull();
            result.GetConstantExpression( symbol ).Should().BeSameAs( constant.Expression );
            result.GetFunctionExpressions( symbol ).Should().BeEmpty();
            result.GetVariadicFunctionType( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
        }
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneTypeDeclaration()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var sut = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<int>( symbol );

        var result = sut.Build();

        using ( new AssertionScope() )
        {
            result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeSequentiallyEqualTo( symbol );
            result.GetConstructType( symbol ).Should().Be( ParsedExpressionConstructType.TypeDeclaration );
            result.GetGenericBinaryOperatorType( symbol ).Should().BeNull();
            result.GetSpecializedBinaryOperators( symbol ).Should().BeEmpty();
            result.GetGenericPrefixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPrefixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetGenericPostfixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPostfixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetTypeConverterTargetType( symbol ).Should().BeNull();
            result.GetTypeDeclarationType( symbol ).Should().Be( typeof( int ) );
            result.GetConstantExpression( symbol ).Should().BeNull();
            result.GetFunctionExpressions( symbol ).Should().BeEmpty();
            result.GetVariadicFunctionType( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
        }
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneFunction()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var function = new ParsedExpressionFunction<int>( () => Fixture.Create<int>() );
        var sut = new ParsedExpressionFactoryBuilder()
            .AddFunction( symbol, function );

        var result = sut.Build();

        using ( new AssertionScope() )
        {
            result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeSequentiallyEqualTo( symbol );
            result.GetConstructType( symbol ).Should().Be( ParsedExpressionConstructType.Function );
            result.GetGenericBinaryOperatorType( symbol ).Should().BeNull();
            result.GetSpecializedBinaryOperators( symbol ).Should().BeEmpty();
            result.GetGenericPrefixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPrefixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetGenericPostfixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPostfixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetTypeConverterTargetType( symbol ).Should().BeNull();
            result.GetTypeDeclarationType( symbol ).Should().BeNull();
            result.GetConstantExpression( symbol ).Should().BeNull();
            result.GetFunctionExpressions( symbol ).Should().HaveCount( 1 ).And.Subject.First().Should().BeSameAs( function.Lambda );
            result.GetVariadicFunctionType( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
        }
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneVariadicFunction()
    {
        var symbol = $"_{Fixture.Create<string>()}";
        var function = Substitute.ForPartsOf<ParsedExpressionVariadicFunction>();
        var sut = new ParsedExpressionFactoryBuilder()
            .AddVariadicFunction( symbol, function );

        var result = sut.Build();

        using ( new AssertionScope() )
        {
            result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeSequentiallyEqualTo( symbol );
            result.GetConstructType( symbol ).Should().Be( ParsedExpressionConstructType.VariadicFunction );
            result.GetGenericBinaryOperatorType( symbol ).Should().BeNull();
            result.GetSpecializedBinaryOperators( symbol ).Should().BeEmpty();
            result.GetGenericPrefixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPrefixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetGenericPostfixUnaryConstructType( symbol ).Should().BeNull();
            result.GetSpecializedPostfixUnaryConstructs( symbol ).Should().BeEmpty();
            result.GetTypeConverterTargetType( symbol ).Should().BeNull();
            result.GetTypeDeclarationType( symbol ).Should().BeNull();
            result.GetConstantExpression( symbol ).Should().BeNull();
            result.GetFunctionExpressions( symbol ).Should().BeEmpty();
            result.GetVariadicFunctionType( symbol ).Should().Be( function.GetType() );
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
        }
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneOfEachConstructsWithPrecedence()
    {
        var (operatorSymbol, typeConverterSymbol, constantSymbol, typeDeclarationSymbol, functionSymbol, variadicFunctionSymbol) =
            Fixture.CreateDistinctCollection<string>( count: 6 ).Select( s => $"_{s}" ).ToList();

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
            .AddFunction( functionSymbol, new ParsedExpressionFunction<double, int>( a => (int)a ) )
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
            .Should()
            .BeEquivalentTo(
                operatorSymbol,
                typeConverterSymbol,
                constantSymbol,
                typeDeclarationSymbol,
                functionSymbol,
                variadicFunctionSymbol );
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

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenAtLeastOneConstructSymbolIsInvalid()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( string.Empty, new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( string.Empty, 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenOperatorDefinitionContainsNonOperatorConstruct()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .SetBinaryOperatorPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericBinaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenSpecializedBinaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddDecimalOperator() )
            .AddBinaryOperator( "+", new ParsedExpressionAddDecimalOperator() )
            .SetBinaryOperatorPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenBinaryOperatorPrecedenceIsUndefined()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericPrefixUnaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "+", new ParsedExpressionNegateOperator() )
            .AddPrefixUnaryOperator( "+", new ParsedExpressionNegateOperator() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
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

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPrefixUnaryOperatorPrecedenceIsUndefined()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "+", new ParsedExpressionNegateOperator() );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericPostfixUnaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "+", new ParsedExpressionNegateOperator() )
            .AddPostfixUnaryOperator( "+", new ParsedExpressionNegateOperator() )
            .SetPostfixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
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

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPostfixUnaryOperatorPrecedenceIsUndefined()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "+", new ParsedExpressionNegateOperator() );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenTypeConverterDefinitionContainsNonTypeConverterConstruct()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
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

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericPrefixTypeConverterIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
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

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPrefixTypeConverterPrecedenceIsUndefined()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
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

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericPostfixTypeConverterIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() )
            .SetPostfixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
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

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPostfixTypeConverterPrecedenceIsUndefined()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
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

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenConstantDefinitionContainsMoreThanOneConstant()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddConstant( "e", new ParsedExpressionConstant<int>( Fixture.Create<int>() ) )
            .AddConstant( "e", new ParsedExpressionConstant<int>( Fixture.Create<int>() ) );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenConstantDefinitionContainsNonConstantConstruct()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddConstant( "e", new ParsedExpressionConstant<int>( Fixture.Create<int>() ) )
            .AddBinaryOperator( "e", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "e", 1 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenTypeDeclarationDefinitionContainsMoreThanOneTypeDeclaration()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<int>( "e" )
            .AddTypeDeclaration<long>( "e" );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenTypeDeclarationDefinitionContainsNonTypeDeclarationConstruct()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<int>( "e" )
            .AddBinaryOperator( "e", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "e", 1 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenFunctionSignatureIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddFunction( "f", new ParsedExpressionFunction<int>( () => Fixture.Create<int>() ) )
            .AddFunction( "f", new ParsedExpressionFunction<int>( () => Fixture.Create<int>() ) );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenFunctionDefinitionContainsNonFunctionConstruct()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddFunction( "f", new ParsedExpressionFunction<int>( () => Fixture.Create<int>() ) )
            .AddBinaryOperator( "f", new ParsedExpressionAddOperator() );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenVariadicFunctionDefinitionContainsMoreThanOneFunction()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddVariadicFunction( "e", Substitute.ForPartsOf<ParsedExpressionVariadicFunction>() )
            .AddVariadicFunction( "e", Substitute.ForPartsOf<ParsedExpressionVariadicFunction>() );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenVariadicFunctionDefinitionContainsNonVariadicFunctionConstruct()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddVariadicFunction( "e", Substitute.ForPartsOf<ParsedExpressionVariadicFunction>() )
            .AddBinaryOperator( "e", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "e", 1 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WithMultipleMessages_WhenMultipleErrorsOccur()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddPrefixTypeConverter( "+", new ParsedExpressionTypeConverter<int>() );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<ParsedExpressionFactoryBuilderException>().AndMatch( e => e.Messages.Count > 1 );
    }
}
