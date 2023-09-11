using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.StringBuilderTests;

public class StringBuilderExtensionsTests : TestsBase
{
    [Fact]
    public void Reverse_ShouldReverseFullString_WhenNoExplicitParametersAreProvided()
    {
        var sut = new StringBuilder( "1234567" );

        var result = sut.Reverse();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.ToString().Should().Be( "7654321" );
        }
    }

    [Theory]
    [InlineData( -3, -1, "1234567" )]
    [InlineData( -3, 3, "1234567" )]
    [InlineData( -3, 4, "1234567" )]
    [InlineData( -3, 5, "2134567" )]
    [InlineData( -3, 8, "5432167" )]
    [InlineData( -3, 11, "7654321" )]
    [InlineData( 0, -1, "1234567" )]
    [InlineData( 0, 2, "2134567" )]
    [InlineData( 0, 5, "5432167" )]
    [InlineData( 0, 8, "7654321" )]
    [InlineData( 2, -1, "1234567" )]
    [InlineData( 2, 4, "1265437" )]
    [InlineData( 2, 6, "1276543" )]
    [InlineData( 5, -1, "1234567" )]
    [InlineData( 5, 2, "1234576" )]
    [InlineData( 5, 3, "1234576" )]
    [InlineData( 7, -1, "1234567" )]
    [InlineData( 7, 1, "1234567" )]
    public void Reverse_ShouldReverseCorrectPartOfString(int startIndex, int length, string expected)
    {
        var sut = new StringBuilder( "1234567" );

        var result = sut.Reverse( startIndex, length );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.ToString().Should().Be( expected );
        }
    }

    [Fact]
    public void Indent_ShouldAppendLine_WhenValueEqualsZero()
    {
        var sut = new StringBuilder();

        var result = sut.Indent( 0 );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.ToString().Should().Be( Environment.NewLine );
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void Indent_ShouldAppendLineFollowedBySpaces_WhenValueIsGreaterThanZero(int value)
    {
        var sut = new StringBuilder();

        var result = sut.Indent( value );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.ToString().Should().Be( Environment.NewLine + new string( ' ', value ) );
        }
    }

    [Fact]
    public void AppendLine_WithChar_ShouldAppendCharFollowedByNewLine()
    {
        var sut = new StringBuilder();

        var result = sut.AppendLine( '.' );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.ToString().Should().Be( $".{Environment.NewLine}" );
        }
    }

    [Fact]
    public void AppendSpace_ShouldAppendSingleSpace()
    {
        var sut = new StringBuilder();

        var result = sut.AppendSpace();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.ToString().Should().Be( " " );
        }
    }

    [Fact]
    public void AppendDot_ShouldAppendSingleDot()
    {
        var sut = new StringBuilder();

        var result = sut.AppendDot();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.ToString().Should().Be( "." );
        }
    }

    [Fact]
    public void AppendComma_ShouldAppendSingleComma()
    {
        var sut = new StringBuilder();

        var result = sut.AppendComma();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.ToString().Should().Be( "," );
        }
    }

    [Fact]
    public void AppendSemicolon_ShouldAppendSingleSemicolon()
    {
        var sut = new StringBuilder();

        var result = sut.AppendSemicolon();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.ToString().Should().Be( ";" );
        }
    }

    [Fact]
    public void ShrinkBy_ShouldReduceLength()
    {
        var sut = new StringBuilder( "foobar" );

        var result = sut.ShrinkBy( 3 );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.ToString().Should().Be( "foo" );
        }
    }
}
