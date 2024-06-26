﻿using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Tests.TokenValidationTests;

public class TokenValidationTests : TestsBase
{
    [Theory]
    [InlineData( "_" )]
    [InlineData( "_0" )]
    [InlineData( "_a" )]
    [InlineData( "_a0" )]
    [InlineData( "_0a" )]
    [InlineData( "foo" )]
    [InlineData( "foo_0123456789_" )]
    public void IsValidLocalTermName_ShouldReturnTrue_WhenTextIsValid(string text)
    {
        var result = TokenValidation.IsValidLocalTermName( new StringSegment( text ), stringDelimiter: '\'' );
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "0" )]
    [InlineData( "0123456789" )]
    [InlineData( "+" )]
    [InlineData( "?" )]
    [InlineData( "foo+" )]
    [InlineData( "_?" )]
    [InlineData( "'" )]
    [InlineData( "foo'" )]
    [InlineData( "foo " )]
    public void IsValidLocalTermName_ShouldReturnFalse_WhenTextIsInvalid(string text)
    {
        var result = TokenValidation.IsValidLocalTermName( new StringSegment( text ), stringDelimiter: '\'' );
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( "_0" )]
    [InlineData( "_a" )]
    [InlineData( "_a0" )]
    [InlineData( "_0a" )]
    [InlineData( "_+" )]
    [InlineData( "foo" )]
    [InlineData( "foo_0123456789_" )]
    [InlineData( "+" )]
    [InlineData( "foo+" )]
    [InlineData( "?0foo/" )]
    [InlineData( "a[" )]
    [InlineData( "a]" )]
    [InlineData( "a[]" )]
    [InlineData( "][" )]
    public void IsValidConstructSymbol_ShouldReturnTrue_WhenTextIsValid(string text)
    {
        var result = TokenValidation.IsValidConstructSymbol( new StringSegment( text ), stringDelimiter: '\'' );
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( "_" )]
    [InlineData( "(" )]
    [InlineData( ")" )]
    [InlineData( "[" )]
    [InlineData( "]" )]
    [InlineData( "[]" )]
    [InlineData( "." )]
    [InlineData( "," )]
    [InlineData( ";" )]
    [InlineData( " " )]
    [InlineData( "0" )]
    [InlineData( "0123456789" )]
    [InlineData( "foo(" )]
    [InlineData( "foo)" )]
    [InlineData( "foo." )]
    [InlineData( "foo," )]
    [InlineData( "foo;" )]
    [InlineData( "'" )]
    [InlineData( "foo'" )]
    [InlineData( "foo " )]
    [InlineData( "false" )]
    [InlineData( "FALSE" )]
    [InlineData( "False" )]
    [InlineData( "true" )]
    [InlineData( "TRUE" )]
    [InlineData( "True" )]
    [InlineData( "let" )]
    [InlineData( "LET" )]
    [InlineData( "Let" )]
    public void IsValidConstructSymbol_ShouldReturnFalse_WhenTextIsInvalid(string text)
    {
        var result = TokenValidation.IsValidConstructSymbol( new StringSegment( text ), stringDelimiter: '\'' );
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 'a' )]
    [InlineData( 'Z' )]
    [InlineData( '*' )]
    [InlineData( '.' )]
    [InlineData( ',' )]
    [InlineData( '\'' )]
    [InlineData( '"' )]
    [InlineData( '_' )]
    public void IsNumberSymbolValid_ShouldReturnTrue_WhenSymbolIsValid(char symbol)
    {
        var result = TokenValidation.IsNumberSymbolValid( symbol );
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData( '+' )]
    [InlineData( '-' )]
    [InlineData( '(' )]
    [InlineData( ')' )]
    [InlineData( '[' )]
    [InlineData( ']' )]
    [InlineData( ';' )]
    [InlineData( '0' )]
    [InlineData( ' ' )]
    [InlineData( '=' )]
    public void IsNumberSymbolValid_ShouldReturnFalse_WhenSymbolIsInvalid(char symbol)
    {
        var result = TokenValidation.IsNumberSymbolValid( symbol );
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 'a' )]
    [InlineData( 'Z' )]
    [InlineData( '*' )]
    [InlineData( '\'' )]
    [InlineData( '"' )]
    public void IsStringDelimiterSymbolValid_ShouldReturnTrue_WhenSymbolIsValid(char symbol)
    {
        var result = TokenValidation.IsStringDelimiterSymbolValid( symbol );
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData( '_' )]
    [InlineData( '+' )]
    [InlineData( '-' )]
    [InlineData( '(' )]
    [InlineData( ')' )]
    [InlineData( '[' )]
    [InlineData( ']' )]
    [InlineData( ';' )]
    [InlineData( '.' )]
    [InlineData( ',' )]
    [InlineData( '0' )]
    [InlineData( ' ' )]
    [InlineData( '=' )]
    public void IsStringDelimiterSymbolValid_ShouldReturnFalse_WhenSymbolIsInvalid(char symbol)
    {
        var result = TokenValidation.IsStringDelimiterSymbolValid( symbol );
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 'e' )]
    [InlineData( 'E' )]
    [InlineData( 'a' )]
    [InlineData( 'Z' )]
    public void IsExponentSymbolValid_ShouldReturnTrue_WhenSymbolIsValid(char symbol)
    {
        var result = TokenValidation.IsExponentSymbolValid( symbol );
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData( '_' )]
    [InlineData( '+' )]
    [InlineData( '-' )]
    [InlineData( '(' )]
    [InlineData( ')' )]
    [InlineData( '[' )]
    [InlineData( ']' )]
    [InlineData( ';' )]
    [InlineData( '.' )]
    [InlineData( ',' )]
    [InlineData( '*' )]
    [InlineData( '\'' )]
    [InlineData( '"' )]
    [InlineData( '0' )]
    [InlineData( ' ' )]
    [InlineData( '=' )]
    public void IsExponentSymbolValid_ShouldReturnFalse_WhenSymbolIsInvalid(char symbol)
    {
        var result = TokenValidation.IsExponentSymbolValid( symbol );
        result.Should().BeFalse();
    }
}
