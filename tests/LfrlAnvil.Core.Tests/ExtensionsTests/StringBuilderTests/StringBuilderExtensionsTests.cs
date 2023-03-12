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
}
