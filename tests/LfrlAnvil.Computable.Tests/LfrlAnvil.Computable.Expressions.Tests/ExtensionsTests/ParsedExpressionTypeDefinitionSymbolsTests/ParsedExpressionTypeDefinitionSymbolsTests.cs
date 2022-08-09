using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Extensions;

namespace LfrlAnvil.Computable.Expressions.Tests.ExtensionsTests.ParsedExpressionTypeDefinitionSymbolsTests;

public class ParsedExpressionTypeDefinitionSymbolsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnWithEmptyName()
    {
        var sut = default( ParsedExpressionTypeDefinitionSymbols );

        using ( new AssertionScope() )
        {
            sut.Name.ToString().Should().BeEmpty();
            sut.PrefixTypeConverter.Should().NotBeNull().And.Subject.ToString().Should().Be( "[]" );
            sut.PostfixTypeConverter.Should().BeNull();
        }
    }

    [Fact]
    public void Empty_ShouldReturnWithEmptyName()
    {
        var sut = ParsedExpressionTypeDefinitionSymbols.Empty;

        using ( new AssertionScope() )
        {
            sut.Name.ToString().Should().BeEmpty();
            sut.PrefixTypeConverter.Should().NotBeNull().And.Subject.ToString().Should().Be( "[]" );
            sut.PostfixTypeConverter.Should().BeNull();
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenAllSymbolsAreSet()
    {
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "name" )
            .SetPrefixTypeConverter( "prefix" )
            .SetPostfixTypeConverter( "postfix" );

        var result = sut.ToString();

        result.Should().Be( "Name: 'name', PrefixTypeConverter: 'prefix', PostfixTypeConverter: 'postfix'" );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenOnlyNameIsSet()
    {
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "name" )
            .DisablePrefixTypeConverter();

        var result = sut.ToString();

        result.Should().Be( "Name: 'name'" );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenOnlyNameAndPrefixTypeConverterAreSet()
    {
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "name" )
            .SetPrefixTypeConverter( "prefix" );

        var result = sut.ToString();

        result.Should().Be( "Name: 'name', PrefixTypeConverter: 'prefix'" );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenOnlyNameAndPostfixTypeConverterAreSet()
    {
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "name" )
            .DisablePrefixTypeConverter()
            .SetPostfixTypeConverter( "postfix" );

        var result = sut.ToString();

        result.Should().Be( "Name: 'name', PostfixTypeConverter: 'postfix'" );
    }

    [Fact]
    public void SetName_ShouldUpdateNameAndPrefixTypeConverter_WhenPrefixTypeConverterIsEnabledAndIsNotSetToCustomValue()
    {
        var name = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols().SetName( name );

        using ( new AssertionScope() )
        {
            sut.Name.ToString().Should().Be( name );
            sut.PrefixTypeConverter.Should().NotBeNull().And.Subject.ToString().Should().Be( $"[{name}]" );
            sut.PostfixTypeConverter.Should().BeNull();
        }
    }

    [Fact]
    public void DisablePrefixTypeConverter_ShouldUpdatePrefixTypeConverterToNull_WhenPrefixTypeConverterIsEnabledAndIsNotSetToCustomValue()
    {
        var sut = new ParsedExpressionTypeDefinitionSymbols().DisablePrefixTypeConverter();

        using ( new AssertionScope() )
        {
            sut.Name.ToString().Should().BeEmpty();
            sut.PrefixTypeConverter.Should().BeNull();
            sut.PostfixTypeConverter.Should().BeNull();
        }
    }

    [Fact]
    public void DisablePrefixTypeConverter_ShouldUpdatePrefixTypeConverterToNull_WhenPrefixTypeConverterHasCustomValue()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetPrefixTypeConverter( symbol )
            .DisablePrefixTypeConverter();

        using ( new AssertionScope() )
        {
            sut.Name.ToString().Should().BeEmpty();
            sut.PrefixTypeConverter.Should().BeNull();
            sut.PostfixTypeConverter.Should().BeNull();
        }
    }

    [Fact]
    public void SetPrefixTypeConverter_ShouldUpdatePrefixTypeConverter_WhenPrefixTypeConverterIsEnabled()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols().SetPrefixTypeConverter( symbol );

        using ( new AssertionScope() )
        {
            sut.Name.ToString().Should().BeEmpty();
            sut.PrefixTypeConverter.Should().NotBeNull().And.Subject.ToString().Should().Be( symbol );
            sut.PostfixTypeConverter.Should().BeNull();
        }
    }

    [Fact]
    public void SetPrefixTypeConverter_ShouldUpdatePrefixTypeConverter_WhenPrefixTypeConverterIsDisabled()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .DisablePrefixTypeConverter()
            .SetPrefixTypeConverter( symbol );

        using ( new AssertionScope() )
        {
            sut.Name.ToString().Should().BeEmpty();
            sut.PrefixTypeConverter.Should().NotBeNull().And.Subject.ToString().Should().Be( symbol );
            sut.PostfixTypeConverter.Should().BeNull();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateNameOnly_WhenPrefixTypeConverterIsSetToCustomValue()
    {
        var (name, prefixSymbol) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetPrefixTypeConverter( prefixSymbol )
            .SetName( name );

        using ( new AssertionScope() )
        {
            sut.Name.ToString().Should().Be( name );
            sut.PrefixTypeConverter.Should().NotBeNull().And.Subject.ToString().Should().Be( prefixSymbol );
            sut.PostfixTypeConverter.Should().BeNull();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateNameOnly_WhenPrefixTypeConverterIsDisabled()
    {
        var name = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .DisablePrefixTypeConverter()
            .SetName( name );

        using ( new AssertionScope() )
        {
            sut.Name.ToString().Should().Be( name );
            sut.PrefixTypeConverter.Should().BeNull();
            sut.PostfixTypeConverter.Should().BeNull();
        }
    }

    [Fact]
    public void SetDefaultPrefixTypeConverter_ShouldUpdatePrefixTypeConverter_WhenPrefixTypeConverterIsSetToCustomValue()
    {
        var (name, prefixSymbol) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( name )
            .SetPrefixTypeConverter( prefixSymbol )
            .SetDefaultPrefixTypeConverter();

        using ( new AssertionScope() )
        {
            sut.Name.ToString().Should().Be( name );
            sut.PrefixTypeConverter.Should().NotBeNull().And.Subject.ToString().Should().Be( $"[{name}]" );
            sut.PostfixTypeConverter.Should().BeNull();
        }
    }

    [Fact]
    public void SetDefaultPrefixTypeConverter_ShouldUpdatePrefixTypeConverter_WhenPrefixTypeConverterIsDisabled()
    {
        var name = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( name )
            .DisablePrefixTypeConverter()
            .SetDefaultPrefixTypeConverter();

        using ( new AssertionScope() )
        {
            sut.Name.ToString().Should().Be( name );
            sut.PrefixTypeConverter.Should().NotBeNull().And.Subject.ToString().Should().Be( $"[{name}]" );
            sut.PostfixTypeConverter.Should().BeNull();
        }
    }

    [Fact]
    public void SetPostfixTypeConverter_ShouldUpdatePostfixTypeConverter()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetPostfixTypeConverter( symbol );

        using ( new AssertionScope() )
        {
            sut.Name.ToString().Should().BeEmpty();
            sut.PrefixTypeConverter.Should().NotBeNull().And.Subject.ToString().Should().Be( "[]" );
            sut.PostfixTypeConverter.Should().NotBeNull().And.Subject.ToString().Should().Be( symbol );
        }
    }

    [Fact]
    public void DisablePostfixTypeConverter_ShouldUpdatePostfixTypeConverterToNull_WhenPostfixTypeConverterHasValue()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetPostfixTypeConverter( symbol )
            .DisablePostfixTypeConverter();

        using ( new AssertionScope() )
        {
            sut.Name.ToString().Should().BeEmpty();
            sut.PrefixTypeConverter.Should().NotBeNull().And.Subject.ToString().Should().Be( "[]" );
            sut.PostfixTypeConverter.Should().BeNull();
        }
    }
}
