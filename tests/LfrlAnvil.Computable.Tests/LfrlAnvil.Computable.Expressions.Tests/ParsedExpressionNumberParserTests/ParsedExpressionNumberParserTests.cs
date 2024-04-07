using System.Collections.Generic;
using System.Numerics;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionNumberParserTests;

public class ParsedExpressionNumberParserTests : TestsBase
{
    [Theory]
    [InlineData( "0", true, true, 0.0 )]
    [InlineData( "1_234_567", true, true, 1234567.0 )]
    [InlineData( "1234x5", true, true, 1234.5 )]
    [InlineData( "1_234x5", true, true, 1234.5 )]
    [InlineData( "1_2_3_4x5", true, true, 1234.5 )]
    [InlineData( "1_234x5e2", true, true, 123450.0 )]
    [InlineData( "50_0E-3", true, true, 0.5 )]
    [InlineData( "foo", true, true, null )]
    [InlineData( "1234x5", false, true, null )]
    [InlineData( "50_0E-3", true, false, null )]
    public void DefaultDecimal_ShouldReturnTrueAndCorrectDecimal_WhenParsingIsPossible(
        string input,
        bool allowNonIntegerNumbers,
        bool allowScientificNotation,
        double? expected)
    {
        var parser = ParsedExpressionNumberParser.CreateDefaultDecimal(
            GetConfiguration( allowNonIntegerNumbers, allowScientificNotation ) );

        var result = parser.TryParse( input, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected is not null );
            outResult.Should().Be( ( decimal? )expected );
        }
    }

    [Theory]
    [InlineData( "0", true, true, 0.0 )]
    [InlineData( "1_234_567", true, true, 1234567.0 )]
    [InlineData( "1234x5", true, true, 1234.5 )]
    [InlineData( "1_234x5", true, true, 1234.5 )]
    [InlineData( "1_2_3_4x5", true, true, 1234.5 )]
    [InlineData( "1_234x5e2", true, true, 123450.0 )]
    [InlineData( "50_0E-3", true, true, 0.5 )]
    [InlineData( "foo", true, true, null )]
    [InlineData( "1234x5", false, true, null )]
    [InlineData( "50_0E-3", true, false, null )]
    public void DefaultDouble_ShouldReturnTrueAndCorrectDouble_WhenParsingIsPossible(
        string input,
        bool allowNonIntegerNumbers,
        bool allowScientificNotation,
        double? expected)
    {
        var parser = ParsedExpressionNumberParser.CreateDefaultDouble(
            GetConfiguration( allowNonIntegerNumbers, allowScientificNotation ) );

        var result = parser.TryParse( input, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected is not null );
            outResult.Should().Be( expected );
        }
    }

    [Theory]
    [InlineData( "0", true, true, 0f )]
    [InlineData( "1_234_567", true, true, 1234567f )]
    [InlineData( "1234x5", true, true, 1234.5f )]
    [InlineData( "1_234x5", true, true, 1234.5f )]
    [InlineData( "1_2_3_4x5", true, true, 1234.5f )]
    [InlineData( "1_234x5e2", true, true, 123450f )]
    [InlineData( "50_0E-3", true, true, 0.5f )]
    [InlineData( "foo", true, true, null )]
    [InlineData( "1234x5", false, true, null )]
    [InlineData( "50_0E-3", true, false, null )]
    public void DefaultFloat_ShouldReturnTrueAndCorrectFloat_WhenParsingIsPossible(
        string input,
        bool allowNonIntegerNumbers,
        bool allowScientificNotation,
        float? expected)
    {
        var parser = ParsedExpressionNumberParser.CreateDefaultFloat( GetConfiguration( allowNonIntegerNumbers, allowScientificNotation ) );

        var result = parser.TryParse( input, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected is not null );
            outResult.Should().Be( expected );
        }
    }

    [Theory]
    [InlineData( "0", true, true, 0 )]
    [InlineData( "1_234_567", true, true, 1234567 )]
    [InlineData( "1_234e2", true, true, 123400 )]
    [InlineData( "50_0E-2", true, true, 5 )]
    [InlineData( "foo", true, true, null )]
    [InlineData( "1234x5", true, true, null )]
    [InlineData( "1234x5", false, true, null )]
    [InlineData( "50_0E-3", true, false, null )]
    public void DefaultInt32_ShouldReturnTrueAndCorrectInt_WhenParsingIsPossible(
        string input,
        bool allowNonIntegerNumbers,
        bool allowScientificNotation,
        int? expected)
    {
        var parser = ParsedExpressionNumberParser.CreateDefaultInt32( GetConfiguration( allowNonIntegerNumbers, allowScientificNotation ) );

        var result = parser.TryParse( input, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected is not null );
            outResult.Should().Be( expected );
        }
    }

    [Theory]
    [InlineData( "0", true, true, 0L )]
    [InlineData( "1_234_567", true, true, 1234567L )]
    [InlineData( "1_234e2", true, true, 123400L )]
    [InlineData( "50_0E-2", true, true, 5L )]
    [InlineData( "foo", true, true, null )]
    [InlineData( "1234x5", true, true, null )]
    [InlineData( "1234x5", false, true, null )]
    [InlineData( "50_0E-3", true, false, null )]
    public void DefaultInt64_ShouldReturnTrueAndCorrectLong_WhenParsingIsPossible(
        string input,
        bool allowNonIntegerNumbers,
        bool allowScientificNotation,
        long? expected)
    {
        var parser = ParsedExpressionNumberParser.CreateDefaultInt64( GetConfiguration( allowNonIntegerNumbers, allowScientificNotation ) );

        var result = parser.TryParse( input, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected is not null );
            outResult.Should().Be( expected );
        }
    }

    [Theory]
    [InlineData( "0", true, true, 0L )]
    [InlineData( "1_234_567", true, true, 1234567L )]
    [InlineData( "1_234e2", true, true, 123400L )]
    [InlineData( "50_0E-2", true, true, 5L )]
    [InlineData( "foo", true, true, null )]
    [InlineData( "1234x5", true, true, null )]
    [InlineData( "1234x5", false, true, null )]
    [InlineData( "50_0E-3", true, false, null )]
    public void DefaultBigInteger_ShouldReturnTrueAndCorrectBigInt_WhenParsingIsPossible(
        string input,
        bool allowNonIntegerNumbers,
        bool allowScientificNotation,
        long? expected)
    {
        var parser = ParsedExpressionNumberParser.CreateDefaultBigInteger(
            GetConfiguration( allowNonIntegerNumbers, allowScientificNotation ) );

        var result = parser.TryParse( input, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected is not null );
            outResult.Should().Be( ( BigInteger? )expected );
        }
    }

    private static ParsedExpressionFactoryInternalConfiguration GetConfiguration(bool allowNonIntegerNumbers, bool allowScientificNotation)
    {
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( 'x' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowNonIntegerNumbers.Returns( allowNonIntegerNumbers );
        configuration.AllowScientificNotation.Returns( allowScientificNotation );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( true );

        return new ParsedExpressionFactoryInternalConfiguration(
            new Dictionary<StringSegment, ConstructTokenDefinition>(),
            configuration );
    }
}
