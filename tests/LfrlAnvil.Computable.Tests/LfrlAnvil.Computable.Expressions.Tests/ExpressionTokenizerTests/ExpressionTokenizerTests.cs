﻿using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Computable.Expressions.Internal;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Computable.Expressions.Tests.ExpressionTokenizerTests;

[TestClass( typeof( ExpressionTokenizerTestsData ) )]
public class ExpressionTokenizerTests : TestsBase
{
    [Fact]
    public void ReadNextToken_ShouldReturnEmptyCollection_WhenInputIsEmpty()
    {
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( string.Empty, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeEmpty();
    }

    [Theory]
    [MethodData( nameof( ExpressionTokenizerTestsData.GetWhiteSpaceInputData ) )]
    public void ReadNextToken_ShouldReturnEmptyCollection_WhenInputConsistsOfOnlyWhiteSpaces(string input)
    {
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeEmpty();
    }

    [Fact]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleOpenParenthesis()
    {
        var input = "(";
        var expected = IntermediateToken.CreateOpenedParenthesis( StringSlice.Create( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleCloseParenthesis()
    {
        var input = ")";
        var expected = IntermediateToken.CreateClosedParenthesis( StringSlice.Create( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleElementSeparator()
    {
        var input = ",";
        var expected = IntermediateToken.CreateElementSeparator( StringSlice.Create( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleInlineFunctionSeparator()
    {
        var input = ";";
        var expected = IntermediateToken.CreateInlineFunctionSeparator( StringSlice.Create( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleMemberAccess()
    {
        var input = ".";
        var expected = IntermediateToken.CreateMemberAccess( StringSlice.Create( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Theory]
    [MethodData( nameof( ExpressionTokenizerTestsData.GetStringOnlyData ) )]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleStringValue(string input)
    {
        var expected = IntermediateToken.CreateStringConstant( StringSlice.Create( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Theory]
    [MethodData( nameof( ExpressionTokenizerTestsData.GetStartsWithStringData ) )]
    public void ReadNextToken_ShouldReturnCorrectFirstToken_WhenInputStartsWithStringValue(string input, string expected)
    {
        var expectedToken = IntermediateToken.CreateStringConstant( StringSlice.Create( expected ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        sut.ReadNextToken( out var token );

        token.Should().BeEquivalentTo( expectedToken );
    }

    [Theory]
    [MethodData( nameof( ExpressionTokenizerTestsData.GetNumberOnlyData ) )]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleNumberValue(string input)
    {
        var expected = IntermediateToken.CreateNumberConstant( StringSlice.Create( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Theory]
    [MethodData( nameof( ExpressionTokenizerTestsData.GetStartsWithNumberData ) )]
    public void ReadNextToken_ShouldStopAndReturnCorrectFirstToken_WhenInputStartsWithNumberValue(string input, string expected)
    {
        var expectedToken = IntermediateToken.CreateNumberConstant( StringSlice.Create( expected ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        sut.ReadNextToken( out var token );

        token.Should().BeEquivalentTo( expectedToken );
    }

    [Fact]
    public void ReadNextToken_ShouldReturnCorrectFirstToken_WhenInputStartsWithNumberValueAndConfigurationDoesNotAllowDecimalPoints()
    {
        var input = "1234.567";
        var expected = IntermediateToken.CreateNumberConstant( StringSlice.Create( "1234" ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration(
            GetConstructs(),
            GetDefaultConfiguration( allowNonIntegerNumbers: false ) );

        var sut = new ExpressionTokenizer( input, configuration );

        sut.ReadNextToken( out var token );

        token.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void ReadNextToken_ShouldReturnCorrectFirstToken_WhenInputStartsWithNumberValueAndConfigurationDoesNotAllowScientificNotation()
    {
        var input = "1234.567E890";
        var expected = IntermediateToken.CreateNumberConstant( StringSlice.Create( "1234.567" ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration(
            GetConstructs(),
            GetDefaultConfiguration( allowScientificNotation: false ) );

        var sut = new ExpressionTokenizer( input, configuration );

        sut.ReadNextToken( out var token );

        token.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ExpressionTokenizerTestsData.GetBooleanOnlyData ) )]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleBooleanValue(string input)
    {
        var expected = IntermediateToken.CreateBooleanConstant( StringSlice.Create( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Theory]
    [MethodData( nameof( ExpressionTokenizerTestsData.GetStartsWithBooleanData ) )]
    public void ReadNextToken_ShouldStopAndReturnCorrectFirstToken_WhenInputStartsWithBooleanValue(string input, string expected)
    {
        var expectedToken = IntermediateToken.CreateBooleanConstant( StringSlice.Create( expected ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        sut.ReadNextToken( out var token );

        token.Should().BeEquivalentTo( expectedToken );
    }

    [Theory]
    [MethodData( nameof( ExpressionTokenizerTestsData.GetArgumentOnlyData ) )]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleArgument(string input)
    {
        var expected = IntermediateToken.CreateArgument( StringSlice.Create( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Theory]
    [MethodData( nameof( ExpressionTokenizerTestsData.GetStartsWithArgumentData ) )]
    public void ReadNextToken_ShouldStopAndReturnCorrectFirstToken_WhenInputStartsWithArgument(string input, string expected)
    {
        var expectedToken = IntermediateToken.CreateArgument( StringSlice.Create( expected ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        sut.ReadNextToken( out var token );

        token.Should().BeEquivalentTo( expectedToken );
    }

    [Theory]
    [MethodData( nameof( ExpressionTokenizerTestsData.GetComplexInputData ) )]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenParsingComplexInput(
        string input,
        IEnumerable<string> tokenSymbols,
        IEnumerable<ExpressionTokenizerTestsData.Token> expected)
    {
        var tokenSets = GetConstructs( tokenSymbols.ToArray() );
        var expectedTokens = expected.Select( t => t.GetToken( tokenSets ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( tokenSets, GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expectedTokens );
    }

    private static IParsedExpressionFactoryConfiguration GetDefaultConfiguration(
        bool allowScientificNotation = true,
        bool allowNonIntegerNumbers = true)
    {
        var result = Substitute.For<IParsedExpressionFactoryConfiguration>();
        result.DecimalPoint.Returns( '.' );
        result.IntegerDigitSeparator.Returns( '_' );
        result.ScientificNotationExponents.Returns( "eE" );
        result.AllowScientificNotation.Returns( allowScientificNotation );
        result.AllowNonIntegerNumbers.Returns( allowNonIntegerNumbers );
        result.StringDelimiter.Returns( '\'' );
        result.ConvertResultToOutputTypeAutomatically.Returns( true );
        return result;
    }

    private static IReadOnlyDictionary<StringSlice, ConstructTokenDefinition> GetConstructs(params string[] symbols)
    {
        var result = new Dictionary<StringSlice, ConstructTokenDefinition>();
        foreach ( var s in symbols )
        {
            var set = ConstructTokenDefinition.CreateOperator(
                BinaryOperatorCollection.Empty,
                UnaryOperatorCollection.Empty,
                UnaryOperatorCollection.Empty );

            result.Add( StringSlice.Create( s ), set );
        }

        return result;
    }
}