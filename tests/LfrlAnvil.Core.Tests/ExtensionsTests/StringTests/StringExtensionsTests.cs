using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.ExtensionsTests.StringTests;

public class StringExtensionsTests : TestsBase
{
    [Theory]
    [InlineData( "" )]
    [InlineData( "foo" )]
    [InlineData( "foobar" )]
    public void AsSlice_WithSource_ShouldReturnCorrectResult(string source)
    {
        var sut = source.AsSlice();

        using ( new AssertionScope() )
        {
            sut.StartIndex.Should().Be( 0 );
            sut.Length.Should().Be( source.Length );
            sut.EndIndex.Should().Be( source.Length );
            sut.Source.Should().BeSameAs( source );
        }
    }

    [Theory]
    [InlineData( "", 0, 0, 0, 0 )]
    [InlineData( "", 1, 0, 0, 0 )]
    [InlineData( "foobar", 0, 0, 6, 6 )]
    [InlineData( "foobar", 1, 1, 6, 5 )]
    [InlineData( "foobar", 3, 3, 6, 3 )]
    [InlineData( "foobar", 6, 6, 6, 0 )]
    [InlineData( "foobar", 7, 6, 6, 0 )]
    public void AsSlice_WithSourceAndStartIndex_ShouldReturnCorrectResult(
        string source,
        int startIndex,
        int expectedStartIndex,
        int expectedEndIndex,
        int expectedLength)
    {
        var sut = source.AsSlice( startIndex );

        using ( new AssertionScope() )
        {
            sut.StartIndex.Should().Be( expectedStartIndex );
            sut.Length.Should().Be( expectedLength );
            sut.EndIndex.Should().Be( expectedEndIndex );
            sut.Source.Should().BeSameAs( source );
        }
    }

    [Fact]
    public void AsSlice_WithSourceAndStartIndex_ShouldThrowArgumentOutOfRangeException_WhenStartIndexIsLessThanZero()
    {
        var action = Lambda.Of( () => Fixture.Create<string>().AsSlice( startIndex: -1 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( "", 0, 0, 0, 0, 0 )]
    [InlineData( "", 1, 0, 0, 0, 0 )]
    [InlineData( "", 1, 1, 0, 0, 0 )]
    [InlineData( "foobar", 0, 0, 0, 0, 0 )]
    [InlineData( "foobar", 0, 1, 0, 1, 1 )]
    [InlineData( "foobar", 0, 3, 0, 3, 3 )]
    [InlineData( "foobar", 0, 6, 0, 6, 6 )]
    [InlineData( "foobar", 0, 7, 0, 6, 6 )]
    [InlineData( "foobar", 1, 0, 1, 1, 0 )]
    [InlineData( "foobar", 1, 1, 1, 2, 1 )]
    [InlineData( "foobar", 1, 3, 1, 4, 3 )]
    [InlineData( "foobar", 1, 5, 1, 6, 5 )]
    [InlineData( "foobar", 1, 6, 1, 6, 5 )]
    [InlineData( "foobar", 3, 0, 3, 3, 0 )]
    [InlineData( "foobar", 3, 1, 3, 4, 1 )]
    [InlineData( "foobar", 3, 2, 3, 5, 2 )]
    [InlineData( "foobar", 3, 3, 3, 6, 3 )]
    [InlineData( "foobar", 3, 4, 3, 6, 3 )]
    [InlineData( "foobar", 6, 0, 6, 6, 0 )]
    [InlineData( "foobar", 6, 1, 6, 6, 0 )]
    [InlineData( "foobar", 7, 0, 6, 6, 0 )]
    [InlineData( "foobar", 7, 1, 6, 6, 0 )]
    public void AsSlice_WithSourceAndStartIndexAndLength_ShouldReturnCorrectResult(
        string source,
        int startIndex,
        int length,
        int expectedStartIndex,
        int expectedEndIndex,
        int expectedLength)
    {
        var sut = source.AsSlice( startIndex, length );

        using ( new AssertionScope() )
        {
            sut.StartIndex.Should().Be( expectedStartIndex );
            sut.Length.Should().Be( expectedLength );
            sut.EndIndex.Should().Be( expectedEndIndex );
            sut.Source.Should().BeSameAs( source );
        }
    }

    [Fact]
    public void AsSlice_WithSourceAndStartIndexAndLength_ShouldThrowArgumentOutOfRangeException_WhenStartIndexIsLessThanZero()
    {
        var action = Lambda.Of( () => Fixture.Create<string>().AsSlice( startIndex: -1, length: 0 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AsSlice_WithSourceAndStartIndexAndLength_ShouldThrowArgumentOutOfRangeException_WhenLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => Fixture.Create<string>().AsSlice( startIndex: 0, length: -1 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }
}
