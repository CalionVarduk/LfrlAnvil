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
            sut.GetCurrentConfiguration().Should().BeNull();
            sut.GetCurrentNumberParserProvider().Should().BeNull();
            sut.GetCurrentConstructs().Should().BeEmpty();
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
            sut.GetCurrentConfiguration().Should().BeSameAs( configuration );
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
            sut.GetCurrentConfiguration().Should().BeNull();
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
            sut.GetCurrentNumberParserProvider().Should().BeSameAs( @delegate );
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
            sut.GetCurrentNumberParserProvider().Should().BeNull();
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( @operator );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( @operator );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( @operator );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( @operator );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( @operator );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( @operator );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( converter );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( converter );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( converter );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( converter );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( constant );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( constant );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( typeof( int ) );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( typeof( int ) );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( function );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( function );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( function );
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
            var entry = sut.GetCurrentConstructs().Should().HaveCount( 1 ).And.Subject.First();
            entry.Key.ToString().Should().Be( symbol );
            entry.Value.Should().BeSameAs( function );
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
            var entry = sut.GetCurrentBinaryOperatorPrecedences().Should().HaveCount( 1 ).And.Subject.First();
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
            var entry = sut.GetCurrentBinaryOperatorPrecedences().Should().HaveCount( 1 ).And.Subject.First();
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
            var entry = sut.GetCurrentPrefixUnaryConstructPrecedences().Should().HaveCount( 1 ).And.Subject.First();
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
            var entry = sut.GetCurrentPrefixUnaryConstructPrecedences().Should().HaveCount( 1 ).And.Subject.First();
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
            var entry = sut.GetCurrentPostfixUnaryConstructPrecedences().Should().HaveCount( 1 ).And.Subject.First();
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
            var entry = sut.GetCurrentPostfixUnaryConstructPrecedences().Should().HaveCount( 1 ).And.Subject.First();
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
            result.ContainsConstructSymbol( nonExistingSymbol ).Should().BeFalse();
            result.ContainsConstructSymbol( nonExistingSymbol.AsMemory() ).Should().BeFalse();
            result.IsFunctionSymbol( nonExistingSymbol ).Should().BeFalse();
            result.IsFunctionSymbol( nonExistingSymbol.AsMemory() ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( nonExistingSymbol ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( nonExistingSymbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( nonExistingSymbol ).Should().BeFalse();
            result.IsOperatorSymbol( nonExistingSymbol.AsMemory() ).Should().BeFalse();
            result.IsTypeConverterSymbol( nonExistingSymbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( nonExistingSymbol.AsMemory() ).Should().BeFalse();
            result.IsConstantSymbol( nonExistingSymbol ).Should().BeFalse();
            result.IsConstantSymbol( nonExistingSymbol.AsMemory() ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( nonExistingSymbol ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( nonExistingSymbol.AsMemory() ).Should().BeFalse();
            result.GetBinaryOperatorPrecedence( nonExistingSymbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( nonExistingSymbol.AsMemory() ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( nonExistingSymbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( nonExistingSymbol.AsMemory() ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( nonExistingSymbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( nonExistingSymbol.AsMemory() ).Should().BeNull();
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
            result.ContainsConstructSymbol( symbol ).Should().BeTrue();
            result.ContainsConstructSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsFunctionSymbol( symbol ).Should().BeFalse();
            result.IsFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( symbol ).Should().BeTrue();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsConstantSymbol( symbol ).Should().BeFalse();
            result.IsConstantSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.GetBinaryOperatorPrecedence( symbol ).Should().Be( precedence );
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().Be( precedence );
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
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
            result.ContainsConstructSymbol( symbol ).Should().BeTrue();
            result.ContainsConstructSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsFunctionSymbol( symbol ).Should().BeFalse();
            result.IsFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( symbol ).Should().BeTrue();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsConstantSymbol( symbol ).Should().BeFalse();
            result.IsConstantSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.GetBinaryOperatorPrecedence( symbol ).Should().Be( precedence );
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().Be( precedence );
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
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
            result.ContainsConstructSymbol( symbol ).Should().BeTrue();
            result.ContainsConstructSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsFunctionSymbol( symbol ).Should().BeFalse();
            result.IsFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( symbol ).Should().BeTrue();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsConstantSymbol( symbol ).Should().BeFalse();
            result.IsConstantSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().Be( precedence );
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
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
            result.ContainsConstructSymbol( symbol ).Should().BeTrue();
            result.ContainsConstructSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsFunctionSymbol( symbol ).Should().BeFalse();
            result.IsFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( symbol ).Should().BeTrue();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsConstantSymbol( symbol ).Should().BeFalse();
            result.IsConstantSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().Be( precedence );
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
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
            result.ContainsConstructSymbol( symbol ).Should().BeTrue();
            result.ContainsConstructSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsFunctionSymbol( symbol ).Should().BeFalse();
            result.IsFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( symbol ).Should().BeTrue();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsConstantSymbol( symbol ).Should().BeFalse();
            result.IsConstantSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().Be( precedence );
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
            result.ContainsConstructSymbol( symbol ).Should().BeTrue();
            result.ContainsConstructSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsFunctionSymbol( symbol ).Should().BeFalse();
            result.IsFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( symbol ).Should().BeTrue();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsConstantSymbol( symbol ).Should().BeFalse();
            result.IsConstantSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().Be( precedence );
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
            result.ContainsConstructSymbol( symbol ).Should().BeTrue();
            result.ContainsConstructSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsFunctionSymbol( symbol ).Should().BeFalse();
            result.IsFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( symbol ).Should().BeFalse();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsConstantSymbol( symbol ).Should().BeFalse();
            result.IsConstantSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().Be( precedence );
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
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
            result.ContainsConstructSymbol( symbol ).Should().BeTrue();
            result.ContainsConstructSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsFunctionSymbol( symbol ).Should().BeFalse();
            result.IsFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( symbol ).Should().BeFalse();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsConstantSymbol( symbol ).Should().BeFalse();
            result.IsConstantSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().Be( precedence );
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
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
            result.ContainsConstructSymbol( symbol ).Should().BeTrue();
            result.ContainsConstructSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsFunctionSymbol( symbol ).Should().BeFalse();
            result.IsFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( symbol ).Should().BeFalse();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsConstantSymbol( symbol ).Should().BeFalse();
            result.IsConstantSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().Be( precedence );
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
            result.ContainsConstructSymbol( symbol ).Should().BeTrue();
            result.ContainsConstructSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsFunctionSymbol( symbol ).Should().BeFalse();
            result.IsFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( symbol ).Should().BeFalse();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsConstantSymbol( symbol ).Should().BeFalse();
            result.IsConstantSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().Be( precedence );
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
            result.ContainsConstructSymbol( symbol ).Should().BeTrue();
            result.ContainsConstructSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsFunctionSymbol( symbol ).Should().BeFalse();
            result.IsFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( symbol ).Should().BeFalse();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsConstantSymbol( symbol ).Should().BeTrue();
            result.IsConstantSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsTypeDeclarationSymbol( symbol ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
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
            result.ContainsConstructSymbol( symbol ).Should().BeTrue();
            result.ContainsConstructSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsFunctionSymbol( symbol ).Should().BeFalse();
            result.IsFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( symbol ).Should().BeFalse();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsConstantSymbol( symbol ).Should().BeFalse();
            result.IsConstantSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol ).Should().BeTrue();
            result.IsTypeDeclarationSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
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
            result.ContainsConstructSymbol( symbol ).Should().BeTrue();
            result.ContainsConstructSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsFunctionSymbol( symbol ).Should().BeTrue();
            result.IsFunctionSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsVariadicFunctionSymbol( symbol ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( symbol ).Should().BeFalse();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsConstantSymbol( symbol ).Should().BeFalse();
            result.IsConstantSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
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
            result.ContainsConstructSymbol( symbol ).Should().BeTrue();
            result.ContainsConstructSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsFunctionSymbol( symbol ).Should().BeFalse();
            result.IsFunctionSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsVariadicFunctionSymbol( symbol ).Should().BeTrue();
            result.IsVariadicFunctionSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsOperatorSymbol( symbol ).Should().BeFalse();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsConstantSymbol( symbol ).Should().BeFalse();
            result.IsConstantSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol ).Should().BeFalse();
            result.IsTypeDeclarationSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
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
