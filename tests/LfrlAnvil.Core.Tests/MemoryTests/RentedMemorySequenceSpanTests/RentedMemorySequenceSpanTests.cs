using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Memory;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.MemoryTests.RentedMemorySequenceSpanTests;

public class RentedMemorySequenceSpanTests : TestsBase
{
    [Fact]
    public void Default_ShouldHaveNoElementsAndSegments()
    {
        var sut = default( RentedMemorySequenceSpan<int> );

        using ( new AssertionScope() )
        {
            sut.StartIndex.Should().Be( 0 );
            sut.Length.Should().Be( 0 );
            sut.Segments.ToArray().Should().BeEmpty();
            sut.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Empty_ShouldHaveNoElementsAndSegments()
    {
        var sut = RentedMemorySequenceSpan<int>.Empty;

        using ( new AssertionScope() )
        {
            sut.StartIndex.Should().Be( 0 );
            sut.Length.Should().Be( 0 );
            sut.Segments.ToArray().Should().BeEmpty();
            sut.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void GetIndexer_ShouldReturnCorrectElement()
    {
        var pool = new MemorySequencePool<int>( 8 );

        var seq = pool.Rent( 16 );
        seq.CopyFrom( Enumerable.Range( 1, 16 ).ToArray() );
        var sut = seq.Slice( 2, 12 );

        var result = new int[12];
        for ( var i = 0; i < sut.Length; ++i )
            result[i] = sut[i];

        result.Should().BeSequentiallyEqualTo( 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 12 )]
    [InlineData( 13 )]
    public void GetIndexer_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfBounds(int index)
    {
        var pool = new MemorySequencePool<int>( 8 );
        var seq = pool.Rent( 16 );
        var sut = seq.Slice( 2, 12 );

        ArgumentOutOfRangeException? exception = null;
        try
        {
            var _ = sut[index];
        }
        catch ( ArgumentOutOfRangeException e )
        {
            exception = e;
        }

        exception.Should().NotBeNull();
    }

    [Fact]
    public void SetIndexer_ShouldUpdateCorrectElement()
    {
        var pool = new MemorySequencePool<int>( 8 );

        var seq = pool.Rent( 16 );
        var sut = seq.Slice( 2, 12 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var result = new int[16];
        seq.CopyTo( result );

        result.Should().BeSequentiallyEqualTo( 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 0, 0 );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 12 )]
    [InlineData( 13 )]
    public void SetIndexer_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfBounds(int index)
    {
        var pool = new MemorySequencePool<int>( 8 );
        var seq = pool.Rent( 16 );
        var sut = seq.Slice( 2, 12 );

        ArgumentOutOfRangeException? exception = null;
        try
        {
            sut[index] = Fixture.Create<int>();
        }
        catch ( ArgumentOutOfRangeException e )
        {
            exception = e;
        }

        exception.Should().NotBeNull();
    }

    [Fact]
    public void GetRef_ShouldReturnReferenceToCorrectElement()
    {
        var pool = new MemorySequencePool<int>( 8 );

        var seq = pool.Rent( 16 );
        var sut = seq.Slice( 2, 12 );
        sut.CopyFrom( Enumerable.Range( 1, 12 ).ToArray() );

        ref var result = ref sut.GetRef( 8 );
        result = 20;

        seq.Should().BeSequentiallyEqualTo( 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 20, 10, 11, 12, 0, 0 );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 12 )]
    [InlineData( 13 )]
    public void GetRef_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfBounds(int index)
    {
        var pool = new MemorySequencePool<int>( 8 );
        var seq = pool.Rent( 16 );
        var sut = seq.Slice( 2, 12 );

        ArgumentOutOfRangeException? exception = null;
        try
        {
            _ = sut.GetRef( index );
        }
        catch ( ArgumentOutOfRangeException e )
        {
            exception = e;
        }

        exception.Should().NotBeNull();
    }

    [Fact]
    public void Segments_ShouldReturnCorrectArraySegments()
    {
        var pool = new MemorySequencePool<int>( 8 );
        pool.Rent( 1 );
        var seq = pool.Rent( 32 );
        var sut = seq.Slice( 2, 28 );
        pool.Rent( 5 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        var result = sut.Segments;

        using ( new AssertionScope() )
        {
            result.Length.Should().Be( 4 );
            result[0].Should().BeSequentiallyEqualTo( 3, 4, 5, 6, 7 );
            result[1].Should().BeSequentiallyEqualTo( 8, 9, 10, 11, 12, 13, 14, 15 );
            result[2].Should().BeSequentiallyEqualTo( 16, 17, 18, 19, 20, 21, 22, 23 );
            result[3].Should().BeSequentiallyEqualTo( 24, 25, 26, 27, 28, 29, 30 );
            result.ToArray().Should().BeSequentiallyEqualTo( result[0], result[1], result[2], result[3] );
            result.ToArray().SelectMany( s => s ).Should().BeSequentiallyEqualTo( sut.ToArray() );
            result.ToString().Should().Be( "RentedMemorySequenceSegmentCollection<Int32>[4]" );
        }
    }

    [Theory]
    [InlineData( -2, 10 )]
    [InlineData( -1, 9 )]
    [InlineData( 0, 8 )]
    [InlineData( 1, 7 )]
    [InlineData( 2, 6 )]
    [InlineData( 3, 5 )]
    [InlineData( 4, 4 )]
    [InlineData( 5, 3 )]
    [InlineData( 6, 2 )]
    [InlineData( 7, 1 )]
    [InlineData( 8, 0 )]
    public void Slice_ShouldReturnSpanWithCorrectLength(int startIndex, int expectedLength)
    {
        var pool = new MemorySequencePool<int>( 4 );
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );

        var result = sut.Slice( startIndex );

        using ( new AssertionScope() )
        {
            result.StartIndex.Should().Be( startIndex + 2 );
            result.Length.Should().Be( expectedLength );
        }
    }

    [Theory]
    [InlineData( -3 )]
    [InlineData( 9 )]
    public void Slice_ShouldThrowArgumentOutOfRangeException_WhenStartIndexIsOutOfBounds(int startIndex)
    {
        var pool = new MemorySequencePool<int>( 4 );
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );

        ArgumentOutOfRangeException? exception = null;
        try
        {
            var _ = sut.Slice( startIndex );
        }
        catch ( ArgumentOutOfRangeException e )
        {
            exception = e;
        }

        exception.Should().NotBeNull();
    }

    [Theory]
    [InlineData( -2, 12 )]
    [InlineData( -1, 11 )]
    [InlineData( 0, 10 )]
    [InlineData( 0, 9 )]
    [InlineData( 0, 8 )]
    [InlineData( 0, 7 )]
    [InlineData( 0, 6 )]
    [InlineData( 0, 5 )]
    [InlineData( 0, 4 )]
    [InlineData( 0, 3 )]
    [InlineData( 0, 2 )]
    [InlineData( 0, 1 )]
    [InlineData( 0, 0 )]
    [InlineData( 1, 7 )]
    [InlineData( 1, 6 )]
    [InlineData( 1, 5 )]
    [InlineData( 1, 4 )]
    [InlineData( 1, 3 )]
    [InlineData( 1, 2 )]
    [InlineData( 1, 1 )]
    [InlineData( 1, 0 )]
    [InlineData( 4, 4 )]
    [InlineData( 4, 3 )]
    [InlineData( 4, 2 )]
    [InlineData( 4, 1 )]
    [InlineData( 4, 0 )]
    [InlineData( 7, 1 )]
    [InlineData( 7, 0 )]
    [InlineData( 8, 0 )]
    [InlineData( 9, 1 )]
    [InlineData( 10, 0 )]
    public void Slice_WithLength_ShouldReturnCorrectSpan(int startIndex, int length)
    {
        var pool = new MemorySequencePool<int>( 4 );
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );

        var result = sut.Slice( startIndex, length );

        using ( new AssertionScope() )
        {
            result.StartIndex.Should().Be( startIndex + 2 );
            result.Length.Should().Be( length );
        }
    }

    [Theory]
    [InlineData( -3, 0 )]
    [InlineData( -2, -1 )]
    [InlineData( -2, 13 )]
    [InlineData( -1, -1 )]
    [InlineData( -1, 12 )]
    [InlineData( 0, -1 )]
    [InlineData( 0, 11 )]
    [InlineData( 1, -1 )]
    [InlineData( 1, 10 )]
    [InlineData( 2, -1 )]
    [InlineData( 2, 9 )]
    [InlineData( 3, -1 )]
    [InlineData( 3, 8 )]
    [InlineData( 4, -1 )]
    [InlineData( 4, 7 )]
    [InlineData( 5, -1 )]
    [InlineData( 5, 6 )]
    [InlineData( 6, -1 )]
    [InlineData( 6, 5 )]
    [InlineData( 7, -1 )]
    [InlineData( 7, 4 )]
    [InlineData( 8, -1 )]
    [InlineData( 8, 3 )]
    [InlineData( 9, -1 )]
    [InlineData( 9, 2 )]
    [InlineData( 10, -1 )]
    [InlineData( 10, 1 )]
    [InlineData( 11, 0 )]
    public void Slice_WithLength_ShouldThrowArgumentOutOfRangeException_WhenStartIndexOrLengthAreOutOfBounds(int startIndex, int length)
    {
        var pool = new MemorySequencePool<int>( 4 );
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );

        ArgumentOutOfRangeException? exception = null;
        try
        {
            var _ = sut.Slice( startIndex, length );
        }
        catch ( ArgumentOutOfRangeException e )
        {
            exception = e;
        }

        exception.Should().NotBeNull();
    }

    [Fact]
    public void ToString_ShouldReturnTypeAndLengthInfo()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var seq = pool.Rent( 16 );
        var sut = seq.Slice( 2, 10 );

        var result = sut.ToString();

        result.Should().Be( "RentedMemorySequenceSpan<Int32>[10]" );
    }

    [Theory]
    [InlineData( 0, false )]
    [InlineData( 1, false )]
    [InlineData( 2, false )]
    [InlineData( 3, true )]
    [InlineData( 4, true )]
    [InlineData( 5, true )]
    [InlineData( 6, true )]
    [InlineData( 7, false )]
    [InlineData( 8, false )]
    [InlineData( 9, false )]
    public void Contains_ShouldReturnCorrectResult_WhenSequenceIsContainedInOneSegment(int value, bool expected)
    {
        var pool = new MemorySequencePool<int>( 8 );
        pool.Rent( 1 );
        var seq = pool.Rent( 8 );
        var sut = seq.Slice( 2, 4 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        var result = sut.Contains( value );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, false )]
    [InlineData( 1, false )]
    [InlineData( 2, false )]
    [InlineData( 3, true )]
    [InlineData( 4, true )]
    [InlineData( 5, true )]
    [InlineData( 6, true )]
    [InlineData( 7, true )]
    [InlineData( 8, true )]
    [InlineData( 9, true )]
    [InlineData( 10, true )]
    [InlineData( 11, true )]
    [InlineData( 12, true )]
    [InlineData( 13, true )]
    [InlineData( 14, true )]
    [InlineData( 15, true )]
    [InlineData( 16, true )]
    [InlineData( 17, true )]
    [InlineData( 18, true )]
    [InlineData( 19, true )]
    [InlineData( 20, true )]
    [InlineData( 21, false )]
    [InlineData( 22, false )]
    [InlineData( 23, false )]
    public void Contains_ShouldReturnCorrectResult_WhenSequenceIsSpreadOutAcrossMultipleSegments(int value, bool expected)
    {
        var pool = new MemorySequencePool<int>( 8 );
        pool.Rent( 1 );
        var seq = pool.Rent( 22 );
        var sut = seq.Slice( 2, 18 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        var result = sut.Contains( value );

        result.Should().Be( expected );
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        seq.Dispose();

        var result = sut.Contains( default );

        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        pool.Rent( 8 );
        seq.Dispose();

        var result = sut.Contains( default );

        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenSequenceIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var seq = pool.Rent( 8 );
        var sut = seq.Slice( 4, 0 );
        pool.Rent( 8 );

        var result = sut.Contains( default );

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    [InlineData( 10 )]
    [InlineData( 15 )]
    [InlineData( 20 )]
    public void Clear_ShouldSetElementsToDefaultValue(int length)
    {
        var pool = new MemorySequencePool<int>( 8 );

        var first = pool.Rent( 1 );
        for ( var i = 0; i < first.Length; ++i )
            first[i] = -1;

        var seq = pool.Rent( length + 4 );
        var sut = seq.Slice( 2, length );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        var third = pool.Rent( 5 );
        for ( var i = 0; i < third.Length; ++i )
            third[i] = -2;

        sut.Clear();

        using ( new AssertionScope() )
        {
            first.Should().AllBeEquivalentTo( -1 );
            third.Should().AllBeEquivalentTo( -2 );
            sut.ToArray().Should().AllBeEquivalentTo( default( int ) );
            seq.Take( 2 ).Should().BeSequentiallyEqualTo( 1, 2 );
            seq.TakeLast( 2 ).Should().BeSequentiallyEqualTo( length + 3, length + 4 );
        }
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        seq.CopyFrom( Enumerable.Range( 1, 12 ).ToArray() );
        seq.Dispose();

        sut.Clear();

        pool.Rent( 12 ).Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 );
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        pool.Rent( 8 );
        seq.CopyFrom( Enumerable.Range( 1, 12 ).ToArray() );
        seq.Dispose();

        sut.Clear();

        pool.Rent( 12 ).Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 );
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenSequenceIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );

        var first = pool.Rent( 3 );
        for ( var i = 0; i < first.Length; ++i )
            first[i] = -1;

        var seq = pool.Rent( 8 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = -2;

        var sut = seq.Slice( 4, 0 );

        var second = pool.Rent( 3 );
        for ( var i = 0; i < second.Length; ++i )
            second[i] = -3;

        sut.Clear();

        using ( new AssertionScope() )
        {
            first.Should().AllBeEquivalentTo( -1 );
            seq.Should().AllBeEquivalentTo( -2 );
            second.Should().AllBeEquivalentTo( -3 );
        }
    }

    [Theory]
    [InlineData( 1, 1, 0 )]
    [InlineData( 1, 2, 1 )]
    [InlineData( 1, 3, 1 )]
    [InlineData( 4, 4, 0 )]
    [InlineData( 4, 5, 0 )]
    [InlineData( 4, 5, 1 )]
    [InlineData( 4, 7, 2 )]
    [InlineData( 10, 10, 0 )]
    [InlineData( 10, 15, 0 )]
    [InlineData( 10, 15, 3 )]
    [InlineData( 30, 30, 0 )]
    [InlineData( 30, 40, 0 )]
    [InlineData( 30, 40, 4 )]
    [InlineData( 30, 40, 10 )]
    public void CopyTo_ShouldCopyElementsCorrectly(int length, int arrayLength, int arrayIndex)
    {
        var pool = new MemorySequencePool<int>( 8 );
        pool.Rent( 1 );
        var seq = pool.Rent( length + 4 );
        var sut = seq.Slice( 2, length );
        pool.Rent( 5 );

        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        var array = new int[arrayLength];
        Array.Fill( array, -1 );

        sut.CopyTo( array, arrayIndex );

        using ( new AssertionScope() )
        {
            array.Take( arrayIndex ).Should().AllBeEquivalentTo( -1 );
            array.Skip( arrayIndex ).Take( sut.Length ).Should().BeSequentiallyEqualTo( sut.ToArray() );
            array.Skip( arrayIndex + sut.Length ).Should().AllBeEquivalentTo( -1 );
        }
    }

    [Fact]
    public void CopyTo_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        seq.Dispose();

        var array = new int[4];

        sut.CopyTo( array, 0 );

        array.Should().AllBeEquivalentTo( 0 );
    }

    [Fact]
    public void CopyTo_ShouldDoNothing_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        pool.Rent( 8 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        seq.Dispose();

        var array = new int[4];

        sut.CopyTo( array, 0 );

        array.Should().AllBeEquivalentTo( 0 );
    }

    [Fact]
    public void CopyTo_ShouldDoNothing_WhenSequenceIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var seq = pool.Rent( 8 );
        seq.CopyFrom( Enumerable.Range( 1, 8 ).ToArray() );

        var sut = seq.Slice( 4, 0 );

        var other = pool.Rent( 8 );
        for ( var i = 0; i < other.Length; ++i )
            other[i] = -1;

        var array = new int[8];

        sut.CopyTo( array, 0 );

        array.Should().AllBeEquivalentTo( 0 );
    }

    [Theory]
    [InlineData( 1, 0 )]
    [InlineData( 2, 1 )]
    [InlineData( 2, 0 )]
    [InlineData( 10, 9 )]
    [InlineData( 10, 5 )]
    [InlineData( 10, 1 )]
    [InlineData( 10, 0 )]
    public void CopyTo_ShouldThrowArgumentOutOfRangeException_WhenTargetSpanIsTooShort(int length, int spanLength)
    {
        var pool = new MemorySequencePool<int>( 8 );
        var seq = pool.Rent( length + 4 );
        var sut = seq.Slice( 2, length );
        var array = new int[spanLength];

        ArgumentOutOfRangeException? exception = null;
        try
        {
            sut.CopyTo( array, 0 );
        }
        catch ( ArgumentOutOfRangeException e )
        {
            exception = e;
        }

        exception.Should().NotBeNull();
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 1, 2 )]
    [InlineData( 1, 3 )]
    [InlineData( 4, 4 )]
    [InlineData( 4, 5 )]
    [InlineData( 4, 7 )]
    [InlineData( 10, 10 )]
    [InlineData( 10, 15 )]
    [InlineData( 30, 30 )]
    [InlineData( 30, 40 )]
    public void CopyTo_SequenceSpan_ShouldCopyElementsCorrectly(int length, int spanLength)
    {
        var pool = new MemorySequencePool<int>( 8 );
        pool.Rent( 1 );
        var seq = pool.Rent( length + 4 );
        var sut = seq.Slice( 2, length );
        pool.Rent( 1 );
        var spanSeq = pool.Rent( spanLength + 4 );
        var span = spanSeq.Slice( 2, spanLength );
        pool.Rent( 6 );

        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        for ( var i = 0; i < spanSeq.Length; ++i )
            spanSeq[i] = -1;

        sut.CopyTo( span );

        using ( new AssertionScope() )
        {
            span.ToArray().Take( sut.Length ).Should().BeSequentiallyEqualTo( sut.ToArray() );
            span.ToArray().Skip( sut.Length ).Should().AllBeEquivalentTo( -1 );
            spanSeq.Take( 2 ).Concat( spanSeq.TakeLast( 2 ) ).Should().AllBeEquivalentTo( -1 );
        }
    }

    [Fact]
    public void CopyTo_SequenceSpan_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var otherSeq = pool.Rent( 12 );
        var other = otherSeq.Slice( 2, 8 );
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        seq.Dispose();

        sut.CopyTo( other );

        other.ToArray().Should().AllBeEquivalentTo( 0 );
    }

    [Fact]
    public void CopyTo_SequenceSpan_ShouldDoNothing_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        var otherSeq = pool.Rent( 12 );
        var other = otherSeq.Slice( 2, 8 );
        seq.Dispose();

        sut.CopyTo( other );

        other.ToArray().Should().AllBeEquivalentTo( 0 );
    }

    [Fact]
    public void CopyTo_SequenceSpan_ShouldDoNothing_WhenSequenceIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var seq = pool.Rent( 8 );
        seq.CopyFrom( Enumerable.Range( 1, 8 ).ToArray() );
        var sut = seq.Slice( 4, 0 );
        var other = pool.Rent( 8 );

        sut.CopyTo( other );

        other.Should().AllBeEquivalentTo( 0 );
    }

    [Theory]
    [InlineData( 1, 0 )]
    [InlineData( 2, 1 )]
    [InlineData( 2, 0 )]
    [InlineData( 10, 9 )]
    [InlineData( 10, 5 )]
    [InlineData( 10, 1 )]
    [InlineData( 10, 0 )]
    public void CopyTo_SequenceSpan_ShouldThrowArgumentOutOfRangeException_WhenTargetSpanIsTooShort(int length, int spanLength)
    {
        var pool = new MemorySequencePool<int>( 8 );
        var seq = pool.Rent( length + 4 );
        var sut = seq.Slice( 2, length );
        var otherSeq = pool.Rent( spanLength + 4 );
        var other = otherSeq.Slice( 2, spanLength );

        ArgumentOutOfRangeException? exception = null;
        try
        {
            sut.CopyTo( other );
        }
        catch ( ArgumentOutOfRangeException e )
        {
            exception = e;
        }

        exception.Should().NotBeNull();
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 2, 1 )]
    [InlineData( 3, 1 )]
    [InlineData( 4, 4 )]
    [InlineData( 5, 4 )]
    [InlineData( 7, 4 )]
    [InlineData( 10, 10 )]
    [InlineData( 15, 10 )]
    [InlineData( 30, 30 )]
    [InlineData( 40, 30 )]
    public void CopyFrom_ShouldCopySpanIntoSequenceCorrectly(int length, int spanLength)
    {
        var pool = new MemorySequencePool<int>( 8 );

        var first = pool.Rent( 1 );
        for ( var i = 0; i < first.Length; ++i )
            first[i] = -1;

        var seq = pool.Rent( length + 4 );
        var sut = seq.Slice( 2, length );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = -3;

        var second = pool.Rent( 5 );
        for ( var i = 0; i < second.Length; ++i )
            second[i] = -2;

        var array = new int[spanLength];
        for ( var i = 0; i < spanLength; ++i )
            array[i] = i + 1;

        sut.CopyFrom( array );

        using ( new AssertionScope() )
        {
            first.Should().AllBeEquivalentTo( -1 );
            second.Should().AllBeEquivalentTo( -2 );
            sut.ToArray().Take( spanLength ).Should().BeSequentiallyEqualTo( array );
            sut.ToArray().Skip( spanLength ).Should().AllBeEquivalentTo( -3 );
            seq.Take( 2 ).Concat( seq.TakeLast( 2 ) ).Should().AllBeEquivalentTo( -3 );
        }
    }

    [Fact]
    public void CopyFrom_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var span = Fixture.CreateMany<int>( count: 10 ).ToArray();
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var head = pool.Rent( 8 );
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        seq.Dispose();

        sut.CopyFrom( span );
        var tail = pool.Rent( 12 );

        using ( new AssertionScope() )
        {
            head.Should().AllBeEquivalentTo( 0 );
            tail.Should().AllBeEquivalentTo( 0 );
        }
    }

    [Fact]
    public void CopyFrom_ShouldDoNothing_WhenNonTailSequenceIsDisposed()
    {
        var span = Fixture.CreateMany<int>( count: 10 ).ToArray();
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        var tail = pool.Rent( 8 );
        seq.Dispose();

        sut.CopyFrom( span );
        var head = pool.Rent( 12 );

        using ( new AssertionScope() )
        {
            head.Should().AllBeEquivalentTo( 0 );
            tail.Should().AllBeEquivalentTo( 0 );
        }
    }

    [Fact]
    public void CopyFrom_ShouldDoNothing_WhenSequenceIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var seq = pool.Rent( 8 );
        seq.CopyFrom( Enumerable.Range( 1, 8 ).ToArray() );
        var sut = seq.Slice( 4, 0 );
        var tail = pool.Rent( 8 );

        sut.CopyFrom( ReadOnlySpan<int>.Empty );

        using ( new AssertionScope() )
        {
            seq.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8 );
            tail.Should().AllBeEquivalentTo( 0 );
        }
    }

    [Theory]
    [InlineData( 0, 1 )]
    [InlineData( 1, 2 )]
    [InlineData( 2, 3 )]
    [InlineData( 2, 4 )]
    [InlineData( 10, 11 )]
    [InlineData( 10, 12 )]
    [InlineData( 10, 15 )]
    [InlineData( 10, 20 )]
    public void CopyFrom_ShouldThrowArgumentOutOfRangeException_WhenTargetSpanIsTooLong(int length, int spanLength)
    {
        var pool = new MemorySequencePool<int>( 8 );
        var seq = pool.Rent( length + 4 );
        var sut = seq.Slice( 2, length );
        var array = new int[spanLength];

        ArgumentOutOfRangeException? exception = null;
        try
        {
            sut.CopyFrom( array );
        }
        catch ( ArgumentOutOfRangeException e )
        {
            exception = e;
        }

        exception.Should().NotBeNull();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    [InlineData( 10 )]
    [InlineData( 20 )]
    public void ToArray_ShouldArrayWithCorrectLengthAndElements(int length)
    {
        var pool = new MemorySequencePool<int>( 8 );
        pool.Rent( 1 );
        var seq = pool.Rent( length + 4 );
        var sut = seq.Slice( 2, length );
        pool.Rent( 5 );
        var expected = new int[sut.Length];

        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        for ( var i = 0; i < sut.Length; ++i )
            expected[i] = sut[i];

        var result = sut.ToArray();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ToArray_ShouldReturnEmptyArray_WhenSpanIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var seq = pool.Rent( 8 );
        var sut = seq.Slice( 4, 0 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        var result = sut.ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ToArray_ShouldReturnEmptyArray_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        seq.Dispose();

        var result = sut.ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ToArray_ShouldReturnEmptyArray_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        pool.Rent( 8 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        seq.Dispose();

        var result = sut.ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Sort_ShouldSortSequenceCorrectly()
    {
        var pool = new MemorySequencePool<int>( 4 );
        var a = pool.Rent( 1 );
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        var b = pool.Rent( 3 );
        seq.CopyFrom( new[] { -1, -1, 10, 7, 5, 8, 7, 2, 3, 9, -1, -1 } );

        sut.Sort();

        using ( new AssertionScope() )
        {
            a.Should().AllBeEquivalentTo( 0 );
            b.Should().AllBeEquivalentTo( 0 );
            seq.Should().BeSequentiallyEqualTo( -1, -1, 2, 3, 5, 7, 7, 8, 9, 10, -1, -1 );
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    public void Sort_ShouldDoNothing_WhenLengthIsLessThanTwo(int length)
    {
        var pool = new MemorySequencePool<int>( 4 );
        var a = pool.Rent( 1 );
        var seq = pool.Rent( 8 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = seq.Length - i;

        var sut = seq.Slice( 2, length );
        var b = pool.Rent( 3 );

        sut.Sort();

        using ( new AssertionScope() )
        {
            a.Should().AllBeEquivalentTo( 0 );
            b.Should().AllBeEquivalentTo( 0 );
            seq.Should().BeSequentiallyEqualTo( 8, 7, 6, 5, 4, 3, 2, 1 );
        }
    }

    [Fact]
    public void Sort_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        pool.Rent( 8 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = seq.Length - i;

        seq.Dispose();

        sut.Sort();
        var other = pool.Rent( 12 );

        other.Should().BeSequentiallyEqualTo( 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 );
    }

    [Fact]
    public void Sort_ShouldDoNothing_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var seq = pool.Rent( 12 );
        var sut = seq.Slice( 2, 8 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = seq.Length - i;

        seq.Dispose();

        sut.Sort();
        var other = pool.Rent( 12 );

        other.Should().BeSequentiallyEqualTo( 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectCollection()
    {
        var pool = new MemorySequencePool<int>( 4 );
        var seq = pool.Rent( 80 );
        var sut = seq.Slice( 10, 65 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        var result = new List<int>();
        foreach ( var v in sut )
            result.Add( v );

        result.Should().BeSequentiallyEqualTo( sut.ToArray() );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmptyCollection_WhenSequenceIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 4 );
        var seq = pool.Rent( 8 );
        var sut = seq.Slice( 4, 0 );
        for ( var i = 0; i < seq.Length; ++i )
            seq[i] = i + 1;

        var result = new List<int>();
        foreach ( var v in sut )
            result.Add( v );

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmptyCollection_WhenSequenceIsDefaultEmpty()
    {
        var pool = new MemorySequencePool<int>( 4 );
        var sut = pool.Rent( 0 ).Slice( 0 );

        var result = new List<int>();
        foreach ( var v in sut )
            result.Add( v );

        result.Should().BeEmpty();
    }
}
