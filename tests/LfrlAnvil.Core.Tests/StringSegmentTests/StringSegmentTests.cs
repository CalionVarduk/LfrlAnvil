using System.Collections.Generic;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.StringSegmentTests;

public class StringSegmentTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnEmptySegment()
    {
        var sut = default( StringSegment );

        Assertion.All(
                sut.StartIndex.TestEquals( 0 ),
                sut.Length.TestEquals( 0 ),
                sut.EndIndex.TestEquals( 0 ),
                sut.Source.TestRefEquals( string.Empty ) )
            .Go();
    }

    [Fact]
    public void Empty_ShouldReturnEmptySegment()
    {
        var sut = StringSegment.Empty;

        Assertion.All(
                sut.StartIndex.TestEquals( 0 ),
                sut.Length.TestEquals( 0 ),
                sut.EndIndex.TestEquals( 0 ),
                sut.Source.TestRefEquals( string.Empty ) )
            .Go();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( "foo" )]
    [InlineData( "foobar" )]
    public void Ctor_WithSource_ShouldReturnCorrectResult(string source)
    {
        var sut = new StringSegment( source );

        Assertion.All(
                sut.StartIndex.TestEquals( 0 ),
                sut.Length.TestEquals( source.Length ),
                sut.EndIndex.TestEquals( source.Length ),
                sut.Source.TestRefEquals( source ) )
            .Go();
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
        var sut = new StringSegment( source, startIndex );

        Assertion.All(
                sut.StartIndex.TestEquals( expectedStartIndex ),
                sut.Length.TestEquals( expectedLength ),
                sut.EndIndex.TestEquals( expectedEndIndex ),
                sut.Source.TestRefEquals( source ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithSourceAndStartIndex_ShouldThrowArgumentOutOfRangeException_WhenStartIndexIsLessThanZero()
    {
        var action = Lambda.Of( () => new StringSegment( Fixture.Create<string>(), startIndex: -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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
        var sut = new StringSegment( source, startIndex, length );

        Assertion.All(
                sut.StartIndex.TestEquals( expectedStartIndex ),
                sut.Length.TestEquals( expectedLength ),
                sut.EndIndex.TestEquals( expectedEndIndex ),
                sut.Source.TestRefEquals( source ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithSourceAndStartIndexAndLength_ShouldThrowArgumentOutOfRangeException_WhenStartIndexIsLessThanZero()
    {
        var action = Lambda.Of( () => new StringSegment( Fixture.Create<string>(), startIndex: -1, length: 0 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithSourceAndStartIndexAndLength_ShouldThrowArgumentOutOfRangeException_WhenLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => new StringSegment( Fixture.Create<string>(), startIndex: 0, length: -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( "foobar", 0, 6 )]
    [InlineData( "foobar", 1, 5 )]
    [InlineData( "foobar", 2, 2 )]
    public void FromMemory_ShouldReturnCorrectResult_WhenUnderlyingObjectIsString(string source, int startIndex, int length)
    {
        var memory = source.AsMemory( startIndex, length );
        var sut = StringSegment.FromMemory( memory );

        Assertion.All(
                sut.StartIndex.TestEquals( startIndex ),
                sut.Length.TestEquals( length ),
                sut.EndIndex.TestEquals( startIndex + length ),
                sut.Source.TestRefEquals( source ) )
            .Go();
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
        var sut = StringSegment.FromMemory( memory );

        Assertion.All(
                sut.StartIndex.TestEquals( 0 ),
                sut.Length.TestEquals( length ),
                sut.EndIndex.TestEquals( length ),
                sut.Source.TestEquals( expectedSource ) )
            .Go();
    }

    [Theory]
    [InlineData( "foobar", 0, 6, "foobar" )]
    [InlineData( "foobar", 1, 5, "oobar" )]
    [InlineData( "foobar", 0, 5, "fooba" )]
    [InlineData( "foobar", 2, 2, "ob" )]
    public void ToString_ShouldReturnCorrectResult(string source, int startIndex, int length, string expected)
    {
        var sut = new StringSegment( source, startIndex, length );
        var result = sut.ToString();
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = new StringSegment( "foobar", 2, 2 );
        var expected = string.GetHashCode( sut.ToString().AsSpan() );
        var result = sut.GetHashCode();
        result.TestEquals( expected ).Go();
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
        var sut = new StringSegment( source, startIndex, length );
        var other = new StringSegment( otherSource, otherStartIndex, otherLength );

        var result = sut.Equals( other );

        result.TestEquals( expected ).Go();
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
        var sut = new StringSegment( source, startIndex, length );
        var other = new StringSegment( otherSource, otherStartIndex, otherLength );

        var result = sut.CompareTo( other );

        Math.Sign( result ).TestEquals( expectedSign ).Go();
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
        var sut = new StringSegment( "foobar", 1, 4 );
        var result = sut.Slice( startIndex );

        Assertion.All(
                result.StartIndex.TestEquals( expectedStartIndex ),
                result.EndIndex.TestEquals( expectedEndIndex ),
                result.Length.TestEquals( expectedLength ),
                result.Source.TestRefEquals( sut.Source ) )
            .Go();
    }

    [Fact]
    public void Slice_WithStartIndex_ShouldThrowArgumentOutOfRangeException_WhenStartIndexIsLessThanZero()
    {
        var sut = new StringSegment( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.Slice( startIndex: -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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
        var sut = new StringSegment( "foobar", 1, 4 );
        var result = sut.Slice( startIndex, length );

        Assertion.All(
                result.StartIndex.TestEquals( expectedStartIndex ),
                result.EndIndex.TestEquals( expectedEndIndex ),
                result.Length.TestEquals( expectedLength ),
                result.Source.TestRefEquals( sut.Source ) )
            .Go();
    }

    [Fact]
    public void Slice_WithStartIndexAndLength_ShouldThrowArgumentOutOfRangeException_WhenStartIndexIsLessThanZero()
    {
        var sut = new StringSegment( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.Slice( startIndex: -1, length: 0 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Slice_WithStartIndexAndLength_ShouldThrowArgumentOutOfRangeException_WhenLengthIsLessThanZero()
    {
        var sut = new StringSegment( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.Slice( startIndex: 0, length: -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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
        var sut = new StringSegment( "foobar", 1, 3 );
        var result = sut.SetStartIndex( value );

        Assertion.All(
                result.StartIndex.TestEquals( expectedStartIndex ),
                result.EndIndex.TestEquals( expectedEndIndex ),
                result.Length.TestEquals( expectedLength ),
                result.Source.TestRefEquals( sut.Source ) )
            .Go();
    }

    [Fact]
    public void SetStartIndex_ShouldThrowArgumentOutOfRangeException_WhenValueIsLessThanZero()
    {
        var sut = new StringSegment( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.SetStartIndex( value: -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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
        var sut = new StringSegment( "foobar", 1, 3 );
        var result = sut.SetEndIndex( value );

        Assertion.All(
                result.StartIndex.TestEquals( expectedStartIndex ),
                result.EndIndex.TestEquals( expectedEndIndex ),
                result.Length.TestEquals( expectedLength ),
                result.Source.TestRefEquals( sut.Source ) )
            .Go();
    }

    [Fact]
    public void SetEndIndex_ShouldThrowArgumentOutOfRangeException_WhenValueIsLessThanZero()
    {
        var sut = new StringSegment( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.SetEndIndex( value: -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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
        var sut = new StringSegment( "foobar", 1, 3 );
        var result = sut.SetLength( value );

        Assertion.All(
                result.StartIndex.TestEquals( expectedStartIndex ),
                result.EndIndex.TestEquals( expectedEndIndex ),
                result.Length.TestEquals( expectedLength ),
                result.Source.TestRefEquals( sut.Source ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldThrowArgumentOutOfRangeException_WhenValueIsLessThanZero()
    {
        var sut = new StringSegment( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.SetLength( value: -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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
        var sut = new StringSegment( "foobar", 2, 3 );
        var result = sut.Offset( offset );

        Assertion.All(
                result.StartIndex.TestEquals( expectedStartIndex ),
                result.EndIndex.TestEquals( expectedEndIndex ),
                result.Length.TestEquals( expectedLength ),
                result.Source.TestRefEquals( sut.Source ) )
            .Go();
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
        var sut = new StringSegment( "foobar", 3, startLength );
        var result = sut.Expand( count );

        Assertion.All(
                result.StartIndex.TestEquals( expectedStartIndex ),
                result.EndIndex.TestEquals( expectedEndIndex ),
                result.Length.TestEquals( expectedLength ),
                result.Source.TestRefEquals( sut.Source ) )
            .Go();
    }

    [Fact]
    public void Expand_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanZero()
    {
        var sut = new StringSegment( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.Expand( count: -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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
        var sut = new StringSegment( "foobar", 0, startLength );
        var result = sut.Shrink( count );

        Assertion.All(
                result.StartIndex.TestEquals( expectedStartIndex ),
                result.EndIndex.TestEquals( expectedEndIndex ),
                result.Length.TestEquals( expectedLength ),
                result.Source.TestRefEquals( sut.Source ) )
            .Go();
    }

    [Fact]
    public void Shrink_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanZero()
    {
        var sut = new StringSegment( "foobar", 1, 4 );
        var action = Lambda.Of( () => sut.Shrink( count: -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 0, 6 )]
    [InlineData( 1, 4 )]
    [InlineData( 2, 2 )]
    public void AsMemory_ShouldReturnCorrectResult(int startIndex, int length)
    {
        var source = "foobar";
        var sut = new StringSegment( source, startIndex, length );
        var expected = source.AsMemory( startIndex, length );

        var result = sut.AsMemory();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 0, 6 )]
    [InlineData( 1, 4 )]
    [InlineData( 2, 2 )]
    public void AsSpan_ShouldReturnCorrectResult(int startIndex, int length)
    {
        var source = "foobar";
        var sut = new StringSegment( source, startIndex, length );
        var expected = source.AsSpan( startIndex, length );

        var result = sut.AsSpan();

        result.ToString().TestEquals( expected.ToString() ).Go();
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
        var sut = new StringSegment( "foobar", 1, 4 );
        var result = sut[index];
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        IEnumerable<char> sut = new StringSegment( "foobar", 1, 4 );
        sut.TestSequence( [ 'o', 'o', 'b', 'a' ] ).Go();
    }

    [Fact]
    public void StringSegmentConversionOperator_ShouldReturnCorrectResult()
    {
        var source = Fixture.Create<string>();
        var result = ( StringSegment )source;

        Assertion.All(
                result.Source.TestRefEquals( source ),
                result.StartIndex.TestEquals( 0 ),
                result.Length.TestEquals( source.Length ) )
            .Go();
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
        var sut = new StringSegment( source, startIndex, length );
        var other = new StringSegment( otherSource, otherStartIndex, otherLength );

        var result = sut == other;

        result.TestEquals( expected ).Go();
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
        var sut = new StringSegment( source, startIndex, length );
        var other = new StringSegment( otherSource, otherStartIndex, otherLength );

        var result = sut != other;

        result.TestEquals( expected ).Go();
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
        var sut = new StringSegment( source, startIndex, length );
        var other = new StringSegment( otherSource, otherStartIndex, otherLength );

        var result = sut > other;

        result.TestEquals( expected ).Go();
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
        var sut = new StringSegment( source, startIndex, length );
        var other = new StringSegment( otherSource, otherStartIndex, otherLength );

        var result = sut < other;

        result.TestEquals( expected ).Go();
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
        var sut = new StringSegment( source, startIndex, length );
        var other = new StringSegment( otherSource, otherStartIndex, otherLength );

        var result = sut >= other;

        result.TestEquals( expected ).Go();
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
        var sut = new StringSegment( source, startIndex, length );
        var other = new StringSegment( otherSource, otherStartIndex, otherLength );

        var result = sut <= other;

        result.TestEquals( expected ).Go();
    }
}
