using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Internal;
using LfrlAnvil.Memory;

namespace LfrlAnvil.Tests.MemoryTests.MemoryPoolTests;

public class MemoryPoolTokenTests : TestsBase
{
    [Fact]
    public void Empty_ShouldHaveCorrectProperties()
    {
        var sut = MemoryPoolToken<int>.Empty;
        Assertion.All(
                sut.Clear.TestFalse(),
                sut.Owner.TestNull(),
                sut.ToString().TestEquals( "Length: 0" ) )
            .Go();
    }

    [Fact]
    public void Active_ShouldHaveCorrectProperties()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );

        Assertion.All(
                sut.Clear.TestFalse(),
                sut.Owner.TestRefEquals( pool ),
                sut.ToString().TestEquals( "Length: 5" ) )
            .Go();
    }

    [Fact]
    public void Active_ShouldHaveCorrectProperties_WithClear()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 ).EnableClearing();

        Assertion.All(
                sut.Clear.TestTrue(),
                sut.Owner.TestRefEquals( pool ),
                sut.ToString().TestEquals( "Length: 5 (clear enabled)" ) )
            .Go();
    }

    [Fact]
    public void Fragmented_ShouldHaveCorrectProperties()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        sut.Dispose();

        Assertion.All(
                sut.Clear.TestFalse(),
                sut.Owner.TestRefEquals( pool ),
                sut.ToString().TestEquals( "Length: 0" ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableClearing_ShouldUpdateClearProperty(bool value)
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 3 );

        sut = sut.EnableClearing( value );

        sut.Clear.TestEquals( value ).Go();
    }

    [Fact]
    public void AsMemory_ShouldReturnEmptyForEmpty()
    {
        var sut = MemoryPoolToken<int>.Empty;
        var result = sut.AsMemory();
        result.Length.TestEquals( 0 ).Go();
    }

    [Fact]
    public void AsMemory_ShouldReturnCorrectForActive()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );

        var result = sut.AsMemory();

        result.Length.TestEquals( 5 ).Go();
    }

    [Fact]
    public void AsMemory_ShouldReturnEmptyForFragmented()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        sut.Dispose();

        var result = sut.AsMemory();

        result.Length.TestEquals( 0 ).Go();
    }

    [Fact]
    public void AsSpan_ShouldReturnEmptyForEmpty()
    {
        var sut = MemoryPoolToken<int>.Empty;
        var result = sut.AsSpan();
        result.Length.TestEquals( 0 ).Go();
    }

    [Fact]
    public void AsSpan_ShouldReturnCorrectForActive()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );

        var result = sut.AsSpan();

        result.Length.TestEquals( 5 ).Go();
    }

    [Fact]
    public void AsSpan_ShouldReturnEmptyForFragmented()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        sut.Dispose();

        var result = sut.AsSpan();

        result.Length.TestEquals( 0 ).Go();
    }

    [Fact]
    public void SetLength_ShouldDoNothingForEmpty()
    {
        var sut = MemoryPoolToken<int>.Empty;
        sut.SetLength( 5 );
        sut.AsMemory().Length.TestEquals( 0 ).Go();
    }

    [Fact]
    public void SetLength_ShouldDoNothingForFragmented()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        sut.Dispose();

        sut.SetLength( 2 );

        sut.AsMemory().Length.TestEquals( 0 ).Go();
    }

    [Fact]
    public void SetLength_ShouldDoNothingForActive_WhenLengthDoesNotChange()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );
        new[] { 1, 2, 3, 4, 5 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 5 );

        sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5 ] ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void SetLength_ShouldThrowArgumentOutOfRangeException_WhenNewLengthIsLessThanOne(int length)
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );

        var action = Lambda.Of( () => sut.SetLength( length ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_ForTailNode()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 3 );

        sut.AsSpan().TestSequence( [ 1, 2, 3 ] ).Go();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_ForNodeFollowedByActiveNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 4 );
        new[] { 1, 2, 3, 4 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 3 );

        sut.SetLength( 2 );
        var other = pool.Rent( 2 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 1, 2 ] ),
                other.AsSpan().TestSequence( [ 3, 4 ] ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_ForNodeFollowedByFragmentedNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 4 );
        new[] { 1, 2, 3, 4 }.CopyTo( sut.AsSpan() );
        var other = pool.Rent( 4 );
        new[] { 5, 6, 7, 8 }.CopyTo( other.AsSpan() );
        _ = pool.Rent( 3 );
        other.Dispose();

        sut.SetLength( 3 );
        other = pool.Rent( 5 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 1, 2, 3 ] ),
                other.AsSpan().TestSequence( [ 4, 5, 6, 7, 8 ] ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_ForNodeFollowedByActiveNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 8 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 3 );

        sut.SetLength( 3 );
        var other = pool.Rent( 5 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 1, 2, 3 ] ),
                other.AsSpan().TestSequence( [ 4, 5, 6, 7, 8 ] ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_ForNodeFollowedByFragmentedNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 8 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );
        var other = pool.Rent( 3 );
        new[] { 9, 10, 11 }.CopyTo( other.AsSpan() );
        _ = pool.Rent( 2 );
        other.Dispose();

        sut.SetLength( 3 );
        other = pool.Rent( 5 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 1, 2, 3 ] ),
                other.AsSpan().TestSequence( [ 4, 5, 6, 7, 8 ] ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_WhenClearIsDisabled()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 8 ).EnableClearing( false );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 8 );

        sut.SetLength( 6 );
        var other = pool.Rent( 2 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6 ] ),
                other.AsSpan().TestSequence( [ 7, 8 ] ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_WhenClearIsEnabled()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 8 ).EnableClearing();
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 8 );

        sut.SetLength( 6 );
        var other = pool.Rent( 2 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6 ] ),
                other.AsSpan().TestSequence( [ 0, 0 ] ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_WithTrimStart_ForHeadNode()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 3, trimStart: true );
        var other = pool.Rent( 3 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 4, 5, 6 ] ),
                other.AsSpan().TestSequence( [ 1, 2, 3 ] ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_WithTrimStart_ForNodePrecededByActiveNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 3 );
        var sut = pool.Rent( 4 );
        new[] { 1, 2, 3, 4 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 2, trimStart: true );
        var other = pool.Rent( 2 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 3, 4 ] ),
                other.AsSpan().TestSequence( [ 1, 2 ] ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_WithTrimStart_ForNodePrecededByFragmentedNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        var other = pool.Rent( 4 );
        new[] { 1, 2, 3, 4 }.CopyTo( other.AsSpan() );
        var sut = pool.Rent( 4 );
        new[] { 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );
        other.Dispose();

        sut.SetLength( 3, trimStart: true );
        other = pool.Rent( 5 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 6, 7, 8 ] ),
                other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5 ] ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_WithTrimStart_ForNodePrecededByActiveNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 8 );
        var sut = pool.Rent( 8 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 3, trimStart: true );
        var other = pool.Rent( 5 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 6, 7, 8 ] ),
                other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5 ] ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_WithTrimStart_ForNodePrecededByFragmentedNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 4 );
        var other = pool.Rent( 4 );
        new[] { -1, -1, -1, -1 }.CopyTo( other.AsSpan() );
        var sut = pool.Rent( 8 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );
        other.Dispose();

        sut.SetLength( 3, trimStart: true );
        other = pool.Rent( 5 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 6, 7, 8 ] ),
                other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5 ] ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_WithTrimStart_WhenClearIsDisabled()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 8 ).EnableClearing( false );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 6, trimStart: true );
        var other = pool.Rent( 2 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 3, 4, 5, 6, 7, 8 ] ),
                other.AsSpan().TestSequence( [ 1, 2 ] ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_WithTrimStart_WhenClearIsEnabled()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 8 ).EnableClearing();
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 6, trimStart: true );
        var other = pool.Rent( 2 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 3, 4, 5, 6, 7, 8 ] ),
                other.AsSpan().TestSequence( [ 0, 0 ] ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForTailNode_ToFullSegmentCapacity()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 8 );

        sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 0, 0 ] ).Go();
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForTailNode_BelowSegmentCapacity()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 7 );

        sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 0 ] ).Go();
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForNodeFollowedByFragmentedNodeInTheSameSegment_ToFullFragmentationCapacity()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 4 );
        new[] { 1, 2, 3, 4 }.CopyTo( sut.AsSpan() );
        var other = pool.Rent( 4 );
        new[] { 5, 6, 7, 8 }.CopyTo( other.AsSpan() );
        using ( pool.Rent( 2 ) )
        {
            _ = pool.Rent( 3 );
            other.Dispose();
        }

        sut.SetLength( 8 );

        sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8 ] ).Go();
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForNodeFollowedByFragmentedNodeInTheSameSegment_BelowFragmentationCapacity()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 4 );
        new[] { 1, 2, 3, 4 }.CopyTo( sut.AsSpan() );
        var other = pool.Rent( 4 );
        new[] { 5, 6, 7, 8 }.CopyTo( other.AsSpan() );
        using ( pool.Rent( 2 ) )
        {
            _ = pool.Rent( 3 );
            other.Dispose();
        }

        sut.SetLength( 7 );

        sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7 ] ).Go();
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForTailNode_WhenSegmentLengthIsExceededAndLargeEnoughFragmentationExists()
    {
        var pool = new MemoryPool<int>( 8 );
        var other = pool.Rent( 8 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( other.AsSpan() );
        var f = pool.Rent( 2 );
        var sut = pool.Rent( 6 );
        new[] { 9, 10, 11, 12, 13, 14 }.CopyTo( sut.AsSpan() );
        f.Dispose();
        other.Dispose();

        sut.SetLength( 8 );

        sut.AsSpan().TestSequence( [ 9, 10, 11, 12, 13, 14, 7, 8 ] ).Go();
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForTailNode_WhenSegmentLengthIsExceededAndNewSegmentIsRequired()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 2 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 8 );

        sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 0, 0 ] ).Go();
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForTailNode_WhenSegmentLengthIsExceededAndNewLargeSegmentIsRequired()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 2 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 10 );

        sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 0, 0, 0, 0 ] ).Go();
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForNodeFollowedByFragmentedNodeInTheSameSegment_WhenFragmentationLengthIsExceeded()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 2 );
        var sut = pool.Rent( 5 );
        new[] { 1, 2, 3, 4, 5 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 3 );

        sut.SetLength( 7 );

        sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 0, 0 ] ).Go();
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForNodeFollowedByActiveNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 2 );

        sut.SetLength( 7 );

        sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 0 ] ).Go();
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForNodeFollowedByFragmentedNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 2 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );
        var other = pool.Rent( 7 );
        new[] { 7, 8, 9, 10, 11, 12, 13 }.CopyTo( other.AsSpan() );
        _ = pool.Rent( 1 );
        other.Dispose();

        sut.SetLength( 7 );

        sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 13 ] ).Go();
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForNodeFollowedByActiveNodeInDifferentSegment_WhenLargeEnoughFragmentationExists()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 1 );
        var other = pool.Rent( 7 );
        new[] { 7, 8, 9, 10, 11, 12, 13 }.CopyTo( other.AsSpan() );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 2 );
        _ = pool.Rent( 3 );
        other.Dispose();

        sut.SetLength( 7 );

        sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 13 ] ).Go();
    }

    [Fact]
    public void
        SetLength_ShouldIncreaseLengthCorrectly_ForNodeFollowedByActiveNodeInDifferentSegment_WhenSegmentLengthIsExceededAndNewSegmentIsRequired()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 2 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 4 );

        sut.SetLength( 7 );

        sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 0 ] ).Go();
    }

    [Fact]
    public void
        SetLength_ShouldIncreaseLengthCorrectly_ForNodeFollowedByActiveNodeInDifferentSegment_WhenSegmentLengthIsExceededAndNewLargeSegmentIsRequired()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 2 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 4 );

        sut.SetLength( 10 );

        sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 0, 0, 0, 0 ] ).Go();
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_WhenNodeIsNotFollowedByValidFragmentationNodeAndClearIsDisabled()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 2 );
        var sut = pool.Rent( 6 ).EnableClearing( false );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 8 );
        var other = pool.Rent( 6 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 0, 0 ] ),
                other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6 ] ) )
            .Go();
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_WhenNodeIsNotFollowedByValidFragmentationNodeAndClearIsEnabled()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 2 );
        var sut = pool.Rent( 6 ).EnableClearing();
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 8 );
        var other = pool.Rent( 6 );

        Assertion.All(
                sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 0, 0 ] ),
                other.AsSpan().TestSequence( [ 0, 0, 0, 0, 0, 0 ] ) )
            .Go();
    }

    [Fact]
    public void Split_ShouldReturnEmptyForEmpty()
    {
        var sut = MemoryPoolToken<int>.Empty;
        var result = sut.Split( 1 );
        result.TestEquals( MemoryPoolToken<int>.Empty ).Go();
    }

    [Fact]
    public void Split_ShouldReturnEmpty_WhenNodeIsInactive()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        sut.Dispose();

        var result = sut.Split( 1 );

        result.TestEquals( MemoryPoolToken<int>.Empty ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Split_ShouldReturnEmpty_WhenLengthIsLessThanOne(int length)
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );
        new[] { 1, 2, 3, 4, 5 }.CopyTo( sut.AsSpan() );

        var result = sut.Split( length );

        Assertion.All(
                result.TestEquals( MemoryPoolToken<int>.Empty ),
                sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5 ] ) )
            .Go();
    }

    [Theory]
    [InlineData( 5 )]
    [InlineData( 6 )]
    public void Split_ShouldReturnSelf_WhenLengthIsGreaterThanOrEqualToTokenLength(int length)
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );
        new[] { 1, 2, 3, 4, 5 }.CopyTo( sut.AsSpan() );

        var result = sut.Split( length );

        Assertion.All(
                result.TestEquals( sut ),
                sut.AsSpan().TestSequence( [ 1, 2, 3, 4, 5 ] ) )
            .Go();
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void Split_ShouldReturnFirstPart_ForHeadToken(bool clear)
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 ).EnableClearing( clear );
        new[] { 1, 2, 3, 4, 5 }.CopyTo( sut.AsSpan() );

        var result = sut.Split( 3 );

        Assertion.All(
                result.Clear.TestEquals( sut.Clear ),
                result.AsSpan().TestSequence( [ 1, 2, 3 ] ),
                sut.AsSpan().TestSequence( [ 4, 5 ] ) )
            .Go();
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void Split_ShouldReturnFirstPart_ForNonHeadToken(bool clear)
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 3 );
        var sut = pool.Rent( 5 ).EnableClearing( clear );
        new[] { 1, 2, 3, 4, 5 }.CopyTo( sut.AsSpan() );

        var result = sut.Split( 2 );

        Assertion.All(
                result.Clear.TestEquals( sut.Clear ),
                result.AsSpan().TestSequence( [ 1, 2 ] ),
                sut.AsSpan().TestSequence( [ 3, 4, 5 ] ) )
            .Go();
    }

    [Fact]
    public void TryGetInfo_ShouldReturnNullForEmpty()
    {
        var sut = MemoryPoolToken<int>.Empty;
        var result = sut.TryGetInfo();
        result.TestNull().Go();
    }

    [Fact]
    public void TryGetInfo_ShouldReturnNodeForActive()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );

        var result = sut.TryGetInfo();

        result.TestEquals(
                new MemoryPool<int>.ReportInfo.Node(
                    pool: pool,
                    segmentIndex: 0,
                    segmentLength: 8,
                    isSegmentActive: true,
                    version: 0,
                    nodeId: NullableIndex.Create( 0 ),
                    startIndex: 0,
                    length: 5,
                    isFragmented: false ) )
            .Go();
    }

    [Fact]
    public void TryGetInfo_ShouldReturnNullForFree()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );
        sut.Dispose();

        var result = sut.TryGetInfo();

        result.TestNull().Go();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool()
    {
        var pool = new MemoryPool<int>( 16 );
        var sut = pool.Rent( 16 );
        var memory = sut.AsMemory();

        sut.Dispose();

        using var next = pool.Rent( 16 );

        memory.TestEquals( next.AsMemory() ).Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenCalledSecondTime()
    {
        var pool = new MemoryPool<int>( 16 );
        var sut = pool.Rent( 16 );
        var memory = sut.AsMemory();

        sut.Dispose();
        sut.Dispose();

        using var next = pool.Rent( 16 );

        memory.TestEquals( next.AsMemory() ).Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenCalledSecondTimeFromAnotherInstance()
    {
        var pool = new MemoryPool<int>( 16 );
        var sut = pool.Rent( 16 );
        var other = sut;
        var memory = sut.AsMemory();

        sut.Dispose();
        other.Dispose();

        using var next = pool.Rent( 16 );

        memory.TestEquals( next.AsMemory() ).Go();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_ForTailNode_PrecededByFragmentedNodes()
    {
        var pool = new MemoryPool<int>( 16 );
        var head = pool.Rent( 16 );
        var other = pool.Rent( 16 );
        var sut = pool.Rent( 16 );
        head.Dispose();
        other.Dispose();

        sut.Dispose();

        pool.Report.ActiveSegments.TestEquals( 0 ).Go();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_ForNonTailHeadNode_FollowedByFragmentedNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        var sut = pool.Rent( 10 );
        _ = pool.Rent( 8 );

        sut.Dispose();
        var other = pool.Rent( 16 );

        (other.TryGetInfo()?.SegmentIndex).TestEquals( 0 ).Go();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_ForNonTailHeadNode_FollowedByActiveNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        var sut = pool.Rent( 10 );
        _ = pool.Rent( 6 );

        sut.Dispose();
        var other = pool.Rent( 10 );

        (other.TryGetInfo()?.SegmentIndex).TestEquals( 0 ).Go();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_ForNonTailHeadNode_FollowedByFragmentedNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        var sut = pool.Rent( 16 );
        using ( pool.Rent( 4 ) )
            _ = pool.Rent( 6 );

        sut.Dispose();
        var other = pool.Rent( 16 );

        (other.TryGetInfo()?.SegmentIndex).TestEquals( 0 ).Go();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_ForNonTailHeadNode_FollowedByActiveNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        var sut = pool.Rent( 16 );
        _ = pool.Rent( 6 );

        sut.Dispose();
        var other = pool.Rent( 16 );

        (other.TryGetInfo()?.SegmentIndex).TestEquals( 0 ).Go();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_ForNodePrecededByActiveNodeInTheSameSegmentAndFollowedByActiveNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        _ = pool.Rent( 4 );
        var sut = pool.Rent( 8 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 4 );

        sut.Dispose();
        var other = pool.Rent( 8 );

        other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8 ] ).Go();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_ForNodePrecededByFragmentedNodeInTheSameSegmentAndFollowedByActiveNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        var other = pool.Rent( 4 );
        new[] { 9, 10, 11, 12 }.CopyTo( other.AsSpan() );
        var sut = pool.Rent( 8 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 4 );
        other.Dispose();

        sut.Dispose();
        other = pool.Rent( 8 );

        other.AsSpan().TestSequence( [ 9, 10, 11, 12, 1, 2, 3, 4 ] ).Go();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_ForNodePrecededByActiveNodeInDifferentSegmentAndFollowedByActiveNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        _ = pool.Rent( 16 );
        var sut = pool.Rent( 8 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 4 );

        sut.Dispose();
        var other = pool.Rent( 8 );

        other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8 ] ).Go();
    }

    [Fact]
    public void
        Dispose_ShouldReturnTokenToThePool_ForNodePrecededByFragmentedNodeInDifferentSegmentAndFollowedByActiveNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        var other = pool.Rent( 16 );
        var sut = pool.Rent( 8 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 4 );
        other.Dispose();

        sut.Dispose();
        _ = pool.Rent( 16 );
        other = pool.Rent( 8 );

        other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8 ] ).Go();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_ForNodePrecededByActiveNodeInTheSameSegmentAndFollowedByFragmentedNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        _ = pool.Rent( 4 );
        var sut = pool.Rent( 8 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );
        var other = pool.Rent( 4 );
        new[] { 9, 10, 11, 12 }.CopyTo( other.AsSpan() );
        _ = pool.Rent( 4 );
        other.Dispose();

        sut.Dispose();
        other = pool.Rent( 12 );

        other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 ] ).Go();
    }

    [Fact]
    public void
        Dispose_ShouldReturnTokenToThePool_ForNodePrecededByFragmentedNodeInTheSameSegmentAndFollowedByFragmentedNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        var other = pool.Rent( 3 );
        new[] { 9, 10, 11 }.CopyTo( other.AsSpan() );
        var sut = pool.Rent( 8 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );
        var next = pool.Rent( 5 );
        new[] { 12, 13, 14, 15, 16 }.CopyTo( next.AsSpan() );
        using ( pool.Rent( 9 ) )
            _ = pool.Rent( 4 );

        other.Dispose();
        next.Dispose();

        sut.Dispose();
        other = pool.Rent( 16 );

        other.AsSpan().TestSequence( [ 9, 10, 11, 1, 2, 3, 4, 5, 6, 7, 8, 12, 13, 14, 15, 16 ] ).Go();
    }

    [Fact]
    public void
        Dispose_ShouldReturnTokenToThePool_ForNodePrecededByFragmentedNodeInTheSameSegmentAndFollowedByFragmentedNodeInTheSameSegment_WithFragmentationIndexChange()
    {
        var pool = new MemoryPool<int>( 256 );
        var _0 = pool.Rent( 6 );
        Enumerable.Range( 0, 6 ).ToArray().CopyTo( _0.AsSpan() );
        var _1 = pool.Rent( 7 );
        Enumerable.Range( 6, 7 ).ToArray().CopyTo( _1.AsSpan() );
        var _2 = pool.Rent( 8 );
        Enumerable.Range( 13, 8 ).ToArray().CopyTo( _2.AsSpan() );
        _0.SetLength( 1, true );
        var _4 = pool.Rent( 13 );
        Enumerable.Range( 21, 13 ).ToArray().CopyTo( _4.AsSpan() );
        _4.Dispose();
        _1.SetLength( 2, true );
        _2.SetLength( 3, true );
        var _5 = pool.Rent( 46 );
        Enumerable.Range( 34, 33 ).ToArray().CopyTo( _5.AsSpan().Slice( 13 ) );
        _0.Dispose();
        _0 = pool.Rent( 47 );
        Enumerable.Range( 67, 47 ).ToArray().CopyTo( _0.AsSpan() );
        _5.Dispose();
        _1.Dispose();
        _0.Dispose();
        _1 = pool.Rent( 48 );
        _2.Dispose();
        _1.Dispose();

        var check = pool.Rent( 114 );

        check.AsSpan().TestAll( (x, i) => x.TestEquals( i ) ).Go();
    }

    [Fact]
    public void
        Dispose_ShouldReturnTokenToThePool_ForNodePrecededByActiveNodeInDifferentSegmentAndFollowedByFragmentedNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        _ = pool.Rent( 16 );
        var sut = pool.Rent( 8 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );
        var other = pool.Rent( 8 );
        new[] { 9, 10, 11, 12, 13, 14, 15, 16 }.CopyTo( other.AsSpan() );
        _ = pool.Rent( 4 );
        other.Dispose();

        sut.Dispose();
        other = pool.Rent( 16 );

        other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 ] ).Go();
    }

    [Fact]
    public void
        Dispose_ShouldReturnTokenToThePool_ForNodePrecededByFragmentedNodeInDifferentSegmentAndFollowedByFragmentedNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        _ = pool.Rent( 6 );
        var other = pool.Rent( 10 );
        var sut = pool.Rent( 8 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );
        var next = pool.Rent( 8 );
        new[] { 9, 10, 11, 12, 13, 14, 15, 16 }.CopyTo( next.AsSpan() );
        _ = pool.Rent( 4 );
        other.Dispose();
        next.Dispose();

        sut.Dispose();
        other = pool.Rent( 16 );

        other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 ] ).Go();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_ForNodePrecededByActiveNodeInTheSameSegmentAndFollowedByActiveNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        _ = pool.Rent( 4 );
        var sut = pool.Rent( 12 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 4 );

        sut.Dispose();
        var other = pool.Rent( 12 );

        other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 ] ).Go();
    }

    [Fact]
    public void
        Dispose_ShouldReturnTokenToThePool_ForNodePrecededByFragmentedNodeInTheSameSegmentAndFollowedByActiveNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        var other = pool.Rent( 4 );
        new[] { 13, 14, 15, 16 }.CopyTo( other.AsSpan() );
        var sut = pool.Rent( 12 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 4 );
        other.Dispose();

        sut.Dispose();
        other = pool.Rent( 16 );

        other.AsSpan().TestSequence( [ 13, 14, 15, 16, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 ] ).Go();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_ForNodePrecededByActiveNodeInDifferentSegmentAndFollowedByActiveNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        _ = pool.Rent( 16 );
        var sut = pool.Rent( 16 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 4 );

        sut.Dispose();
        var other = pool.Rent( 16 );

        other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 ] ).Go();
    }

    [Fact]
    public void
        Dispose_ShouldReturnTokenToThePool_ForNodePrecededByFragmentedNodeInDifferentSegmentAndFollowedByActiveNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        _ = pool.Rent( 6 );
        var other = pool.Rent( 10 );
        var sut = pool.Rent( 16 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 4 );
        other.Dispose();

        sut.Dispose();
        other = pool.Rent( 16 );

        other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 ] ).Go();
    }

    [Fact]
    public void
        Dispose_ShouldReturnTokenToThePool_ForNodePrecededByActiveNodeInTheSameSegmentAndFollowedByFragmentedNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        _ = pool.Rent( 4 );
        var sut = pool.Rent( 12 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.CopyTo( sut.AsSpan() );
        var other = pool.Rent( 4 );
        _ = pool.Rent( 4 );
        other.Dispose();

        sut.Dispose();
        other = pool.Rent( 12 );

        other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 ] ).Go();
    }

    [Fact]
    public void
        Dispose_ShouldReturnTokenToThePool_ForNodePrecededByFragmentedNodeInTheSameSegmentAndFollowedByFragmentedNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        var other = pool.Rent( 4 );
        new[] { 13, 14, 15, 16 }.CopyTo( other.AsSpan() );
        var sut = pool.Rent( 12 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.CopyTo( sut.AsSpan() );
        var next = pool.Rent( 4 );
        _ = pool.Rent( 4 );
        other.Dispose();
        next.Dispose();

        sut.Dispose();
        other = pool.Rent( 16 );

        other.AsSpan().TestSequence( [ 13, 14, 15, 16, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 ] ).Go();
    }

    [Fact]
    public void
        Dispose_ShouldReturnTokenToThePool_ForNodePrecededByActiveNodeInDifferentSegmentAndFollowedByFragmentedNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        _ = pool.Rent( 16 );
        var sut = pool.Rent( 16 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }.CopyTo( sut.AsSpan() );
        var other = pool.Rent( 4 );
        _ = pool.Rent( 4 );
        other.Dispose();

        sut.Dispose();
        other = pool.Rent( 16 );

        other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 ] ).Go();
    }

    [Fact]
    public void
        Dispose_ShouldReturnTokenToThePool_ForNodePrecededByFragmentedNodeInDifferentSegmentAndFollowedByFragmentedNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        _ = pool.Rent( 6 );
        var other = pool.Rent( 10 );
        var sut = pool.Rent( 16 );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }.CopyTo( sut.AsSpan() );
        var next = pool.Rent( 4 );
        _ = pool.Rent( 4 );
        other.Dispose();
        next.Dispose();

        sut.Dispose();
        other = pool.Rent( 16 );

        other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 ] ).Go();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_WithClearDisabled()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 8 ).EnableClearing( false );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );

        sut.Dispose();
        var other = pool.Rent( 8 );

        other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8 ] ).Go();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_WithClearEnabled()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 8 ).EnableClearing();
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );

        sut.Dispose();
        var other = pool.Rent( 8 );

        other.AsSpan().TestSequence( [ 0, 0, 0, 0, 0, 0, 0, 0 ] ).Go();
    }
}
