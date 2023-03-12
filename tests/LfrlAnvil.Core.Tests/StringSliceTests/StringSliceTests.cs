using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.StringSliceTests;

public class StringSliceTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnEmptySlice()
    {
        var sut = default( StringSlice );

        using ( new AssertionScope() )
        {
            sut.StartIndex.Should().Be( 0 );
            sut.Length.Should().Be( 0 );
            sut.EndIndex.Should().Be( 0 );
            sut.Source.Should().BeSameAs( string.Empty );
        }
    }

    [Fact]
    public void Empty_ShouldReturnEmptySlice()
    {
        var sut = StringSlice.Empty;

        using ( new AssertionScope() )
        {
            sut.StartIndex.Should().Be( 0 );
            sut.Length.Should().Be( 0 );
            sut.EndIndex.Should().Be( 0 );
            sut.Source.Should().BeSameAs( string.Empty );
        }
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( "foo" )]
    [InlineData( "foobar" )]
    public void Ctor_WithSource_ShouldReturnCorrectResult(string source)
    {
        var sut = new StringSlice( source );

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
    public void Ctor_WithSourceAndStartIndex_ShouldReturnCorrectResult(
        string source,
        int startIndex,
        int expectedStartIndex,
        int expectedEndIndex,
        int expectedLength)
    {
        var sut = new StringSlice( source, startIndex );

        using ( new AssertionScope() )
        {
            sut.StartIndex.Should().Be( expectedStartIndex );
            sut.Length.Should().Be( expectedLength );
            sut.EndIndex.Should().Be( expectedEndIndex );
            sut.Source.Should().BeSameAs( source );
        }
    }

    [Fact]
    public void Ctor_WithSourceAndStartIndex_ShouldThrowArgumentOutOfRangeException_WhenStartIndexIsLessThanZero()
    {
        var action = Lambda.Of( () => new StringSlice( Fixture.Create<string>(), startIndex: -1 ) );
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
    public void Ctor_WithSourceAndStartIndexAndLength_ShouldReturnCorrectResult(
        string source,
        int startIndex,
        int length,
        int expectedStartIndex,
        int expectedEndIndex,
        int expectedLength)
    {
        var sut = new StringSlice( source, startIndex, length );

        using ( new AssertionScope() )
        {
            sut.StartIndex.Should().Be( expectedStartIndex );
            sut.Length.Should().Be( expectedLength );
            sut.EndIndex.Should().Be( expectedEndIndex );
            sut.Source.Should().BeSameAs( source );
        }
    }

    [Fact]
    public void Ctor_WithSourceAndStartIndexAndLength_ShouldThrowArgumentOutOfRangeException_WhenStartIndexIsLessThanZero()
    {
        var action = Lambda.Of( () => new StringSlice( Fixture.Create<string>(), startIndex: -1, length: 0 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Ctor_WithSourceAndStartIndexAndLength_ShouldThrowArgumentOutOfRangeException_WhenLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => new StringSlice( Fixture.Create<string>(), startIndex: 0, length: -1 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( "foobar", 0, 6 )]
    [InlineData( "foobar", 1, 5 )]
    [InlineData( "foobar", 2, 2 )]
    public void FromMemory_ShouldReturnCorrectResult_WhenUnderlyingObjectIsString(string source, int startIndex, int length)
    {
        var memory = source.AsMemory( startIndex, length );
        var sut = StringSlice.FromMemory( memory );

        using ( new AssertionScope() )
        {
            sut.StartIndex.Should().Be( startIndex );
            sut.Length.Should().Be( length );
            sut.EndIndex.Should().Be( startIndex + length );
            sut.Source.Should().BeSameAs( source );
        }
    }

    [Theory]
    [InlineData( 0, 6, "foobar" )]
    [InlineData( 1, 5, "oobar" )]
    [InlineData( 2, 2, "ob" )]
    public void FromMemory_ShouldReturnCorrectResultWithNewString_WhenUnderlyingObjectIsNotString(
        int startIndex,
        int length,
        string expectedSource)
    {
        var source = new[] { 'f', 'o', 'o', 'b', 'a', 'r' };
        var memory = source.AsMemory( startIndex, length );
        var sut = StringSlice.FromMemory( memory );

        using ( new AssertionScope() )
        {
            sut.StartIndex.Should().Be( 0 );
            sut.Length.Should().Be( length );
            sut.EndIndex.Should().Be( length );
            sut.Source.Should().Be( expectedSource );
        }
    }

    [Theory]
    [InlineData( "foobar", 0, 6, "foobar" )]
    [InlineData( "foobar", 1, 5, "oobar" )]
    [InlineData( "foobar", 0, 5, "fooba" )]
    [InlineData( "foobar", 2, 2, "ob" )]
    public void ToString_ShouldReturnCorrectResult(string source, int startIndex, int length, string expected)
    {
        var sut = new StringSlice( source, startIndex, length );
        var result = sut.ToString();
        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = new StringSlice( "foobar", 2, 2 );
        var expected = string.GetHashCode( sut.ToString().AsSpan() );
        var result = sut.GetHashCode();
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foobar", 0, 6, "foobar", 0, 6, true )]
    [InlineData( "foobar", 0, 6, "foobar", 0, 5, false )]
    [InlineData( "foobar", 0, 5, "foobar", 0, 6, false )]
    [InlineData( "foobar", 2, 2, "foobar", 2, 2, true )]
    [InlineData( "foobar", 0, 3, "foo", 0, 3, true )]
    [InlineData( "foobar", 3, 3, "bar", 0, 3, true )]
    public void Equals_ShouldReturnCorrectResult(
        string source,
        int startIndex,
        int length,
        string otherSource,
        int otherStartIndex,
        int otherLength,
        bool expected)
    {
        var sut = new StringSlice( source, startIndex, length );
        var other = new StringSlice( otherSource, otherStartIndex, otherLength );

        var result = sut.Equals( other );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foobar", 0, 6, "foobar", 0, 6, 0 )]
    [InlineData( "foobar", 0, 6, "foobar", 0, 5, 1 )]
    [InlineData( "foobar", 0, 5, "foobar", 0, 6, -1 )]
    [InlineData( "foobar", 2, 2, "foobar", 2, 2, 0 )]
    [InlineData( "foobar", 0, 3, "foo", 0, 3, 0 )]
    [InlineData( "foobar", 3, 3, "bar", 0, 3, 0 )]
    [InlineData( "a", 0, 1, "b", 0, 1, -1 )]
    [InlineData( "b", 0, 1, "a", 0, 1, 1 )]
    public void CompareTo_ShouldReturnCorrectResult(
        string source,
        int startIndex,
        int length,
        string otherSource,
        int otherStartIndex,
        int otherLength,
        int expectedSign)
    {
        var sut = new StringSlice( source, startIndex, length );
        var other = new StringSlice( otherSource, otherStartIndex, otherLength );

        var result = sut.CompareTo( other );

        Math.Sign( result ).Should().Be( expectedSign );
    }

    [Theory]
    [InlineData( 0, 1, 5, 4 )]
    [InlineData( 1, 2, 5, 3 )]
    [InlineData( 2, 3, 5, 2 )]
    [InlineData( 3, 4, 5, 1 )]
    [InlineData( 4, 5, 5, 0 )]
    [InlineData( 5, 5, 5, 0 )]
    public void Slice_WithStartIndex_ShouldReturnCorrectResult(
        int startIndex,
        int expectedStartIndex,
        int expectedEndIndex,
        int expectedLength)
    {
        var sut = new StringSlice( "foobar", 1, 4 );
        var result = sut.Slice( startIndex );

        using ( new AssertionScope() )
        {
            result.StartIndex.Should().Be( expectedStartIndex );
            result.EndIndex.Should().Be( expectedEndIndex );
            result.Length.Should().Be( expectedLength );
            result.Source.Should().BeSameAs( sut.Source );
        }
    }

    [Fact]
    public void Slice_WithStartIndex_ShouldThrowArgumentOutOfRangeException_WhenStartIndexIsLessThanZero()
    {
        var sut = new StringSlice( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.Slice( startIndex: -1 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 0, 1, 1, 0 )]
    [InlineData( 0, 1, 1, 2, 1 )]
    [InlineData( 0, 2, 1, 3, 2 )]
    [InlineData( 0, 3, 1, 4, 3 )]
    [InlineData( 0, 4, 1, 5, 4 )]
    [InlineData( 0, 5, 1, 5, 4 )]
    [InlineData( 1, 0, 2, 2, 0 )]
    [InlineData( 1, 1, 2, 3, 1 )]
    [InlineData( 1, 2, 2, 4, 2 )]
    [InlineData( 1, 3, 2, 5, 3 )]
    [InlineData( 1, 4, 2, 5, 3 )]
    [InlineData( 2, 0, 3, 3, 0 )]
    [InlineData( 2, 1, 3, 4, 1 )]
    [InlineData( 2, 2, 3, 5, 2 )]
    [InlineData( 2, 3, 3, 5, 2 )]
    [InlineData( 3, 0, 4, 4, 0 )]
    [InlineData( 3, 1, 4, 5, 1 )]
    [InlineData( 3, 2, 4, 5, 1 )]
    [InlineData( 4, 0, 5, 5, 0 )]
    [InlineData( 4, 1, 5, 5, 0 )]
    [InlineData( 5, 0, 5, 5, 0 )]
    [InlineData( 5, 1, 5, 5, 0 )]
    public void Slice_WithStartIndexAndLength_ShouldReturnCorrectResult(
        int startIndex,
        int length,
        int expectedStartIndex,
        int expectedEndIndex,
        int expectedLength)
    {
        var sut = new StringSlice( "foobar", 1, 4 );
        var result = sut.Slice( startIndex, length );

        using ( new AssertionScope() )
        {
            result.StartIndex.Should().Be( expectedStartIndex );
            result.EndIndex.Should().Be( expectedEndIndex );
            result.Length.Should().Be( expectedLength );
            result.Source.Should().BeSameAs( sut.Source );
        }
    }

    [Fact]
    public void Slice_WithStartIndexAndLength_ShouldThrowArgumentOutOfRangeException_WhenStartIndexIsLessThanZero()
    {
        var sut = new StringSlice( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.Slice( startIndex: -1, length: 0 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Slice_WithStartIndexAndLength_ShouldThrowArgumentOutOfRangeException_WhenLengthIsLessThanZero()
    {
        var sut = new StringSlice( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.Slice( startIndex: 0, length: -1 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 0, 3, 3 )]
    [InlineData( 1, 1, 4, 3 )]
    [InlineData( 2, 2, 5, 3 )]
    [InlineData( 3, 3, 6, 3 )]
    [InlineData( 4, 4, 6, 2 )]
    [InlineData( 5, 5, 6, 1 )]
    [InlineData( 6, 6, 6, 0 )]
    [InlineData( 7, 6, 6, 0 )]
    public void SetStartIndex_ShouldReturnCorrectResult(int value, int expectedStartIndex, int expectedEndIndex, int expectedLength)
    {
        var sut = new StringSlice( "foobar", 1, 3 );
        var result = sut.SetStartIndex( value );

        using ( new AssertionScope() )
        {
            result.StartIndex.Should().Be( expectedStartIndex );
            result.EndIndex.Should().Be( expectedEndIndex );
            result.Length.Should().Be( expectedLength );
            result.Source.Should().BeSameAs( sut.Source );
        }
    }

    [Fact]
    public void SetStartIndex_ShouldThrowArgumentOutOfRangeException_WhenValueIsLessThanZero()
    {
        var sut = new StringSlice( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.SetStartIndex( value: -1 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 0, 0, 0 )]
    [InlineData( 1, 0, 1, 1 )]
    [InlineData( 2, 0, 2, 2 )]
    [InlineData( 3, 0, 3, 3 )]
    [InlineData( 4, 1, 4, 3 )]
    [InlineData( 5, 2, 5, 3 )]
    [InlineData( 6, 3, 6, 3 )]
    [InlineData( 7, 4, 6, 2 )]
    [InlineData( 8, 5, 6, 1 )]
    [InlineData( 9, 6, 6, 0 )]
    [InlineData( 10, 6, 6, 0 )]
    public void SetEndIndex_ShouldReturnCorrectResult(int value, int expectedStartIndex, int expectedEndIndex, int expectedLength)
    {
        var sut = new StringSlice( "foobar", 1, 3 );
        var result = sut.SetEndIndex( value );

        using ( new AssertionScope() )
        {
            result.StartIndex.Should().Be( expectedStartIndex );
            result.EndIndex.Should().Be( expectedEndIndex );
            result.Length.Should().Be( expectedLength );
            result.Source.Should().BeSameAs( sut.Source );
        }
    }

    [Fact]
    public void SetEndIndex_ShouldThrowArgumentOutOfRangeException_WhenValueIsLessThanZero()
    {
        var sut = new StringSlice( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.SetEndIndex( value: -1 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 1, 1, 0 )]
    [InlineData( 1, 1, 2, 1 )]
    [InlineData( 2, 1, 3, 2 )]
    [InlineData( 3, 1, 4, 3 )]
    [InlineData( 4, 1, 5, 4 )]
    [InlineData( 5, 1, 6, 5 )]
    [InlineData( 6, 1, 6, 5 )]
    public void SetLength_ShouldReturnCorrectResult(int value, int expectedStartIndex, int expectedEndIndex, int expectedLength)
    {
        var sut = new StringSlice( "foobar", 1, 3 );
        var result = sut.SetLength( value );

        using ( new AssertionScope() )
        {
            result.StartIndex.Should().Be( expectedStartIndex );
            result.EndIndex.Should().Be( expectedEndIndex );
            result.Length.Should().Be( expectedLength );
            result.Source.Should().BeSameAs( sut.Source );
        }
    }

    [Fact]
    public void SetLength_ShouldThrowArgumentOutOfRangeException_WhenValueIsLessThanZero()
    {
        var sut = new StringSlice( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.SetLength( value: -1 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( -6, 0, 0, 0 )]
    [InlineData( -5, 0, 0, 0 )]
    [InlineData( -4, 0, 1, 1 )]
    [InlineData( -3, 0, 2, 2 )]
    [InlineData( -2, 0, 3, 3 )]
    [InlineData( -1, 1, 4, 3 )]
    [InlineData( 0, 2, 5, 3 )]
    [InlineData( 1, 3, 6, 3 )]
    [InlineData( 2, 4, 6, 2 )]
    [InlineData( 3, 5, 6, 1 )]
    [InlineData( 4, 6, 6, 0 )]
    [InlineData( 5, 6, 6, 0 )]
    public void Offset_ShouldReturnCorrectResult(int offset, int expectedStartIndex, int expectedEndIndex, int expectedLength)
    {
        var sut = new StringSlice( "foobar", 2, 3 );
        var result = sut.Offset( offset );

        using ( new AssertionScope() )
        {
            result.StartIndex.Should().Be( expectedStartIndex );
            result.EndIndex.Should().Be( expectedEndIndex );
            result.Length.Should().Be( expectedLength );
            result.Source.Should().BeSameAs( sut.Source );
        }
    }

    [Theory]
    [InlineData( 0, 0, 3, 3, 0 )]
    [InlineData( 1, 0, 2, 4, 2 )]
    [InlineData( 2, 0, 1, 5, 4 )]
    [InlineData( 3, 0, 0, 6, 6 )]
    [InlineData( 4, 0, 0, 6, 6 )]
    [InlineData( 0, 1, 3, 4, 1 )]
    [InlineData( 1, 1, 2, 5, 3 )]
    [InlineData( 2, 1, 1, 6, 5 )]
    [InlineData( 3, 1, 0, 6, 6 )]
    [InlineData( 4, 1, 0, 6, 6 )]
    public void Expand_ShouldReturnCorrectResult(
        int count,
        int startLength,
        int expectedStartIndex,
        int expectedEndIndex,
        int expectedLength)
    {
        var sut = new StringSlice( "foobar", 3, startLength );
        var result = sut.Expand( count );

        using ( new AssertionScope() )
        {
            result.StartIndex.Should().Be( expectedStartIndex );
            result.EndIndex.Should().Be( expectedEndIndex );
            result.Length.Should().Be( expectedLength );
            result.Source.Should().BeSameAs( sut.Source );
        }
    }

    [Fact]
    public void Expand_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanZero()
    {
        var sut = new StringSlice( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.Expand( count: -1 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 6, 0, 6, 6 )]
    [InlineData( 1, 6, 1, 5, 4 )]
    [InlineData( 2, 6, 2, 4, 2 )]
    [InlineData( 3, 6, 3, 3, 0 )]
    [InlineData( 4, 6, 3, 3, 0 )]
    [InlineData( 10, 6, 3, 3, 0 )]
    [InlineData( 0, 5, 0, 5, 5 )]
    [InlineData( 1, 5, 1, 4, 3 )]
    [InlineData( 2, 5, 2, 3, 1 )]
    [InlineData( 3, 5, 2, 2, 0 )]
    [InlineData( 4, 5, 2, 2, 0 )]
    [InlineData( 10, 5, 2, 2, 0 )]
    public void Shrink_ShouldReturnCorrectResult(
        int count,
        int startLength,
        int expectedStartIndex,
        int expectedEndIndex,
        int expectedLength)
    {
        var sut = new StringSlice( "foobar", 0, startLength );
        var result = sut.Shrink( count );

        using ( new AssertionScope() )
        {
            result.StartIndex.Should().Be( expectedStartIndex );
            result.EndIndex.Should().Be( expectedEndIndex );
            result.Length.Should().Be( expectedLength );
            result.Source.Should().BeSameAs( sut.Source );
        }
    }

    [Fact]
    public void Shrink_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanZero()
    {
        var sut = new StringSlice( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.Shrink( count: -1 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 6 )]
    [InlineData( 1, 4 )]
    [InlineData( 2, 2 )]
    public void AsMemory_ShouldReturnCorrectResult(int startIndex, int length)
    {
        var source = "foobar";
        var sut = new StringSlice( source, startIndex, length );
        var expected = source.AsMemory( startIndex, length );

        var result = sut.AsMemory();

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, 6 )]
    [InlineData( 1, 4 )]
    [InlineData( 2, 2 )]
    public void AsSpan_ShouldReturnCorrectResult(int startIndex, int length)
    {
        var source = "foobar";
        var sut = new StringSlice( source, startIndex, length );
        var expected = source.AsSpan( startIndex, length );

        var result = sut.AsSpan();

        result.ToString().Should().Be( expected.ToString() );
    }

    [Theory]
    [InlineData( -1, 'f' )]
    [InlineData( 0, 'o' )]
    [InlineData( 1, 'o' )]
    [InlineData( 2, 'b' )]
    [InlineData( 3, 'a' )]
    [InlineData( 4, 'r' )]
    public void GetIndexer_ShouldReturnCorrectResult(int index, char expected)
    {
        var sut = new StringSlice( "foobar", 1, 4 );
        var result = sut[index];
        result.Should().Be( expected );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        IEnumerable<char> sut = new StringSlice( "foobar", 1, 4 );
        sut.Should().BeSequentiallyEqualTo( 'o', 'o', 'b', 'a' );
    }

    [Theory]
    [InlineData( "foobar", 0, 6, "foobar", 0, 6, true )]
    [InlineData( "foobar", 0, 6, "foobar", 0, 5, false )]
    [InlineData( "foobar", 0, 5, "foobar", 0, 6, false )]
    [InlineData( "foobar", 2, 2, "foobar", 2, 2, true )]
    [InlineData( "foobar", 0, 3, "foo", 0, 3, true )]
    [InlineData( "foobar", 3, 3, "bar", 0, 3, true )]
    public void EqualityOperator_ShouldReturnCorrectResult(
        string source,
        int startIndex,
        int length,
        string otherSource,
        int otherStartIndex,
        int otherLength,
        bool expected)
    {
        var sut = new StringSlice( source, startIndex, length );
        var other = new StringSlice( otherSource, otherStartIndex, otherLength );

        var result = sut == other;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foobar", 0, 6, "foobar", 0, 6, false )]
    [InlineData( "foobar", 0, 6, "foobar", 0, 5, true )]
    [InlineData( "foobar", 0, 5, "foobar", 0, 6, true )]
    [InlineData( "foobar", 2, 2, "foobar", 2, 2, false )]
    [InlineData( "foobar", 0, 3, "foo", 0, 3, false )]
    [InlineData( "foobar", 3, 3, "bar", 0, 3, false )]
    public void InequalityOperator_ShouldReturnCorrectResult(
        string source,
        int startIndex,
        int length,
        string otherSource,
        int otherStartIndex,
        int otherLength,
        bool expected)
    {
        var sut = new StringSlice( source, startIndex, length );
        var other = new StringSlice( otherSource, otherStartIndex, otherLength );

        var result = sut != other;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foobar", 0, 6, "foobar", 0, 6, false )]
    [InlineData( "foobar", 0, 6, "foobar", 0, 5, true )]
    [InlineData( "foobar", 0, 5, "foobar", 0, 6, false )]
    [InlineData( "foobar", 2, 2, "foobar", 2, 2, false )]
    [InlineData( "foobar", 0, 3, "foo", 0, 3, false )]
    [InlineData( "foobar", 3, 3, "bar", 0, 3, false )]
    [InlineData( "a", 0, 1, "b", 0, 1, false )]
    [InlineData( "b", 0, 1, "a", 0, 1, true )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(
        string source,
        int startIndex,
        int length,
        string otherSource,
        int otherStartIndex,
        int otherLength,
        bool expected)
    {
        var sut = new StringSlice( source, startIndex, length );
        var other = new StringSlice( otherSource, otherStartIndex, otherLength );

        var result = sut > other;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foobar", 0, 6, "foobar", 0, 6, false )]
    [InlineData( "foobar", 0, 6, "foobar", 0, 5, false )]
    [InlineData( "foobar", 0, 5, "foobar", 0, 6, true )]
    [InlineData( "foobar", 2, 2, "foobar", 2, 2, false )]
    [InlineData( "foobar", 0, 3, "foo", 0, 3, false )]
    [InlineData( "foobar", 3, 3, "bar", 0, 3, false )]
    [InlineData( "a", 0, 1, "b", 0, 1, true )]
    [InlineData( "b", 0, 1, "a", 0, 1, false )]
    public void LessThanOperator_ShouldReturnCorrectResult(
        string source,
        int startIndex,
        int length,
        string otherSource,
        int otherStartIndex,
        int otherLength,
        bool expected)
    {
        var sut = new StringSlice( source, startIndex, length );
        var other = new StringSlice( otherSource, otherStartIndex, otherLength );

        var result = sut < other;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foobar", 0, 6, "foobar", 0, 6, true )]
    [InlineData( "foobar", 0, 6, "foobar", 0, 5, true )]
    [InlineData( "foobar", 0, 5, "foobar", 0, 6, false )]
    [InlineData( "foobar", 2, 2, "foobar", 2, 2, true )]
    [InlineData( "foobar", 0, 3, "foo", 0, 3, true )]
    [InlineData( "foobar", 3, 3, "bar", 0, 3, true )]
    [InlineData( "a", 0, 1, "b", 0, 1, false )]
    [InlineData( "b", 0, 1, "a", 0, 1, true )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(
        string source,
        int startIndex,
        int length,
        string otherSource,
        int otherStartIndex,
        int otherLength,
        bool expected)
    {
        var sut = new StringSlice( source, startIndex, length );
        var other = new StringSlice( otherSource, otherStartIndex, otherLength );

        var result = sut >= other;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foobar", 0, 6, "foobar", 0, 6, true )]
    [InlineData( "foobar", 0, 6, "foobar", 0, 5, false )]
    [InlineData( "foobar", 0, 5, "foobar", 0, 6, true )]
    [InlineData( "foobar", 2, 2, "foobar", 2, 2, true )]
    [InlineData( "foobar", 0, 3, "foo", 0, 3, true )]
    [InlineData( "foobar", 3, 3, "bar", 0, 3, true )]
    [InlineData( "a", 0, 1, "b", 0, 1, true )]
    [InlineData( "b", 0, 1, "a", 0, 1, false )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(
        string source,
        int startIndex,
        int length,
        string otherSource,
        int otherStartIndex,
        int otherLength,
        bool expected)
    {
        var sut = new StringSlice( source, startIndex, length );
        var other = new StringSlice( otherSource, otherStartIndex, otherLength );

        var result = sut <= other;

        result.Should().Be( expected );
    }
}
