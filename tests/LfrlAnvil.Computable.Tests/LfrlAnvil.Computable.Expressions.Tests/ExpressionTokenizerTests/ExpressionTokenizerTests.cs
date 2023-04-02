using System.Collections.Generic;
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
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleOpenedParenthesis()
    {
        var input = "(";
        var expected = IntermediateToken.CreateOpenedParenthesis( new StringSegment( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleClosedParenthesis()
    {
        var input = ")";
        var expected = IntermediateToken.CreateClosedParenthesis( new StringSegment( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleOpenedSquareBracket()
    {
        var input = "[";
        var expected = IntermediateToken.CreateOpenedSquareBracket( new StringSegment( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleClosedSquareBracket()
    {
        var input = "]";
        var expected = IntermediateToken.CreateClosedSquareBracket( new StringSegment( input ) );
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
        var expected = IntermediateToken.CreateElementSeparator( new StringSegment( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleVariableSeparator()
    {
        var input = ";";
        var expected = IntermediateToken.CreateLineSeparator( new StringSegment( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Theory]
    [InlineData( "let" )]
    [InlineData( "LET" )]
    [InlineData( "Let" )]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleVariableDeclaration(string input)
    {
        var expected = IntermediateToken.CreateVariableDeclaration( new StringSegment( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Theory]
    [InlineData( "macro" )]
    [InlineData( "MACRO" )]
    [InlineData( "Macro" )]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleMacroDeclaration(string input)
    {
        var expected = IntermediateToken.CreateMacroDeclaration( new StringSegment( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleAssignment()
    {
        var input = "=";
        var expected = IntermediateToken.CreateAssignment( new StringSegment( input ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        var result = new List<IntermediateToken>();
        while ( sut.ReadNextToken( out var token ) )
            result.Add( token );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleAssignmentWithConstructs()
    {
        var input = "=";
        var constructs = GetConstructs( "=" );
        var expected = IntermediateToken.CreateAssignmentWithConstructs( new StringSegment( input ), constructs.First().Value );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( constructs, GetDefaultConfiguration() );
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
        var expected = IntermediateToken.CreateMemberAccess( new StringSegment( input ) );
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
        var expected = IntermediateToken.CreateStringConstant( new StringSegment( input ) );
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
        var expectedToken = IntermediateToken.CreateStringConstant( new StringSegment( expected ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        sut.ReadNextToken( out var token );

        token.Should().BeEquivalentTo( expectedToken );
    }

    [Theory]
    [MethodData( nameof( ExpressionTokenizerTestsData.GetNumberOnlyData ) )]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleNumberValue(string input)
    {
        var expected = IntermediateToken.CreateNumberConstant( new StringSegment( input ) );
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
        var expectedToken = IntermediateToken.CreateNumberConstant( new StringSegment( expected ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        sut.ReadNextToken( out var token );

        token.Should().BeEquivalentTo( expectedToken );
    }

    [Fact]
    public void ReadNextToken_ShouldReturnCorrectFirstToken_WhenInputStartsWithNumberValueAndConfigurationDoesNotAllowDecimalPoints()
    {
        var input = "1234.567";
        var expected = IntermediateToken.CreateNumberConstant( new StringSegment( "1234" ) );
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
        var expected = IntermediateToken.CreateNumberConstant( new StringSegment( "1234.567" ) );
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
        var expected = IntermediateToken.CreateBooleanConstant( new StringSegment( input ) );
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
        var expectedToken = IntermediateToken.CreateBooleanConstant( new StringSegment( expected ) );
        var configuration = new ParsedExpressionFactoryInternalConfiguration( GetConstructs(), GetDefaultConfiguration() );
        var sut = new ExpressionTokenizer( input, configuration );

        sut.ReadNextToken( out var token );

        token.Should().BeEquivalentTo( expectedToken );
    }

    [Theory]
    [MethodData( nameof( ExpressionTokenizerTestsData.GetArgumentOnlyData ) )]
    public void ReadNextToken_ShouldReturnCorrectResult_WhenInputConsistsOfSingleArgument(string input)
    {
        var expected = IntermediateToken.CreateArgument( new StringSegment( input ) );
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
        var expectedToken = IntermediateToken.CreateArgument( new StringSegment( expected ) );
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

    private static IReadOnlyDictionary<StringSegment, ConstructTokenDefinition> GetConstructs(params string[] symbols)
    {
        var result = new Dictionary<StringSegment, ConstructTokenDefinition>();
        foreach ( var s in symbols )
        {
            var set = ConstructTokenDefinition.CreateOperator(
                BinaryOperatorCollection.Empty,
                UnaryOperatorCollection.Empty,
                UnaryOperatorCollection.Empty );

            result.Add( new StringSegment( s ), set );
        }

        return result;
    }
}
