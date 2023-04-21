using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Memory;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.MemoryTests.RentedMemorySequenceTests;

public class RentedMemorySequenceTests : TestsBase
{
    [Fact]
    public void Default_ShouldHaveNoElementsAndSegments()
    {
        var sut = default( RentedMemorySequence<int> );

        using ( new AssertionScope() )
        {
            sut.Owner.Should().BeNull();
            sut.Length.Should().Be( 0 );
            sut.Segments.ToArray().Should().BeEmpty();
            sut.Should().BeEmpty();
        }
    }

    [Fact]
    public void Empty_ShouldHaveNoElementsAndSegments()
    {
        var sut = RentedMemorySequence<int>.Empty;

        using ( new AssertionScope() )
        {
            sut.Owner.Should().BeNull();
            sut.Length.Should().Be( 0 );
            sut.Segments.ToArray().Should().BeEmpty();
            sut.Should().BeEmpty();
        }
    }

    [Fact]
    public void GetIndexer_ShouldReturnCorrectElement()
    {
        var pool = new MemorySequencePool<int>( 8 );

        var sut = pool.Rent( 12 );
        sut.CopyFrom( Enumerable.Range( 1, 12 ).ToArray() );

        var result = new int[12];
        for ( var i = 0; i < sut.Length; ++i )
            result[i] = sut[i];

        result.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 12 )]
    [InlineData( 13 )]
    public void GetIndexer_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfBounds(int index)
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 12 );

        var action = Lambda.Of( () => sut[index] );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetIndexer_ShouldUpdateCorrectElement()
    {
        var pool = new MemorySequencePool<int>( 8 );

        var sut = pool.Rent( 12 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var result = new int[12];
        sut.CopyTo( result );

        result.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 12 )]
    [InlineData( 13 )]
    public void SetIndexer_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfBounds(int index)
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 12 );

        var action = Lambda.Of( () => sut[index] = Fixture.Create<int>() );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Segments_ShouldReturnCorrectArraySegments()
    {
        var pool = new MemorySequencePool<int>( 8 );
        pool.Rent( 3 );
        var sut = pool.Rent( 28 );
        pool.Rent( 5 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var result = sut.Segments;

        using ( new AssertionScope() )
        {
            result.Length.Should().Be( 4 );
            result[0].Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5 );
            result[1].Should().BeSequentiallyEqualTo( 6, 7, 8, 9, 10, 11, 12, 13 );
            result[2].Should().BeSequentiallyEqualTo( 14, 15, 16, 17, 18, 19, 20, 21 );
            result[3].Should().BeSequentiallyEqualTo( 22, 23, 24, 25, 26, 27, 28 );
            result.ToArray().Should().BeSequentiallyEqualTo( result[0], result[1], result[2], result[3] );
            result.ToArray().SelectMany( s => s ).Should().BeSequentiallyEqualTo( sut );
            result.ToString().Should().Be( "RentedMemorySequenceSegmentCollection<Int32>[4]" );
        }
    }

    [Theory]
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
        var sut = pool.Rent( 8 );

        var result = sut.Slice( startIndex );

        using ( new AssertionScope() )
        {
            result.StartIndex.Should().Be( startIndex );
            result.Length.Should().Be( expectedLength );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 9 )]
    public void Slice_ShouldThrowArgumentOutOfRangeException_WhenStartIndexIsOutOfBounds(int startIndex)
    {
        var pool = new MemorySequencePool<int>( 4 );
        var sut = pool.Rent( 8 );

        var action = Lambda.Of(
            () =>
            {
                var _ = sut.Slice( startIndex );
            } );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
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
    public void Slice_WithLength_ShouldReturnCorrectSpan(int startIndex, int length)
    {
        var pool = new MemorySequencePool<int>( 4 );
        var sut = pool.Rent( 8 );

        var result = sut.Slice( startIndex, length );

        using ( new AssertionScope() )
        {
            result.StartIndex.Should().Be( startIndex );
            result.Length.Should().Be( length );
        }
    }

    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 0, -1 )]
    [InlineData( 0, 9 )]
    [InlineData( 1, -1 )]
    [InlineData( 1, 8 )]
    [InlineData( 2, -1 )]
    [InlineData( 2, 7 )]
    [InlineData( 3, -1 )]
    [InlineData( 3, 6 )]
    [InlineData( 4, -1 )]
    [InlineData( 4, 5 )]
    [InlineData( 5, -1 )]
    [InlineData( 5, 4 )]
    [InlineData( 6, -1 )]
    [InlineData( 6, 3 )]
    [InlineData( 7, -1 )]
    [InlineData( 7, 2 )]
    [InlineData( 8, -1 )]
    [InlineData( 8, 1 )]
    [InlineData( 9, 0 )]
    public void Slice_WithLength_ShouldThrowArgumentOutOfRangeException_WhenStartIndexOrLengthAreOutOfBounds(int startIndex, int length)
    {
        var pool = new MemorySequencePool<int>( 4 );
        var sut = pool.Rent( 8 );

        var action = Lambda.Of(
            () =>
            {
                var _ = sut.Slice( startIndex, length );
            } );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Push_ShouldExtendSequenceByOneAndSetLastItem_WhenAllocatedNodeIsGreedyEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.GreedyRent();

        sut.Push( 1 );

        using ( new AssertionScope() )
        {
            sut.Should().BeSequentiallyEqualTo( 1 );
            sut.Length.Should().Be( 1 );
        }
    }

    [Fact]
    public void Push_ShouldExtendSequenceByOneAndSetLastItem_WhenAllocatedNodeIsNotEmptyTail()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );

        sut.Push( 1 );

        using ( new AssertionScope() )
        {
            sut.Should().BeSequentiallyEqualTo( 0, 0, 0, 0, 0, 0, 0, 0, 1 );
            sut.Length.Should().Be( 9 );
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void Push_ShouldExtendSequenceByOneAndSetLastItem_WhenAllocatedNodeIsNotEmptyAndSucceededByLargeEnoughFragmentedNode(
        int fragmentedLength)
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        var f = pool.Rent( fragmentedLength );
        var tail = pool.Rent( 1 );
        f.Dispose();

        sut.Push( 1 );

        using ( new AssertionScope() )
        {
            pool.Report.FragmentedElements.Should().Be( fragmentedLength - 1 );
            pool.Report.GetRentedNodes().Should().BeSequentiallyEqualTo( tail, sut );
            sut.Should().BeSequentiallyEqualTo( 0, 0, 0, 0, 0, 0, 0, 0, 1 );
            sut.Length.Should().Be( 9 );
        }
    }

    [Fact]
    public void Push_ShouldReallocateSequence_WhenAllocatedNodeIsNotEmptyAndSucceededByActiveNode()
    {
        var pool = new MemorySequencePool<int>( 8 );

        var sut = pool.Rent( 8 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var other = pool.Rent( 1 );

        sut.Push( 9 );

        using ( new AssertionScope() )
        {
            pool.Report.GetFragmentedNodeSizes().Should().BeSequentiallyEqualTo( 8 );
            pool.Report.GetRentedNodes().Should().BeSequentiallyEqualTo( sut, other );
            sut.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9 );
            sut.Length.Should().Be( 9 );
            other.Should().AllBeEquivalentTo( 0 );
        }
    }

    [Fact]
    public void Push_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        sut.Dispose();

        sut.Push( 1 );

        sut.Length.Should().Be( 0 );
    }

    [Fact]
    public void Push_ShouldDoNothing_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        pool.Rent( 8 );
        sut.Dispose();

        sut.Push( 1 );

        sut.Length.Should().Be( 0 );
    }

    [Fact]
    public void Push_ShouldDoNothing_WhenAllocatedNodeIsDefaultEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 0 );

        sut.Push( 1 );

        sut.Length.Should().Be( 0 );
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void Expand_ShouldExtendSequence_WhenAllocatedNodeIsGreedyEmpty(int length)
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.GreedyRent();

        sut.Expand( length );

        using ( new AssertionScope() )
        {
            sut.Should().BeSequentiallyEqualTo( Enumerable.Repeat( 0, length ) );
            sut.Length.Should().Be( length );
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void Expand_ShouldExtendSequence_WhenAllocatedNodeIsNotEmptyTail(int length)
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        sut.Expand( length );

        using ( new AssertionScope() )
        {
            sut.Should().BeSequentiallyEqualTo( Enumerable.Range( 1, 8 ).Concat( Enumerable.Repeat( 0, length ) ) );
            sut.Length.Should().Be( 8 + length );
        }
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 1, 2 )]
    [InlineData( 2, 2 )]
    [InlineData( 1, 3 )]
    [InlineData( 2, 3 )]
    [InlineData( 3, 3 )]
    public void Expand_ShouldExtendSequence_WhenAllocatedNodeIsNotEmptyAndSucceededByLargeEnoughFragmentedNode(
        int length,
        int fragmentedLength)
    {
        var pool = new MemorySequencePool<int>( 8 );

        var sut = pool.Rent( 8 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var f = pool.Rent( fragmentedLength );
        var tail = pool.Rent( 1 );
        f.Dispose();

        sut.Expand( length );

        using ( new AssertionScope() )
        {
            pool.Report.FragmentedElements.Should().Be( fragmentedLength - length );
            pool.Report.GetRentedNodes().Should().BeSequentiallyEqualTo( tail, sut );
            sut.Should().BeSequentiallyEqualTo( Enumerable.Range( 1, 8 ).Concat( Enumerable.Repeat( 0, length ) ) );
            sut.Length.Should().Be( 8 + length );
        }
    }

    [Fact]
    public void Expand_ShouldReallocateSequence_WhenAllocatedNodeIsNotEmptyAndSucceededByActiveNode()
    {
        var pool = new MemorySequencePool<int>( 8 );

        var sut = pool.Rent( 8 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var other = pool.Rent( 1 );
        other[0] = -1;

        sut.Expand( 1 );

        using ( new AssertionScope() )
        {
            pool.Report.GetFragmentedNodeSizes().Should().BeSequentiallyEqualTo( 8 );
            pool.Report.GetRentedNodes().Should().BeSequentiallyEqualTo( sut, other );
            sut.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 0 );
            sut.Length.Should().Be( 9 );
            other.Should().AllBeEquivalentTo( -1 );
        }
    }

    [Fact]
    public void Expand_ShouldReallocateSequence_WhenAllocatedNodeIsNotEmptyAndSucceededByTooSmallFragmentedNode()
    {
        var pool = new MemorySequencePool<int>( 8 );

        var sut = pool.Rent( 8 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var f = pool.Rent( 1 );
        var other = pool.Rent( 1 );
        other[0] = -1;

        f.Dispose();
        sut.Expand( 2 );

        using ( new AssertionScope() )
        {
            pool.Report.GetFragmentedNodeSizes().Should().BeSequentiallyEqualTo( 9 );
            pool.Report.GetRentedNodes().Should().BeSequentiallyEqualTo( sut, other );
            sut.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 0, 0 );
            sut.Length.Should().Be( 10 );
            other.Should().AllBeEquivalentTo( -1 );
        }
    }

    [Fact]
    public void Expand_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        sut.Dispose();

        sut.Expand( 1 );

        sut.Length.Should().Be( 0 );
    }

    [Fact]
    public void Expand_ShouldDoNothing_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        pool.Rent( 8 );
        sut.Dispose();

        sut.Expand( 1 );

        sut.Length.Should().Be( 0 );
    }

    [Fact]
    public void Expand_ShouldDoNothing_WhenAllocatedNodeIsDefaultEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 0 );

        sut.Expand( 1 );

        sut.Length.Should().Be( 0 );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Expand_ShouldDoNothing_WhenLengthIsLessThanOne(int length)
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );

        sut.Expand( length );

        sut.Length.Should().Be( 8 );
    }

    [Fact]
    public void Refresh_ShouldUpdateLength_WhenSequenceHasBeenModifiedFromAnotherInstance()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        var other = sut;
        other.Expand( 3 );

        sut.Refresh();

        sut.Length.Should().Be( 11 );
    }

    [Fact]
    public void Refresh_ShouldUpdateLengthToZero_WhenSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        sut.Dispose();

        sut.Refresh();

        sut.Length.Should().Be( 0 );
    }

    [Fact]
    public void Refresh_ShouldUpdateLengthToZero_WhenTailSequenceIsDisposedFromAnotherInstance()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        var other = sut;
        other.Dispose();

        sut.Refresh();

        sut.Length.Should().Be( 0 );
    }

    [Fact]
    public void Refresh_ShouldUpdateLengthToZero_WhenNonTailSequenceIsDisposedFromAnotherInstance()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        var other = sut;
        pool.Rent( 8 );
        other.Dispose();

        sut.Refresh();

        sut.Length.Should().Be( 0 );
    }

    [Fact]
    public void ToString_ShouldReturnTypeAndLengthInfo()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 16 );

        var result = sut.ToString();

        result.Should().Be( "RentedMemorySequence<Int32>[16]" );
    }

    [Theory]
    [InlineData( 0, false )]
    [InlineData( 1, true )]
    [InlineData( 2, true )]
    [InlineData( 3, true )]
    [InlineData( 4, true )]
    [InlineData( 5, false )]
    public void Contains_ShouldReturnCorrectResult_WhenSequenceIsContainedInOneSegment(int value, bool expected)
    {
        var pool = new MemorySequencePool<int>( 8 );
        pool.Rent( 3 );
        var sut = pool.Rent( 4 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var result = sut.Contains( value );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, false )]
    [InlineData( 1, true )]
    [InlineData( 2, true )]
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
    [InlineData( 19, false )]
    public void Contains_ShouldReturnCorrectResult_WhenSequenceIsSpreadOutAcrossMultipleSegments(int value, bool expected)
    {
        var pool = new MemorySequencePool<int>( 8 );
        pool.Rent( 3 );
        var sut = pool.Rent( 18 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var result = sut.Contains( value );

        result.Should().Be( expected );
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        sut.Dispose();

        var result = sut.Contains( default );

        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        pool.Rent( 8 );
        sut.Dispose();

        var result = sut.Contains( default );

        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenAllocatedNodeIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.GreedyRent();
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

        var first = pool.Rent( 3 );
        for ( var i = 0; i < first.Length; ++i )
            first[i] = -1;

        var sut = pool.Rent( length );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var third = pool.Rent( 5 );
        for ( var i = 0; i < third.Length; ++i )
            third[i] = -2;

        sut.Clear();

        using ( new AssertionScope() )
        {
            first.Should().AllBeEquivalentTo( -1 );
            third.Should().AllBeEquivalentTo( -2 );
            sut.Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var sut = pool.Rent( 8 );
        sut.CopyFrom( Enumerable.Range( 1, 8 ).ToArray() );
        sut.Dispose();

        sut.Clear();

        pool.Rent( 8 ).Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8 );
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var sut = pool.Rent( 8 );
        pool.Rent( 8 );
        sut.CopyFrom( Enumerable.Range( 1, 8 ).ToArray() );
        sut.Dispose();

        sut.Clear();

        pool.Rent( 8 ).Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8 );
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenAllocatedNodeIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );

        var first = pool.Rent( 3 );
        for ( var i = 0; i < first.Length; ++i )
            first[i] = -1;

        var sut = pool.GreedyRent();

        var second = pool.Rent( 3 );
        for ( var i = 0; i < second.Length; ++i )
            second[i] = -2;

        sut.Clear();

        using ( new AssertionScope() )
        {
            first.Should().AllBeEquivalentTo( -1 );
            second.Should().AllBeEquivalentTo( -2 );
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
        pool.Rent( 3 );
        var sut = pool.Rent( length );
        pool.Rent( 5 );

        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var array = new int[arrayLength];
        Array.Fill( array, -1 );

        sut.CopyTo( array, arrayIndex );

        using ( new AssertionScope() )
        {
            array.Take( arrayIndex ).Should().AllBeEquivalentTo( -1 );
            array.Skip( arrayIndex ).Take( sut.Length ).Should().BeSequentiallyEqualTo( sut );
            array.Skip( arrayIndex + sut.Length ).Should().AllBeEquivalentTo( -1 );
        }
    }

    [Fact]
    public void CopyTo_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var sut = pool.Rent( 8 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        sut.Dispose();

        var array = new int[4];

        sut.CopyTo( array, 0 );

        array.Should().AllBeEquivalentTo( 0 );
    }

    [Fact]
    public void CopyTo_ShouldDoNothing_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var sut = pool.Rent( 8 );
        pool.Rent( 8 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        sut.Dispose();

        var array = new int[4];

        sut.CopyTo( array, 0 );

        array.Should().AllBeEquivalentTo( 0 );
    }

    [Fact]
    public void CopyTo_ShouldDoNothing_WhenAllocatedNodeIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.GreedyRent();

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
        var sut = pool.Rent( length );
        var array = new int[spanLength];

        var action = Lambda.Of( () => sut.CopyTo( array, 0 ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
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
        pool.Rent( 3 );
        var sut = pool.Rent( length );
        pool.Rent( 5 );
        var span = pool.Rent( spanLength );
        pool.Rent( 6 );

        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        for ( var i = 0; i < span.Length; ++i )
            span[i] = -1;

        sut.CopyTo( span );

        using ( new AssertionScope() )
        {
            span.Take( sut.Length ).Should().BeSequentiallyEqualTo( sut );
            span.Skip( sut.Length ).Should().AllBeEquivalentTo( -1 );
        }
    }

    [Fact]
    public void CopyTo_SequenceSpan_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var other = pool.Rent( 8 );
        var sut = pool.Rent( 8 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        sut.Dispose();

        sut.CopyTo( other );

        other.Should().AllBeEquivalentTo( 0 );
    }

    [Fact]
    public void CopyTo_SequenceSpan_ShouldDoNothing_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var sut = pool.Rent( 8 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var other = pool.Rent( 8 );
        sut.Dispose();

        sut.CopyTo( other );

        other.Should().AllBeEquivalentTo( 0 );
    }

    [Fact]
    public void CopyTo_SequenceSpan_ShouldDoNothing_WhenAllocatedNodeIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.GreedyRent();
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
        var sut = pool.Rent( length );
        var other = pool.Rent( spanLength );

        var action = Lambda.Of( () => sut.CopyTo( other ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
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

        var first = pool.Rent( 3 );
        for ( var i = 0; i < first.Length; ++i )
            first[i] = -1;

        var sut = pool.Rent( length );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = -3;

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
            sut.Take( spanLength ).Should().BeSequentiallyEqualTo( array );
            sut.Skip( spanLength ).Should().AllBeEquivalentTo( -3 );
        }
    }

    [Fact]
    public void CopyFrom_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var span = Fixture.CreateMany<int>( count: 10 ).ToArray();
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var head = pool.Rent( 8 );
        var sut = pool.Rent( 8 );
        sut.Dispose();

        sut.CopyFrom( span );
        var tail = pool.Rent( 8 );

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
        var sut = pool.Rent( 8 );
        var tail = pool.Rent( 8 );
        sut.Dispose();

        sut.CopyFrom( span );
        var head = pool.Rent( 8 );

        using ( new AssertionScope() )
        {
            head.Should().AllBeEquivalentTo( 0 );
            tail.Should().AllBeEquivalentTo( 0 );
        }
    }

    [Fact]
    public void CopyFrom_ShouldDoNothing_WhenAllocatedNodeIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.GreedyRent();
        var tail = pool.Rent( 8 );

        sut.CopyFrom( ReadOnlySpan<int>.Empty );

        tail.Should().AllBeEquivalentTo( 0 );
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
        var sut = pool.GreedyRent( length );
        var array = new int[spanLength];

        var action = Lambda.Of( () => sut.CopyFrom( array ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
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
        pool.Rent( 3 );
        var sut = pool.Rent( length );
        pool.Rent( 5 );

        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var result = sut.ToArray();

        result.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ToArray_ShouldReturnEmptyArray_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var sut = pool.Rent( 8 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        sut.Dispose();

        var result = sut.ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ToArray_ShouldReturnEmptyArray_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var sut = pool.Rent( 8 );
        pool.Rent( 8 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        sut.Dispose();

        var result = sut.ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ToArray_ShouldReturnEmptyArray_WhenAllocatedNodeIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.GreedyRent();
        pool.Rent( 8 );

        var result = sut.ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Sort_ShouldSortSequenceCorrectly()
    {
        var pool = new MemorySequencePool<int>( 4 );
        var a = pool.Rent( 3 );
        var sut = pool.Rent( 8 );
        var b = pool.Rent( 5 );
        sut.CopyFrom( new[] { 10, 7, 5, 8, 7, 2, 3, 9 } );

        sut.Sort();

        using ( new AssertionScope() )
        {
            a.Should().AllBeEquivalentTo( 0 );
            b.Should().AllBeEquivalentTo( 0 );
            sut.Should().BeSequentiallyEqualTo( 2, 3, 5, 7, 7, 8, 9, 10 );
        }
    }

    [Fact]
    public void Sort_ShouldDoNothing_WhenLengthIsLessThanTwo()
    {
        var pool = new MemorySequencePool<int>( 4 );
        var a = pool.Rent( 3 );
        var sut = pool.Rent( 1 );
        sut[0] = 1;
        var b = pool.Rent( 5 );

        sut.Sort();

        using ( new AssertionScope() )
        {
            a.Should().AllBeEquivalentTo( 0 );
            b.Should().AllBeEquivalentTo( 0 );
            sut.Should().BeSequentiallyEqualTo( 1 );
        }
    }

    [Fact]
    public void Sort_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var sut = pool.Rent( 8 );
        pool.Rent( 8 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = sut.Length - i;

        sut.Dispose();

        sut.Sort();
        var other = pool.Rent( 8 );

        other.Should().BeSequentiallyEqualTo( 8, 7, 6, 5, 4, 3, 2, 1 );
    }

    [Fact]
    public void Sort_ShouldDoNothing_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var sut = pool.Rent( 8 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = sut.Length - i;

        sut.Dispose();

        sut.Sort();
        var other = pool.Rent( 8 );

        other.Should().BeSequentiallyEqualTo( 8, 7, 6, 5, 4, 3, 2, 1 );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectCollection()
    {
        var pool = new MemorySequencePool<int>( 4 );
        var sut = pool.Rent( 70 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var result = sut.Where( i => i > 0 );

        result.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmptyCollection_WhenAllocatedNodeIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 4 );
        var sut = pool.GreedyRent();

        var result = sut.Where( i => i >= 0 );

        result.Should().BeEmpty();
    }

    [Fact]
    public void SpanConversionOperator_ShouldReturnCorrectSpan()
    {
        var pool = new MemorySequencePool<int>( 4 );
        var sut = pool.Rent( 8 );

        var result = (RentedMemorySequenceSpan<int>)sut;

        using ( new AssertionScope() )
        {
            result.StartIndex.Should().Be( 0 );
            result.Length.Should().Be( sut.Length );
        }
    }

    [Fact]
    public void ICollectionProperties_ShouldReturnCorrectResult()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 16 );

        using ( new AssertionScope() )
        {
            ((ICollection<int>)sut).IsReadOnly.Should().BeFalse();
            ((ICollection<int>)sut).Count.Should().Be( sut.Length );
            ((IReadOnlyCollection<int>)sut).Count.Should().Be( sut.Length );
        }
    }

    [Fact]
    public void ICollectionAdd_ShouldThrowNotSupportedException()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );

        var action = Lambda.Of( () => ((ICollection<int>)sut).Add( Fixture.Create<int>() ) );

        action.Should().ThrowExactly<NotSupportedException>();
    }

    [Fact]
    public void ICollectionRemove_ShouldThrowNotSupportedException()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );

        var action = Lambda.Of( () => ((ICollection<int>)sut).Remove( Fixture.Create<int>() ) );

        action.Should().ThrowExactly<NotSupportedException>();
    }
}
