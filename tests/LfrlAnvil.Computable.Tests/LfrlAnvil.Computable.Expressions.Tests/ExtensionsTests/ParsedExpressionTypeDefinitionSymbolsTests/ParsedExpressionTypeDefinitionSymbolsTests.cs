using LfrlAnvil.Computable.Expressions.Extensions;

namespace LfrlAnvil.Computable.Expressions.Tests.ExtensionsTests.ParsedExpressionTypeDefinitionSymbolsTests;

public class ParsedExpressionTypeDefinitionSymbolsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnWithEmptyName()
    {
        var sut = default( ParsedExpressionTypeDefinitionSymbols );

        Assertion.All(
                sut.Name.ToString().TestEmpty(),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( "[]" ),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( string.Empty ) )
            .Go();
    }

    [Fact]
    public void Empty_ShouldReturnWithEmptyName()
    {
        var sut = ParsedExpressionTypeDefinitionSymbols.Empty;

        Assertion.All(
                sut.Name.ToString().TestEmpty(),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( "[]" ),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( string.Empty ) )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenAllSymbolsAreSet()
    {
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "name" )
            .SetPrefixTypeConverter( "prefix" )
            .SetPostfixTypeConverter( "postfix" )
            .SetConstant( "constant" );

        var result = sut.ToString();

        result.TestEquals( "Name: 'name', PrefixTypeConverter: 'prefix', PostfixTypeConverter: 'postfix', Constant: 'constant'" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenOnlyNameIsSet()
    {
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "name" )
            .DisablePrefixTypeConverter()
            .DisableConstant();

        var result = sut.ToString();

        result.TestEquals( "Name: 'name'" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenOnlyNameAndPrefixTypeConverterAreSet()
    {
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "name" )
            .SetPrefixTypeConverter( "prefix" )
            .DisableConstant();

        var result = sut.ToString();

        result.TestEquals( "Name: 'name', PrefixTypeConverter: 'prefix'" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenOnlyNameAndPostfixTypeConverterAreSet()
    {
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "name" )
            .DisablePrefixTypeConverter()
            .DisableConstant()
            .SetPostfixTypeConverter( "postfix" );

        var result = sut.ToString();

        result.TestEquals( "Name: 'name', PostfixTypeConverter: 'postfix'" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenOnlyNameAndConstantAreSet()
    {
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "name" )
            .DisablePrefixTypeConverter()
            .SetConstant( "constant" );

        var result = sut.ToString();

        result.TestEquals( "Name: 'name', Constant: 'constant'" ).Go();
    }

    [Fact]
    public void
        SetName_ShouldUpdateNameAndPrefixTypeConverterAndConstant_WhenPrefixTypeConverterAndConstantAreEnabledAndAreNotSetToCustomValues()
    {
        var name = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols().SetName( name );

        Assertion.All(
                sut.Name.ToString().TestEquals( name ),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( $"[{name}]" ),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( name.ToUpperInvariant() ) )
            .Go();
    }

    [Fact]
    public void DisablePrefixTypeConverter_ShouldUpdatePrefixTypeConverterToNull_WhenPrefixTypeConverterIsEnabledAndIsNotSetToCustomValue()
    {
        var sut = new ParsedExpressionTypeDefinitionSymbols().DisablePrefixTypeConverter();

        Assertion.All(
                sut.Name.ToString().TestEmpty(),
                sut.PrefixTypeConverter.TestNull(),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( string.Empty ) )
            .Go();
    }

    [Fact]
    public void DisablePrefixTypeConverter_ShouldUpdatePrefixTypeConverterToNull_WhenPrefixTypeConverterHasCustomValue()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetPrefixTypeConverter( symbol )
            .DisablePrefixTypeConverter();

        Assertion.All(
                sut.Name.ToString().TestEmpty(),
                sut.PrefixTypeConverter.TestNull(),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( string.Empty ) )
            .Go();
    }

    [Fact]
    public void SetPrefixTypeConverter_ShouldUpdatePrefixTypeConverter_WhenPrefixTypeConverterIsEnabled()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols().SetPrefixTypeConverter( symbol );

        Assertion.All(
                sut.Name.ToString().TestEmpty(),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( symbol ),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( string.Empty ) )
            .Go();
    }

    [Fact]
    public void SetPrefixTypeConverter_ShouldUpdatePrefixTypeConverter_WhenPrefixTypeConverterIsDisabled()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .DisablePrefixTypeConverter()
            .SetPrefixTypeConverter( symbol );

        Assertion.All(
                sut.Name.ToString().TestEmpty(),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( symbol ),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( string.Empty ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateNameOnly_WhenPrefixTypeConverterAndConstantAreSetToCustomValues()
    {
        var (name, prefixSymbol, constant) = Fixture.CreateManyDistinct<string>( count: 3 );
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetPrefixTypeConverter( prefixSymbol )
            .SetConstant( constant )
            .SetName( name );

        Assertion.All(
                sut.Name.ToString().TestEquals( name ),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( prefixSymbol ),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( constant ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateNameOnly_WhenPrefixTypeConverterAndConstantAreDisabled()
    {
        var name = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .DisablePrefixTypeConverter()
            .DisableConstant()
            .SetName( name );

        Assertion.All(
                sut.Name.ToString().TestEquals( name ),
                sut.PrefixTypeConverter.TestNull(),
                sut.PostfixTypeConverter.TestNull(),
                sut.Constant.TestNull() )
            .Go();
    }

    [Fact]
    public void SetDefaultPrefixTypeConverter_ShouldUpdatePrefixTypeConverter_WhenPrefixTypeConverterIsSetToCustomValue()
    {
        var (name, prefixSymbol) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( name )
            .SetPrefixTypeConverter( prefixSymbol )
            .SetDefaultPrefixTypeConverter();

        Assertion.All(
                sut.Name.ToString().TestEquals( name ),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( $"[{name}]" ),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( name.ToUpperInvariant() ) )
            .Go();
    }

    [Fact]
    public void SetDefaultPrefixTypeConverter_ShouldUpdatePrefixTypeConverter_WhenPrefixTypeConverterIsDisabled()
    {
        var name = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( name )
            .DisablePrefixTypeConverter()
            .SetDefaultPrefixTypeConverter();

        Assertion.All(
                sut.Name.ToString().TestEquals( name ),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( $"[{name}]" ),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( name.ToUpperInvariant() ) )
            .Go();
    }

    [Fact]
    public void SetPostfixTypeConverter_ShouldUpdatePostfixTypeConverter()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetPostfixTypeConverter( symbol );

        Assertion.All(
                sut.Name.ToString().TestEmpty(),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( "[]" ),
                (sut.PostfixTypeConverter?.ToString()).TestEquals( symbol ),
                (sut.Constant?.ToString()).TestEquals( string.Empty ) )
            .Go();
    }

    [Fact]
    public void DisablePostfixTypeConverter_ShouldUpdatePostfixTypeConverterToNull_WhenPostfixTypeConverterHasValue()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetPostfixTypeConverter( symbol )
            .DisablePostfixTypeConverter();

        Assertion.All(
                sut.Name.ToString().TestEmpty(),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( "[]" ),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( string.Empty ) )
            .Go();
    }

    [Fact]
    public void DisableConstant_ShouldUpdateConstantToNull_WhenConstantIsEnabledAndIsNotSetToCustomValue()
    {
        var sut = new ParsedExpressionTypeDefinitionSymbols().DisableConstant();

        Assertion.All(
                sut.Name.ToString().TestEmpty(),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( "[]" ),
                sut.PostfixTypeConverter.TestNull(),
                sut.Constant.TestNull() )
            .Go();
    }

    [Fact]
    public void DisableConstant_ShouldUpdateConstantToNull_WhenConstantHasCustomValue()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetConstant( symbol )
            .DisableConstant();

        Assertion.All(
                sut.Name.ToString().TestEmpty(),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( "[]" ),
                sut.PostfixTypeConverter.TestNull(),
                sut.Constant.TestNull() )
            .Go();
    }

    [Fact]
    public void SetConstant_ShouldUpdateConstant_WhenConstantIsEnabled()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols().SetConstant( symbol );

        Assertion.All(
                sut.Name.ToString().TestEmpty(),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( "[]" ),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( symbol ) )
            .Go();
    }

    [Fact]
    public void SetConstant_ShouldUpdateConstant_WhenConstantIsDisabled()
    {
        var symbol = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .DisableConstant()
            .SetConstant( symbol );

        Assertion.All(
                sut.Name.ToString().TestEmpty(),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( "[]" ),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( symbol ) )
            .Go();
    }

    [Fact]
    public void SetDefaultConstant_ShouldUpdateConstant_WhenConstantIsSetToCustomValue()
    {
        var (name, constant) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( name )
            .SetConstant( constant )
            .SetDefaultConstant();

        Assertion.All(
                sut.Name.ToString().TestEquals( name ),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( $"[{name}]" ),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( name.ToUpperInvariant() ) )
            .Go();
    }

    [Fact]
    public void SetDefaultConstant_ShouldUpdateConstant_WhenConstantIsDisabled()
    {
        var name = Fixture.Create<string>();
        var sut = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( name )
            .DisableConstant()
            .SetDefaultConstant();

        Assertion.All(
                sut.Name.ToString().TestEquals( name ),
                (sut.PrefixTypeConverter?.ToString()).TestEquals( $"[{name}]" ),
                sut.PostfixTypeConverter.TestNull(),
                (sut.Constant?.ToString()).TestEquals( name.ToUpperInvariant() ) )
            .Go();
    }
}
