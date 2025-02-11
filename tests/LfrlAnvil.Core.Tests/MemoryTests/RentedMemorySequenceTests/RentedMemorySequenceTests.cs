using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Memory;

namespace LfrlAnvil.Tests.MemoryTests.RentedMemorySequenceTests;

public class RentedMemorySequenceTests : TestsBase
{
    [Fact]
    public void Default_ShouldHaveNoElementsAndSegments()
    {
        var sut = default( RentedMemorySequence<int> );

        Assertion.All(
                sut.Owner.TestNull(),
                sut.Length.TestEquals( 0 ),
                sut.Segments.ToArray().TestEmpty(),
                sut.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Empty_ShouldHaveNoElementsAndSegments()
    {
        var sut = RentedMemorySequence<int>.Empty;

        Assertion.All(
                sut.Owner.TestNull(),
                sut.Length.TestEquals( 0 ),
                sut.Segments.ToArray().TestEmpty(),
                sut.TestEmpty() )
            .Go();
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

        result.TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 ] ).Go();
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

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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

        result.TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 ] ).Go();
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

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void GetRef_ShouldReturnReferenceToCorrectElement()
    {
        var pool = new MemorySequencePool<int>( 8 );

        var sut = pool.Rent( 12 );
        sut.CopyFrom( Enumerable.Range( 1, 12 ).ToArray() );

        ref var result = ref sut.GetRef( 8 );
        result = 20;

        sut.TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 20, 10, 11, 12 ] ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 12 )]
    [InlineData( 13 )]
    public void GetRef_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfBounds(int index)
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 12 );

        var action = Lambda.Of( () => sut.GetRef( index ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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

        Assertion.All(
                result.Length.TestEquals( 4 ),
                result[0].TestSequence( [ 1, 2, 3, 4, 5 ] ),
                result[1].TestSequence( [ 6, 7, 8, 9, 10, 11, 12, 13 ] ),
                result[2].TestSequence( [ 14, 15, 16, 17, 18, 19, 20, 21 ] ),
                result[3].TestSequence( [ 22, 23, 24, 25, 26, 27, 28 ] ),
                result.ToArray().TestSequence( [ result[0], result[1], result[2], result[3] ] ),
                result.ToArray().SelectMany( s => s ).TestSequence( sut ),
                result.ToString().TestEquals( "RentedMemorySequenceSegmentCollection<Int32>[4]" ) )
            .Go();
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

        Assertion.All(
                result.StartIndex.TestEquals( startIndex ),
                result.Length.TestEquals( expectedLength ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 9 )]
    public void Slice_ShouldThrowArgumentOutOfRangeException_WhenStartIndexIsOutOfBounds(int startIndex)
    {
        var pool = new MemorySequencePool<int>( 4 );
        var sut = pool.Rent( 8 );

        var action = Lambda.Of( () => { _ = sut.Slice( startIndex ); } );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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

        Assertion.All(
                result.StartIndex.TestEquals( startIndex ),
                result.Length.TestEquals( length ) )
            .Go();
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

        var action = Lambda.Of( () => { _ = sut.Slice( startIndex, length ); } );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Push_ShouldExtendSequenceByOneAndSetLastItem_WhenAllocatedNodeIsGreedyEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.GreedyRent();

        sut.Push( 1 );

        Assertion.All(
                sut.TestSequence( [ 1 ] ),
                sut.Length.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void Push_ShouldExtendSequenceByOneAndSetLastItem_WhenAllocatedNodeIsNotEmptyTail()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );

        sut.Push( 1 );

        Assertion.All(
                sut.TestSequence( [ 0, 0, 0, 0, 0, 0, 0, 0, 1 ] ),
                sut.Length.TestEquals( 9 ) )
            .Go();
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

        Assertion.All(
                pool.Report.FragmentedElements.TestEquals( fragmentedLength - 1 ),
                pool.Report.GetRentedNodes().TestSequence( [ tail, sut ] ),
                sut.TestSequence( [ 0, 0, 0, 0, 0, 0, 0, 0, 1 ] ),
                sut.Length.TestEquals( 9 ) )
            .Go();
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

        Assertion.All(
                pool.Report.GetFragmentedNodeSizes().TestSequence( [ 8 ] ),
                pool.Report.GetRentedNodes().TestSequence( [ sut, other ] ),
                sut.TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9 ] ),
                sut.Length.TestEquals( 9 ),
                other.TestAll( (e, _) => e.TestEquals( 0 ) ) )
            .Go();
    }

    [Fact]
    public void Push_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        sut.Dispose();

        sut.Push( 1 );

        sut.Length.TestEquals( 0 ).Go();
    }

    [Fact]
    public void Push_ShouldDoNothing_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        pool.Rent( 8 );
        sut.Dispose();

        sut.Push( 1 );

        sut.Length.TestEquals( 0 ).Go();
    }

    [Fact]
    public void Push_ShouldDoNothing_WhenAllocatedNodeIsDefaultEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 0 );

        sut.Push( 1 );

        sut.Length.TestEquals( 0 ).Go();
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

        Assertion.All(
                sut.TestSequence( Enumerable.Repeat( 0, length ) ),
                sut.Length.TestEquals( length ) )
            .Go();
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

        Assertion.All(
                sut.TestSequence( Enumerable.Range( 1, 8 ).Concat( Enumerable.Repeat( 0, length ) ) ),
                sut.Length.TestEquals( 8 + length ) )
            .Go();
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

        Assertion.All(
                pool.Report.FragmentedElements.TestEquals( fragmentedLength - length ),
                pool.Report.GetRentedNodes().TestSequence( [ tail, sut ] ),
                sut.TestSequence( Enumerable.Range( 1, 8 ).Concat( Enumerable.Repeat( 0, length ) ) ),
                sut.Length.TestEquals( 8 + length ) )
            .Go();
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

        Assertion.All(
                pool.Report.GetFragmentedNodeSizes().TestSequence( [ 8 ] ),
                pool.Report.GetRentedNodes().TestSequence( [ sut, other ] ),
                sut.TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 0 ] ),
                sut.Length.TestEquals( 9 ),
                other.TestAll( (e, _) => e.TestEquals( -1 ) ) )
            .Go();
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

        Assertion.All(
                pool.Report.GetFragmentedNodeSizes().TestSequence( [ 9 ] ),
                pool.Report.GetRentedNodes().TestSequence( [ sut, other ] ),
                sut.TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 0, 0 ] ),
                sut.Length.TestEquals( 10 ),
                other.TestAll( (e, _) => e.TestEquals( -1 ) ) )
            .Go();
    }

    [Fact]
    public void Expand_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        sut.Dispose();

        sut.Expand( 1 );

        sut.Length.TestEquals( 0 ).Go();
    }

    [Fact]
    public void Expand_ShouldDoNothing_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        pool.Rent( 8 );
        sut.Dispose();

        sut.Expand( 1 );

        sut.Length.TestEquals( 0 ).Go();
    }

    [Fact]
    public void Expand_ShouldDoNothing_WhenAllocatedNodeIsDefaultEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 0 );

        sut.Expand( 1 );

        sut.Length.TestEquals( 0 ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Expand_ShouldDoNothing_WhenLengthIsLessThanOne(int length)
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );

        sut.Expand( length );

        sut.Length.TestEquals( 8 ).Go();
    }

    [Fact]
    public void Refresh_ShouldUpdateLength_WhenSequenceHasBeenModifiedFromAnotherInstance()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        var other = sut;
        other.Expand( 3 );

        sut.Refresh();

        sut.Length.TestEquals( 11 ).Go();
    }

    [Fact]
    public void Refresh_ShouldUpdateLengthToZero_WhenSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        sut.Dispose();

        sut.Refresh();

        sut.Length.TestEquals( 0 ).Go();
    }

    [Fact]
    public void Refresh_ShouldUpdateLengthToZero_WhenTailSequenceIsDisposedFromAnotherInstance()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        var other = sut;
        other.Dispose();

        sut.Refresh();

        sut.Length.TestEquals( 0 ).Go();
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

        sut.Length.TestEquals( 0 ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnTypeAndLengthInfo()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 16 );

        var result = sut.ToString();

        result.TestEquals( "RentedMemorySequence<Int32>[16]" ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        sut.Dispose();

        var result = sut.Contains( default );

        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenNonTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );
        pool.Rent( 8 );
        sut.Dispose();

        var result = sut.Contains( default );

        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenAllocatedNodeIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.GreedyRent();
        pool.Rent( 8 );

        var result = sut.Contains( default );

        result.TestFalse().Go();
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

        Assertion.All(
                first.TestAll( (e, _) => e.TestEquals( -1 ) ),
                third.TestAll( (e, _) => e.TestEquals( -2 ) ),
                sut.TestAll( (e, _) => e.TestEquals( default( int ) ) ) )
            .Go();
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenTailSequenceIsDisposed()
    {
        var pool = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };
        var sut = pool.Rent( 8 );
        sut.CopyFrom( Enumerable.Range( 1, 8 ).ToArray() );
        sut.Dispose();

        sut.Clear();

        pool.Rent( 8 ).TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8 ] ).Go();
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

        pool.Rent( 8 ).TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8 ] ).Go();
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

        Assertion.All(
                first.TestAll( (e, _) => e.TestEquals( -1 ) ),
                second.TestAll( (e, _) => e.TestEquals( -2 ) ) )
            .Go();
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

        Assertion.All(
                array.Take( arrayIndex ).TestAll( (e, _) => e.TestEquals( -1 ) ),
                array.Skip( arrayIndex ).Take( sut.Length ).TestSequence( sut ),
                array.Skip( arrayIndex + sut.Length ).TestAll( (e, _) => e.TestEquals( -1 ) ) )
            .Go();
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

        array.TestAll( (e, _) => e.TestEquals( 0 ) ).Go();
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

        array.TestAll( (e, _) => e.TestEquals( 0 ) ).Go();
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

        array.TestAll( (e, _) => e.TestEquals( 0 ) ).Go();
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

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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

        Assertion.All(
                span.Take( sut.Length ).TestSequence( sut ),
                span.Skip( sut.Length ).TestAll( (e, _) => e.TestEquals( -1 ) ) )
            .Go();
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

        other.TestAll( (e, _) => e.TestEquals( 0 ) ).Go();
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

        other.TestAll( (e, _) => e.TestEquals( 0 ) ).Go();
    }

    [Fact]
    public void CopyTo_SequenceSpan_ShouldDoNothing_WhenAllocatedNodeIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.GreedyRent();
        var other = pool.Rent( 8 );

        sut.CopyTo( other );

        other.TestAll( (e, _) => e.TestEquals( 0 ) ).Go();
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

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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

        Assertion.All(
                first.TestAll( (e, _) => e.TestEquals( -1 ) ),
                second.TestAll( (e, _) => e.TestEquals( -2 ) ),
                sut.Take( spanLength ).TestSequence( array ),
                sut.Skip( spanLength ).TestAll( (e, _) => e.TestEquals( -3 ) ) )
            .Go();
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

        Assertion.All(
                head.TestAll( (e, _) => e.TestEquals( 0 ) ),
                tail.TestAll( (e, _) => e.TestEquals( 0 ) ) )
            .Go();
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

        Assertion.All(
                head.TestAll( (e, _) => e.TestEquals( 0 ) ),
                tail.TestAll( (e, _) => e.TestEquals( 0 ) ) )
            .Go();
    }

    [Fact]
    public void CopyFrom_ShouldDoNothing_WhenAllocatedNodeIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.GreedyRent();
        var tail = pool.Rent( 8 );

        sut.CopyFrom( ReadOnlySpan<int>.Empty );

        tail.TestAll( (e, _) => e.TestEquals( 0 ) ).Go();
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

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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

        result.TestSequence( sut ).Go();
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

        result.TestEmpty().Go();
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

        result.TestEmpty().Go();
    }

    [Fact]
    public void ToArray_ShouldReturnEmptyArray_WhenAllocatedNodeIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.GreedyRent();
        pool.Rent( 8 );

        var result = sut.ToArray();

        result.TestEmpty().Go();
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

        Assertion.All(
                a.TestAll( (e, _) => e.TestEquals( 0 ) ),
                b.TestAll( (e, _) => e.TestEquals( 0 ) ),
                sut.TestSequence( [ 2, 3, 5, 7, 7, 8, 9, 10 ] ) )
            .Go();
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

        Assertion.All(
                a.TestAll( (e, _) => e.TestEquals( 0 ) ),
                b.TestAll( (e, _) => e.TestEquals( 0 ) ),
                sut.TestSequence( [ 1 ] ) )
            .Go();
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

        other.TestSequence( [ 8, 7, 6, 5, 4, 3, 2, 1 ] ).Go();
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

        other.TestSequence( [ 8, 7, 6, 5, 4, 3, 2, 1 ] ).Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectCollection()
    {
        var pool = new MemorySequencePool<int>( 4 );
        var sut = pool.Rent( 70 );
        for ( var i = 0; i < sut.Length; ++i )
            sut[i] = i + 1;

        var result = sut.Where( i => i > 0 );

        result.TestSequence( sut ).Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmptyCollection_WhenAllocatedNodeIsEmpty()
    {
        var pool = new MemorySequencePool<int>( 4 );
        var sut = pool.GreedyRent();

        var result = sut.Where( i => i >= 0 );

        result.TestEmpty().Go();
    }

    [Fact]
    public void SpanConversionOperator_ShouldReturnCorrectSpan()
    {
        var pool = new MemorySequencePool<int>( 4 );
        var sut = pool.Rent( 8 );

        var result = ( RentedMemorySequenceSpan<int> )sut;

        Assertion.All(
                result.StartIndex.TestEquals( 0 ),
                result.Length.TestEquals( sut.Length ) )
            .Go();
    }

    [Fact]
    public void ICollectionProperties_ShouldReturnCorrectResult()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 16 );

        Assertion.All(
                (( ICollection<int> )sut).IsReadOnly.TestFalse(),
                (( ICollection<int> )sut).Count.TestEquals( sut.Length ),
                (( IReadOnlyCollection<int> )sut).Count.TestEquals( sut.Length ) )
            .Go();
    }

    [Fact]
    public void ICollectionAdd_ShouldThrowNotSupportedException()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );

        var action = Lambda.Of( () => (( ICollection<int> )sut).Add( Fixture.Create<int>() ) );

        action.Test( exc => exc.TestType().Exact<NotSupportedException>() ).Go();
    }

    [Fact]
    public void ICollectionRemove_ShouldThrowNotSupportedException()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var sut = pool.Rent( 8 );

        var action = Lambda.Of( () => (( ICollection<int> )sut).Remove( Fixture.Create<int>() ) );

        action.Test( exc => exc.TestType().Exact<NotSupportedException>() ).Go();
    }
}
