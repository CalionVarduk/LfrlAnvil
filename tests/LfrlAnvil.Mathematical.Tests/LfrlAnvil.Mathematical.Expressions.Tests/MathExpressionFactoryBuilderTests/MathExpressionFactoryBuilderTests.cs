using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Mathematical.Expressions.Constructs;
using LfrlAnvil.Mathematical.Expressions.Constructs.Boolean;
using LfrlAnvil.Mathematical.Expressions.Constructs.Decimal;
using LfrlAnvil.Mathematical.Expressions.Exceptions;
using LfrlAnvil.Mathematical.Expressions.Internal;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Mathematical.Expressions.Tests.MathExpressionFactoryBuilderTests;

public class MathExpressionFactoryBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyBuilder()
    {
        var sut = new MathExpressionFactoryBuilder();

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
        var sut = new MathExpressionFactoryBuilder();
        var configuration = new MathExpressionFactoryDefaultConfiguration();

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
        var sut = new MathExpressionFactoryBuilder();
        var configuration = new MathExpressionFactoryDefaultConfiguration();
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
        var sut = new MathExpressionFactoryBuilder();
        var @delegate = Lambda.Of(
            (MathExpressionNumberParserParams p) => MathExpressionNumberParser.CreateDefaultDecimal( p.Configuration ) );

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
        var sut = new MathExpressionFactoryBuilder();
        var @delegate = Lambda.Of(
            (MathExpressionNumberParserParams p) => MathExpressionNumberParser.CreateDefaultDecimal( p.Configuration ) );

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
        var sut = new MathExpressionFactoryBuilder();
        var @operator = new MathExpressionAddOperator();

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
        var sut = new MathExpressionFactoryBuilder();
        var @operator = new MathExpressionAddOperator();

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
        var sut = new MathExpressionFactoryBuilder();
        var @operator = new MathExpressionNegateOperator();

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
        var sut = new MathExpressionFactoryBuilder();
        var @operator = new MathExpressionNegateOperator();

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
        var sut = new MathExpressionFactoryBuilder();
        var @operator = new MathExpressionNegateOperator();

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
        var sut = new MathExpressionFactoryBuilder();
        var @operator = new MathExpressionNegateOperator();

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
        var sut = new MathExpressionFactoryBuilder();
        var converter = new MathExpressionTypeConverter<int>();

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
        var sut = new MathExpressionFactoryBuilder();
        var converter = new MathExpressionTypeConverter<int>();

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
        var sut = new MathExpressionFactoryBuilder();
        var converter = new MathExpressionTypeConverter<int>();

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
        var sut = new MathExpressionFactoryBuilder();
        var converter = new MathExpressionTypeConverter<int>();

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
    public void SetBinaryOperatorPrecedence_WithString_ShouldRegisterPrecedence()
    {
        var symbol = Fixture.Create<string>();
        var sut = new MathExpressionFactoryBuilder();
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
        var sut = new MathExpressionFactoryBuilder();
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
        var sut = new MathExpressionFactoryBuilder();
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
        var sut = new MathExpressionFactoryBuilder();
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
        var sut = new MathExpressionFactoryBuilder();
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
        var sut = new MathExpressionFactoryBuilder();
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
        var sut = new MathExpressionFactoryBuilder();

        var result = sut.Build();

        using ( new AssertionScope() )
        {
            result.Configuration.Should().NotBeNull();
            result.GetConstructSymbols().Should().BeEmpty();
            result.ContainsConstructSymbol( nonExistingSymbol ).Should().BeFalse();
            result.ContainsConstructSymbol( nonExistingSymbol.AsMemory() ).Should().BeFalse();
            result.IsFunctionSymbol( nonExistingSymbol ).Should().BeFalse();
            result.IsFunctionSymbol( nonExistingSymbol.AsMemory() ).Should().BeFalse();
            result.IsOperatorSymbol( nonExistingSymbol ).Should().BeFalse();
            result.IsOperatorSymbol( nonExistingSymbol.AsMemory() ).Should().BeFalse();
            result.IsTypeConverterSymbol( nonExistingSymbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( nonExistingSymbol.AsMemory() ).Should().BeFalse();
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
        var @operator = new MathExpressionAddOperator();
        var sut = new MathExpressionFactoryBuilder()
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
            result.IsOperatorSymbol( symbol ).Should().BeTrue();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
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
        var @operator = new MathExpressionAndOperator();
        var sut = new MathExpressionFactoryBuilder()
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
            result.IsOperatorSymbol( symbol ).Should().BeTrue();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
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
        var @operator = new MathExpressionNegateOperator();
        var sut = new MathExpressionFactoryBuilder()
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
            result.IsOperatorSymbol( symbol ).Should().BeTrue();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
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
        var @operator = new MathExpressionNotOperator();
        var sut = new MathExpressionFactoryBuilder()
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
            result.IsOperatorSymbol( symbol ).Should().BeTrue();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
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
        var @operator = new MathExpressionNegateOperator();
        var sut = new MathExpressionFactoryBuilder()
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
            result.IsOperatorSymbol( symbol ).Should().BeTrue();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
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
        var @operator = new MathExpressionNotOperator();
        var sut = new MathExpressionFactoryBuilder()
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
            result.IsOperatorSymbol( symbol ).Should().BeTrue();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeFalse();
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
        var converter = new MathExpressionTypeConverter<int>();
        var sut = new MathExpressionFactoryBuilder()
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
            result.IsOperatorSymbol( symbol ).Should().BeFalse();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeTrue();
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
        var converter = new MathExpressionTypeConverter<int, long>();
        var sut = new MathExpressionFactoryBuilder()
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
            result.IsOperatorSymbol( symbol ).Should().BeFalse();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeTrue();
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
        var converter = new MathExpressionTypeConverter<int>();
        var sut = new MathExpressionFactoryBuilder()
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
            result.IsOperatorSymbol( symbol ).Should().BeFalse();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeTrue();
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
        var converter = new MathExpressionTypeConverter<int, long>();
        var sut = new MathExpressionFactoryBuilder()
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
            result.IsOperatorSymbol( symbol ).Should().BeFalse();
            result.IsOperatorSymbol( symbol.AsMemory() ).Should().BeFalse();
            result.IsTypeConverterSymbol( symbol ).Should().BeTrue();
            result.IsTypeConverterSymbol( symbol.AsMemory() ).Should().BeTrue();
            result.GetBinaryOperatorPrecedence( symbol ).Should().BeNull();
            result.GetBinaryOperatorPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol ).Should().BeNull();
            result.GetPrefixUnaryConstructPrecedence( symbol.AsMemory() ).Should().BeNull();
            result.GetPostfixUnaryConstructPrecedence( symbol ).Should().Be( precedence );
            result.GetPostfixUnaryConstructPrecedence( symbol.AsMemory() ).Should().Be( precedence );
        }
    }

    [Fact]
    public void Build_ShouldReturnValidFactory_WhenBuilderHasOneOfEachConstructsWithPrecedence()
    {
        var (operatorSymbol, typeConverterSymbol) = Fixture.CreateDistinctCollection<string>( count: 2 ).Select( s => $"_{s}" ).ToList();
        var precedence = Fixture.Create<int>();
        var sut = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( operatorSymbol, new MathExpressionAddOperator() )
            .AddBinaryOperator( operatorSymbol, new MathExpressionAddDecimalOperator() )
            .AddPrefixUnaryOperator( operatorSymbol, new MathExpressionNegateOperator() )
            .AddPrefixUnaryOperator( operatorSymbol, new MathExpressionNegateDecimalOperator() )
            .AddPostfixUnaryOperator( operatorSymbol, new MathExpressionNegateOperator() )
            .AddPostfixUnaryOperator( operatorSymbol, new MathExpressionNegateDecimalOperator() )
            .AddPrefixTypeConverter( typeConverterSymbol, new MathExpressionTypeConverter<int>() )
            .AddPrefixTypeConverter( typeConverterSymbol, new MathExpressionTypeConverter<int, long>() )
            .AddPostfixTypeConverter( typeConverterSymbol, new MathExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( typeConverterSymbol, new MathExpressionTypeConverter<int, long>() )
            .SetBinaryOperatorPrecedence( operatorSymbol, precedence )
            .SetPrefixUnaryConstructPrecedence( operatorSymbol, precedence )
            .SetPostfixUnaryConstructPrecedence( operatorSymbol, precedence )
            .SetPrefixUnaryConstructPrecedence( typeConverterSymbol, precedence )
            .SetPostfixUnaryConstructPrecedence( typeConverterSymbol, precedence );

        var result = sut.Build();

        result.GetConstructSymbols().Select( s => s.ToString() ).Should().BeEquivalentTo( operatorSymbol, typeConverterSymbol );
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
        var configuration = Substitute.For<IMathExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( decimalPoint );
        configuration.IntegerDigitSeparator.Returns( integerDigitSeparator );
        configuration.ScientificNotationExponents.Returns( scientificNotationExponents );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.AllowScientificNotation.Returns( allowScientificNotation );
        configuration.StringDelimiter.Returns( stringDelimiter );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( true );

        var sut = new MathExpressionFactoryBuilder()
            .SetConfiguration( configuration );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenAtLeastOneConstructSymbolIsInvalid()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( string.Empty, new MathExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( string.Empty, 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenOperatorDefinitionContainsNonOperatorConstruct()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .AddPrefixTypeConverter( "+", new MathExpressionTypeConverter<int>() )
            .SetBinaryOperatorPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericBinaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenSpecializedBinaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddDecimalOperator() )
            .AddBinaryOperator( "+", new MathExpressionAddDecimalOperator() )
            .SetBinaryOperatorPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenBinaryOperatorPrecedenceIsUndefined()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericPrefixUnaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "+", new MathExpressionNegateOperator() )
            .AddPrefixUnaryOperator( "+", new MathExpressionNegateOperator() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void
        Build_ShouldThrowMathExpressionFactoryBuilderException_WhenSpecializedPrefixUnaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "+", new MathExpressionNegateDecimalOperator() )
            .AddPrefixUnaryOperator( "+", new MathExpressionNegateDecimalOperator() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPrefixUnaryOperatorPrecedenceIsUndefined()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "+", new MathExpressionNegateOperator() );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericPostfixUnaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "+", new MathExpressionNegateOperator() )
            .AddPostfixUnaryOperator( "+", new MathExpressionNegateOperator() )
            .SetPostfixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void
        Build_ShouldThrowMathExpressionFactoryBuilderException_WhenSpecializedPostfixUnaryOperatorIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "+", new MathExpressionNegateDecimalOperator() )
            .AddPostfixUnaryOperator( "+", new MathExpressionNegateDecimalOperator() )
            .SetPostfixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPostfixUnaryOperatorPrecedenceIsUndefined()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "+", new MathExpressionNegateOperator() );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenTypeConverterDefinitionContainsNonTypeConverterConstruct()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new MathExpressionTypeConverter<int>() )
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void
        Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPrefixAndPostfixTypeConverterCollectionsHaveDifferentTargetTypeWithinTheSameDefinition()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new MathExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( "+", new MathExpressionTypeConverter<decimal>() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 )
            .SetPostfixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericPrefixTypeConverterIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new MathExpressionTypeConverter<int>() )
            .AddPrefixTypeConverter( "+", new MathExpressionTypeConverter<int>() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void
        Build_ShouldThrowMathExpressionFactoryBuilderException_WhenSpecializedPrefixTypeConverterIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new MathExpressionTypeConverter<int, long>() )
            .AddPrefixTypeConverter( "+", new MathExpressionTypeConverter<int, long>() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPrefixTypeConverterPrecedenceIsUndefined()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new MathExpressionTypeConverter<int>() );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void
        Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPrefixTypeConvertersHaveDifferentTargetTypeWithinTheSameCollection()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "+", new MathExpressionTypeConverter<int>() )
            .AddPrefixTypeConverter( "+", new MathExpressionTypeConverter<long, int>() )
            .SetPrefixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenGenericPostfixTypeConverterIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "+", new MathExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( "+", new MathExpressionTypeConverter<int>() )
            .SetPostfixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void
        Build_ShouldThrowMathExpressionFactoryBuilderException_WhenSpecializedPostfixTypeConverterIsDuplicatedWithinTheSameDefinition()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "+", new MathExpressionTypeConverter<int, long>() )
            .AddPostfixTypeConverter( "+", new MathExpressionTypeConverter<int, long>() )
            .SetPostfixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPostfixTypeConverterPrecedenceIsUndefined()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "+", new MathExpressionTypeConverter<int>() );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void
        Build_ShouldThrowMathExpressionFactoryBuilderException_WhenPostfixTypeConvertersHaveDifferentTargetTypeWithinTheSameCollection()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "+", new MathExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( "+", new MathExpressionTypeConverter<long, int>() )
            .SetPostfixUnaryConstructPrecedence( "+", 0 );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>();
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionFactoryBuilderException_WithMultipleMessages_WhenMultipleErrorsOccur()
    {
        var sut = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .AddPrefixTypeConverter( "+", new MathExpressionTypeConverter<int>() );

        var action = Lambda.Of( () => sut.Build() );

        action.Should().ThrowExactly<MathExpressionFactoryBuilderException>().AndMatch( e => e.Messages.Count > 1 );
    }
}
