// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using LfrlAnvil.Functional;
using LfrlAnvil.Internal;
using LfrlAnvil.Memory;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.MemoryTests.MemoryPoolTests;

public class MemoryPoolTokenTests : TestsBase
{
    [Fact]
    public void Empty_ShouldHaveCorrectProperties()
    {
        var sut = MemoryPoolToken<int>.Empty;
        using ( new AssertionScope() )
        {
            sut.Clear.Should().BeFalse();
            sut.Owner.Should().BeNull();
            sut.ToString().Should().Be( "Length: 0" );
        }
    }

    [Fact]
    public void Active_ShouldHaveCorrectProperties()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );

        using ( new AssertionScope() )
        {
            sut.Clear.Should().BeFalse();
            sut.Owner.Should().BeSameAs( pool );
            sut.ToString().Should().Be( "Length: 5" );
        }
    }

    [Fact]
    public void Active_ShouldHaveCorrectProperties_WithClear()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 ).EnableClearing();

        using ( new AssertionScope() )
        {
            sut.Clear.Should().BeTrue();
            sut.Owner.Should().BeSameAs( pool );
            sut.ToString().Should().Be( "Length: 5 (clear enabled)" );
        }
    }

    [Fact]
    public void Fragmented_ShouldHaveCorrectProperties()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.Clear.Should().BeFalse();
            sut.Owner.Should().BeSameAs( pool );
            sut.ToString().Should().Be( "Length: 0" );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableClearing_ShouldUpdateClearProperty(bool value)
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 3 );

        sut = sut.EnableClearing( value );

        sut.Clear.Should().Be( value );
    }

    [Fact]
    public void AsMemory_ShouldReturnEmptyForEmpty()
    {
        var sut = MemoryPoolToken<int>.Empty;
        var result = sut.AsMemory();
        result.Length.Should().Be( 0 );
    }

    [Fact]
    public void AsMemory_ShouldReturnCorrectForActive()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );

        var result = sut.AsMemory();

        result.Length.Should().Be( 5 );
    }

    [Fact]
    public void AsMemory_ShouldReturnEmptyForFragmented()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        sut.Dispose();

        var result = sut.AsMemory();

        result.Length.Should().Be( 0 );
    }

    [Fact]
    public void AsSpan_ShouldReturnEmptyForEmpty()
    {
        var sut = MemoryPoolToken<int>.Empty;
        var result = sut.AsSpan();
        result.Length.Should().Be( 0 );
    }

    [Fact]
    public void AsSpan_ShouldReturnCorrectForActive()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );

        var result = sut.AsSpan();

        result.Length.Should().Be( 5 );
    }

    [Fact]
    public void AsSpan_ShouldReturnEmptyForFragmented()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        sut.Dispose();

        var result = sut.AsSpan();

        result.Length.Should().Be( 0 );
    }

    [Fact]
    public void SetLength_ShouldDoNothingForEmpty()
    {
        var sut = MemoryPoolToken<int>.Empty;
        sut.SetLength( 5 );
        sut.AsMemory().Length.Should().Be( 0 );
    }

    [Fact]
    public void SetLength_ShouldDoNothingForFragmented()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        sut.Dispose();

        sut.SetLength( 2 );

        sut.AsMemory().Length.Should().Be( 0 );
    }

    [Fact]
    public void SetLength_ShouldDoNothingForActive_WhenLengthDoesNotChange()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );
        new[] { 1, 2, 3, 4, 5 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 5 );

        sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5 );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void SetLength_ShouldThrowArgumentOutOfRangeException_WhenNewLengthIsLessThanOne(int length)
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );

        var action = Lambda.Of( () => sut.SetLength( length ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetLength_ShouldReduceLengthCorrectly_ForTailNode()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 6 );

        sut.SetLength( 3 );

        sut.AsSpan().ToArray().Should().HaveCount( 3 );
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

        using ( new AssertionScope() )
        {
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2 );
            other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 3, 4 );
        }
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

        using ( new AssertionScope() )
        {
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3 );
            other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 4, 5, 6, 7, 8 );
        }
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

        using ( new AssertionScope() )
        {
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3 );
            other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 4, 5, 6, 7, 8 );
        }
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

        using ( new AssertionScope() )
        {
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3 );
            other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 4, 5, 6, 7, 8 );
        }
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

        using ( new AssertionScope() )
        {
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6 );
            other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 7, 8 );
        }
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

        using ( new AssertionScope() )
        {
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6 );
            other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 0, 0 );
        }
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForTailNode_ToFullSegmentCapacity()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 8 );

        sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 0, 0 );
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForTailNode_BelowSegmentCapacity()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 7 );

        sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 0 );
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

        sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8 );
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

        sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7 );
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

        sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 9, 10, 11, 12, 13, 14, 7, 8 );
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForTailNode_WhenSegmentLengthIsExceededAndNewSegmentIsRequired()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 2 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 8 );

        sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 0, 0 );
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForTailNode_WhenSegmentLengthIsExceededAndNewLargeSegmentIsRequired()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 2 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );

        sut.SetLength( 10 );

        sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 0, 0, 0, 0 );
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

        sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 0, 0 );
    }

    [Fact]
    public void SetLength_ShouldIncreaseLengthCorrectly_ForNodeFollowedByActiveNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 6 );
        new[] { 1, 2, 3, 4, 5, 6 }.CopyTo( sut.AsSpan() );
        _ = pool.Rent( 2 );

        sut.SetLength( 7 );

        sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 0 );
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

        sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 13 );
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

        sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 13 );
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

        sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 0 );
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

        sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 0, 0, 0, 0 );
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

        using ( new AssertionScope() )
        {
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 0, 0 );
            other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6 );
        }
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

        using ( new AssertionScope() )
        {
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 0, 0 );
            other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 0, 0, 0, 0, 0, 0 );
        }
    }

    [Fact]
    public void TryGetInfo_ShouldReturnNullForEmpty()
    {
        var sut = MemoryPoolToken<int>.Empty;
        var result = sut.TryGetInfo();
        result.Should().BeNull();
    }

    [Fact]
    public void TryGetInfo_ShouldReturnNodeForActive()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );

        var result = sut.TryGetInfo();

        result.Should()
            .BeEquivalentTo(
                new MemoryPool<int>.ReportInfo.Node(
                    pool: pool,
                    segmentIndex: 0,
                    segmentLength: 8,
                    isSegmentActive: true,
                    nodeId: NullableIndex.Create( 0 ),
                    startIndex: 0,
                    length: 5,
                    isFragmented: false ) );
    }

    [Fact]
    public void TryGetInfo_ShouldReturnNodeForFragmented()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        sut.Dispose();

        var result = sut.TryGetInfo();

        result.Should()
            .BeEquivalentTo(
                new MemoryPool<int>.ReportInfo.Node(
                    pool: pool,
                    segmentIndex: 0,
                    segmentLength: 8,
                    isSegmentActive: true,
                    nodeId: NullableIndex.Create( 0 ),
                    startIndex: 0,
                    length: 3,
                    isFragmented: true ) );
    }

    [Fact]
    public void TryGetInfo_ShouldReturnNullForFree()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 5 );
        sut.Dispose();

        var result = sut.TryGetInfo();

        result.Should().BeNull();
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool()
    {
        var pool = new MemoryPool<int>( 16 );
        var sut = pool.Rent( 16 );
        var memory = sut.AsMemory();

        sut.Dispose();

        using var next = pool.Rent( 16 );

        memory.Should().BeEquivalentTo( next.AsMemory() );
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

        memory.Should().BeEquivalentTo( next.AsMemory() );
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

        memory.Should().BeEquivalentTo( next.AsMemory() );
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

        pool.Report.ActiveSegments.Should().Be( 0 );
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_ForNonTailHeadNode_FollowedByFragmentedNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        var sut = pool.Rent( 10 );
        _ = pool.Rent( 8 );

        sut.Dispose();
        var other = pool.Rent( 16 );

        (other.TryGetInfo()?.SegmentIndex).Should().Be( 0 );
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_ForNonTailHeadNode_FollowedByActiveNodeInTheSameSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        var sut = pool.Rent( 10 );
        _ = pool.Rent( 6 );

        sut.Dispose();
        var other = pool.Rent( 10 );

        (other.TryGetInfo()?.SegmentIndex).Should().Be( 0 );
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

        (other.TryGetInfo()?.SegmentIndex).Should().Be( 0 );
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_ForNonTailHeadNode_FollowedByActiveNodeInDifferentSegment()
    {
        var pool = new MemoryPool<int>( 16 );
        var sut = pool.Rent( 16 );
        _ = pool.Rent( 6 );

        sut.Dispose();
        var other = pool.Rent( 16 );

        (other.TryGetInfo()?.SegmentIndex).Should().Be( 0 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 9, 10, 11, 12, 1, 2, 3, 4 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 9, 10, 11, 1, 2, 3, 4, 5, 6, 7, 8, 12, 13, 14, 15, 16 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 13, 14, 15, 16, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 13, 14, 15, 16, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 );
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

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 );
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_WithClearDisabled()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 8 ).EnableClearing( false );
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );

        sut.Dispose();
        var other = pool.Rent( 8 );

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8 );
    }

    [Fact]
    public void Dispose_ShouldReturnTokenToThePool_WithClearEnabled()
    {
        var pool = new MemoryPool<int>( 8 );
        var sut = pool.Rent( 8 ).EnableClearing();
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.CopyTo( sut.AsSpan() );

        sut.Dispose();
        var other = pool.Rent( 8 );

        other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 0, 0, 0, 0, 0, 0, 0, 0 );
    }
}
