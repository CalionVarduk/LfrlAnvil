using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Memory;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.MemoryTests.MemorySequencePoolTests;

[TestClass( typeof( MemorySequencePoolTestsData ) )]
public class MemorySequencePoolTests : TestsBase
{
    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( (1 << 30) + 1 )]
    [InlineData( (1 << 30) + 2 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMinSegmentLengthIsOutOfBounds(int minLength)
    {
        var action = Lambda.Of( () => new MemorySequencePool<int>( minLength ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 2, 2 )]
    [InlineData( 3, 4 )]
    [InlineData( 4, 4 )]
    [InlineData( 5, 8 )]
    [InlineData( 7, 8 )]
    [InlineData( 8, 8 )]
    [InlineData( 9, 16 )]
    [InlineData( 15, 16 )]
    [InlineData( 16, 16 )]
    [InlineData( 513, 1024 )]
    [InlineData( 1023, 1024 )]
    [InlineData( 1024, 1024 )]
    [InlineData( (1 << 30) - 1, 1 << 30 )]
    [InlineData( 1 << 30, 1 << 30 )]
    public void Ctor_ShouldComputeSegmentLengthByRoundingUpMinSegmentLengthToPowerOf2(int minLength, int expected)
    {
        var sut = new MemorySequencePool<int>( minLength );

        using ( new AssertionScope() )
        {
            sut.SegmentLength.Should().Be( expected );
            sut.ClearReturnedSequences.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Rent_ShouldReturnEmptySequence_WhenLengthIsLessThanOne(int length)
    {
        var sut = new MemorySequencePool<int>( 16 );
        using var result = sut.Rent( length );

        using ( new AssertionScope() )
        {
            result.Length.Should().Be( 0 );
            result.Segments.Length.Should().Be( 0 );
            result.Owner.Should().BeNull();
        }
    }

    [Theory]
    [MethodData( nameof( MemorySequencePoolTestsData.GetFirstSequenceData ) )]
    public void Rent_ShouldReturnCorrectFirstSequence(int segmentLength, int length, int[] expectedSegmentCountRange)
    {
        var sut = new MemorySequencePool<int>( segmentLength );
        using var result = sut.Rent( length );

        using ( new AssertionScope() )
        {
            result.Length.Should().Be( length );
            result.Segments.Length.Should().Be( expectedSegmentCountRange.Length );
            result.Segments.ToArray().Select( s => s.Count ).Should().BeSequentiallyEqualTo( expectedSegmentCountRange );
            result.Owner.Should().BeSameAs( sut );
        }
    }

    [Theory]
    [MethodData( nameof( MemorySequencePoolTestsData.GetSecondSequenceData ) )]
    public void Rent_ShouldReturnCorrectSecondSequence(int segmentLength, int firstLength, int length, int[] expectedSegmentCountRange)
    {
        var sut = new MemorySequencePool<int>( segmentLength );

        using var first = sut.Rent( firstLength );
        foreach ( var s in first.Segments )
            Array.Fill( s.Array!, 1, s.Offset, s.Count );

        using var result = sut.Rent( length );

        using ( new AssertionScope() )
        {
            result.Length.Should().Be( length );
            result.Segments.Length.Should().Be( expectedSegmentCountRange.Length );
            result.Segments.ToArray().Select( s => s.Count ).Should().BeSequentiallyEqualTo( expectedSegmentCountRange );
            result.Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Theory]
    [MethodData( nameof( MemorySequencePoolTestsData.GetReuseReturnedTailSequenceData ) )]
    public void Rent_ShouldCorrectlyReuseReturnedTailSequence(int segmentLength, int firstLength, int tailLength, int length)
    {
        var tailValue = 2;
        var sut = new MemorySequencePool<int>( segmentLength ) { ClearReturnedSequences = false };

        using var first = sut.Rent( firstLength );
        foreach ( var s in first.Segments )
            Array.Fill( s.Array!, 1, s.Offset, s.Count );

        using ( var tail = sut.Rent( tailLength ) )
        {
            foreach ( var s in tail.Segments )
                Array.Fill( s.Array!, tailValue, s.Offset, s.Count );
        }

        using var result = sut.Rent( length );

        using ( new AssertionScope() )
        {
            result.Length.Should().Be( length );
            result.Take( tailLength ).Should().AllBeEquivalentTo( tailValue );
            result.Skip( tailLength ).Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedFragmentedHeadSegments()
    {
        var sut = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };

        var first = sut.Rent( 16 );
        for ( var i = 0; i < first.Length; ++i )
            first[i] = i + 1;

        using var second = sut.Rent( 8 );
        foreach ( var s in second.Segments )
            Array.Fill( s.Array!, -1, s.Offset, s.Count );

        first.Dispose();

        using var third = sut.Rent( 10 );
        using var fourth = sut.Rent( 4 );
        using var fifth = sut.Rent( 2 );
        using var sixth = sut.Rent( 8 );

        using ( new AssertionScope() )
        {
            second.Should().AllBeEquivalentTo( -1 );
            third.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 );
            fourth.Should().BeSequentiallyEqualTo( 11, 12, 13, 14 );
            fifth.Should().BeSequentiallyEqualTo( 15, 16 );
            sixth.Length.Should().Be( 8 );
            sixth.Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedFragmentedMergedHeadSegments()
    {
        var sut = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };

        var first = sut.Rent( 8 );
        for ( var i = 0; i < first.Length; ++i )
            first[i] = i + 1;

        var second = sut.Rent( 8 );
        for ( var i = 0; i < second.Length; ++i )
            second[i] = i + first.Length + 1;

        using var third = sut.Rent( 16 );
        foreach ( var s in third.Segments )
            Array.Fill( s.Array!, -1, s.Offset, s.Count );

        second.Dispose();
        first.Dispose();

        using var fourth = sut.Rent( 10 );
        using var fifth = sut.Rent( 4 );
        using var sixth = sut.Rent( 2 );
        using var seventh = sut.Rent( 8 );

        using ( new AssertionScope() )
        {
            third.Should().AllBeEquivalentTo( -1 );
            fourth.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 );
            fifth.Should().BeSequentiallyEqualTo( 11, 12, 13, 14 );
            sixth.Should().BeSequentiallyEqualTo( 15, 16 );
            seventh.Length.Should().Be( 8 );
            seventh.Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedFragmentedIntermediateSegments()
    {
        var sut = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };

        using var first = sut.Rent( 8 );
        foreach ( var s in first.Segments )
            Array.Fill( s.Array!, -1, s.Offset, s.Count );

        var second = sut.Rent( 16 );
        for ( var i = 0; i < second.Length; ++i )
            second[i] = i + 1;

        using var third = sut.Rent( 8 );
        foreach ( var s in third.Segments )
            Array.Fill( s.Array!, -2, s.Offset, s.Count );

        second.Dispose();

        using var fourth = sut.Rent( 10 );
        using var fifth = sut.Rent( 4 );
        using var sixth = sut.Rent( 2 );
        using var seventh = sut.Rent( 8 );

        using ( new AssertionScope() )
        {
            first.Should().AllBeEquivalentTo( -1 );
            third.Should().AllBeEquivalentTo( -2 );
            fourth.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 );
            fifth.Should().BeSequentiallyEqualTo( 11, 12, 13, 14 );
            sixth.Should().BeSequentiallyEqualTo( 15, 16 );
            seventh.Length.Should().Be( 8 );
            seventh.Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedFragmentedIntermediateMergedWithPrevSegments()
    {
        var sut = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };

        var first = sut.Rent( 8 );
        for ( var i = 0; i < first.Length; ++i )
            first[i] = i + 1;

        var second = sut.Rent( 8 );
        for ( var i = 0; i < second.Length; ++i )
            second[i] = i + first.Length + 1;

        using var third = sut.Rent( 16 );
        foreach ( var s in third.Segments )
            Array.Fill( s.Array!, -1, s.Offset, s.Count );

        first.Dispose();
        second.Dispose();

        using var fourth = sut.Rent( 10 );
        using var fifth = sut.Rent( 4 );
        using var sixth = sut.Rent( 2 );
        using var seventh = sut.Rent( 8 );

        using ( new AssertionScope() )
        {
            third.Should().AllBeEquivalentTo( -1 );
            fourth.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 );
            fifth.Should().BeSequentiallyEqualTo( 11, 12, 13, 14 );
            sixth.Should().BeSequentiallyEqualTo( 15, 16 );
            seventh.Length.Should().Be( 8 );
            seventh.Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedFragmentedIntermediateMergedWithNextSegments()
    {
        var sut = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };

        var first = sut.Rent( 8 );
        for ( var i = 0; i < first.Length; ++i )
            first[i] = i + 1;

        var second = sut.Rent( 8 );
        for ( var i = 0; i < second.Length; ++i )
            second[i] = i + first.Length + 1;

        var third = sut.Rent( 8 );
        for ( var i = 0; i < third.Length; ++i )
            third[i] = i + first.Length + second.Length + 1;

        using var fourth = sut.Rent( 16 );
        foreach ( var s in fourth.Segments )
            Array.Fill( s.Array!, -1, s.Offset, s.Count );

        third.Dispose();
        second.Dispose();

        using var fifth = sut.Rent( 10 );
        using var sixth = sut.Rent( 4 );
        using var seventh = sut.Rent( 2 );
        using var eighth = sut.Rent( 8 );

        using ( new AssertionScope() )
        {
            fourth.Should().AllBeEquivalentTo( -1 );
            fifth.Should().BeSequentiallyEqualTo( 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 );
            sixth.Should().BeSequentiallyEqualTo( 19, 20, 21, 22 );
            seventh.Should().BeSequentiallyEqualTo( 23, 24 );
            eighth.Length.Should().Be( 8 );
            eighth.Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedFragmentedIntermediateMergedWithPrevAndNextSegments()
    {
        var sut = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };

        var first = sut.Rent( 8 );
        for ( var i = 0; i < first.Length; ++i )
            first[i] = i + 1;

        var second = sut.Rent( 8 );
        for ( var i = 0; i < second.Length; ++i )
            second[i] = i + first.Length + 1;

        var third = sut.Rent( 8 );
        for ( var i = 0; i < third.Length; ++i )
            third[i] = i + first.Length + second.Length + 1;

        using var fourth = sut.Rent( 16 );
        foreach ( var s in fourth.Segments )
            Array.Fill( s.Array!, -1, s.Offset, s.Count );

        first.Dispose();
        third.Dispose();
        second.Dispose();

        using var fifth = sut.Rent( 15 );
        using var sixth = sut.Rent( 6 );
        using var seventh = sut.Rent( 3 );
        using var eighth = sut.Rent( 8 );

        using ( new AssertionScope() )
        {
            fourth.Should().AllBeEquivalentTo( -1 );
            fifth.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 );
            sixth.Should().BeSequentiallyEqualTo( 16, 17, 18, 19, 20, 21 );
            seventh.Should().BeSequentiallyEqualTo( 22, 23, 24 );
            eighth.Length.Should().Be( 8 );
            eighth.Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedTailMergedWithHeadSegments()
    {
        var sut = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };

        var first = sut.Rent( 8 );
        for ( var i = 0; i < first.Length; ++i )
            first[i] = i + 1;

        var second = sut.Rent( 8 );
        for ( var i = 0; i < second.Length; ++i )
            second[i] = i + first.Length + 1;

        first.Dispose();
        second.Dispose();

        using var third = sut.Rent( 10 );
        using var fourth = sut.Rent( 4 );
        using var fifth = sut.Rent( 2 );
        using var sixth = sut.Rent( 8 );

        using ( new AssertionScope() )
        {
            third.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 );
            fourth.Should().BeSequentiallyEqualTo( 11, 12, 13, 14 );
            fifth.Should().BeSequentiallyEqualTo( 15, 16 );
            sixth.Length.Should().Be( 8 );
            sixth.Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedTailMergedWithNonHeadPrevSegments()
    {
        var sut = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };

        using var first = sut.Rent( 16 );
        foreach ( var s in first.Segments )
            Array.Fill( s.Array!, -1, s.Offset, s.Count );

        var second = sut.Rent( 8 );
        for ( var i = 0; i < second.Length; ++i )
            second[i] = i + 1;

        var third = sut.Rent( 8 );
        for ( var i = 0; i < third.Length; ++i )
            third[i] = i + second.Length + 1;

        second.Dispose();
        third.Dispose();

        using var fourth = sut.Rent( 10 );
        using var fifth = sut.Rent( 4 );
        using var sixth = sut.Rent( 2 );
        using var seventh = sut.Rent( 8 );

        using ( new AssertionScope() )
        {
            first.Should().AllBeEquivalentTo( -1 );
            fourth.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 );
            fifth.Should().BeSequentiallyEqualTo( 11, 12, 13, 14 );
            sixth.Should().BeSequentiallyEqualTo( 15, 16 );
            seventh.Length.Should().Be( 8 );
            seventh.Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Fact]
    public void Rent_ShouldNotReuseFragmentedSegments_WhenRequestedLengthIsLargerThenLargestSegment()
    {
        var sut = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };

        using var first = sut.Rent( 8 );
        foreach ( var s in first.Segments )
            Array.Fill( s.Array!, -1, s.Offset, s.Count );

        var second = sut.Rent( 8 );
        for ( var i = 0; i < second.Length; ++i )
            second[i] = i + 1;

        using var third = sut.Rent( 8 );
        foreach ( var s in third.Segments )
            Array.Fill( s.Array!, -2, s.Offset, s.Count );

        second.Dispose();

        using var fourth = sut.Rent( 9 );
        using var fifth = sut.Rent( 8 );

        using ( new AssertionScope() )
        {
            first.Should().AllBeEquivalentTo( -1 );
            third.Should().AllBeEquivalentTo( -2 );
            fourth.Should().AllBeEquivalentTo( default( int ) );
            fifth.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8 );
        }
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseFragmentedSegmentsInComplexScenario()
    {
        var sut = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };

        var all = sut.Rent( 150 );
        for ( var index = 0; index < all.Length; ++index )
            all[index] = index + 1;

        all.Dispose();

        var a = sut.Rent( 3 );
        var b = sut.Rent( 4 );
        var c = sut.Rent( 5 );
        var d = sut.Rent( 6 );
        var e = sut.Rent( 7 );
        var f = sut.Rent( 8 );
        var g = sut.Rent( 9 );
        var h = sut.Rent( 10 );
        var i = sut.Rent( 11 );
        var j = sut.Rent( 12 );
        var k = sut.Rent( 13 );
        var l = sut.Rent( 14 );
        var m = sut.Rent( 15 );
        var n = sut.Rent( 16 );
        var o = sut.Rent( 17 );

        h.Dispose();
        d.Dispose();
        l.Dispose();
        f.Dispose();
        b.Dispose();
        n.Dispose();
        j.Dispose();
        c.Dispose();
        k.Dispose();

        using var p = sut.Rent( 20 );
        using var q = sut.Rent( 3 );
        using var r = sut.Rent( 9 );
        using var s = sut.Rent( 15 );
        using var t = sut.Rent( 15 );
        using var u = sut.Rent( 10 );
        using var v = sut.Rent( 4 );
        using var w = sut.Rent( 7 );
        using var x = sut.Rent( 4 );
        using var y = sut.Rent( 1 );
        using var z = sut.Rent( 8 );

        using ( new AssertionScope() )
        {
            a.Should().BeSequentiallyEqualTo( 1, 2, 3 );
            e.Should().BeSequentiallyEqualTo( 19, 20, 21, 22, 23, 24, 25 );
            g.Should().BeSequentiallyEqualTo( 34, 35, 36, 37, 38, 39, 40, 41, 42 );
            i.Should().BeSequentiallyEqualTo( 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63 );
            m.Should().BeSequentiallyEqualTo( 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117 );
            o.Should().BeSequentiallyEqualTo( 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150 );
            p.Should().BeSequentiallyEqualTo( 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83 );
            q.Should().BeSequentiallyEqualTo( 84, 85, 86 );
            r.Should().BeSequentiallyEqualTo( 87, 88, 89, 90, 91, 92, 93, 94, 95 );
            s.Should().BeSequentiallyEqualTo( 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132 );
            t.Should().BeSequentiallyEqualTo( 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 );
            u.Should().BeSequentiallyEqualTo( 43, 44, 45, 46, 47, 48, 49, 50, 51, 52 );
            v.Should().BeSequentiallyEqualTo( 26, 27, 28, 29 );
            w.Should().BeSequentiallyEqualTo( 96, 97, 98, 99, 100, 101, 102 );
            x.Should().BeSequentiallyEqualTo( 30, 31, 32, 33 );
            y.Should().BeSequentiallyEqualTo( 133 );
            z.Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 3 )]
    [InlineData( 10 )]
    [InlineData( 16 )]
    public void GreedyRent_ShouldReturnCorrectFirstSequence(int length)
    {
        var sut = new MemorySequencePool<int>( 8 );
        using var result = sut.GreedyRent( length );

        using ( new AssertionScope() )
        {
            result.Length.Should().Be( length );
            result.Owner.Should().BeSameAs( sut );
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 4 )]
    [InlineData( 7 )]
    [InlineData( 8 )]
    public void GreedyRent_ShouldAllocateAtTail_WhenValidFragmentedSegmentExists(int length)
    {
        var sut = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };

        var first = sut.Rent( 8 );
        for ( var i = 0; i < first.Length; ++i )
            first[i] = i + 1;

        var second = sut.Rent( 1 );
        second[0] = -1;
        first.Dispose();

        using var result = sut.GreedyRent( length );
        var other = sut.Rent( 8 );

        using ( new AssertionScope() )
        {
            result.Length.Should().Be( length );
            result.Should().AllBeEquivalentTo( 0 );
            other.Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8 );
            second.Should().AllBeEquivalentTo( -1 );
        }
    }

    [Fact]
    public void SequenceDispose_ShouldReturnSegmentsToThePool()
    {
        var sut = new MemorySequencePool<int>( 16 );
        var result = sut.Rent( 16 );
        var arr = result.Segments[0].Array;

        result.Dispose();

        using var next = sut.Rent( 16 );

        arr.Should().BeSameAs( next.Segments[0].Array );
    }

    [Fact]
    public void SequenceDispose_ShouldDoNothing_WhenCalledSecondTime()
    {
        var sut = new MemorySequencePool<int>( 16 );
        var result = sut.Rent( 16 );
        var arr = result.Segments[0].Array;

        result.Dispose();
        result.Dispose();

        using var next = sut.Rent( 16 );

        arr.Should().BeSameAs( next.Segments[0].Array );
    }

    [Fact]
    public void SequenceDispose_ShouldReturnEmptyOnlyNodeToThePool()
    {
        var sut = new MemorySequencePool<int>( 16 );
        var result = sut.GreedyRent();

        result.Dispose();

        using ( new AssertionScope() )
        {
            sut.Report.CachedNodes.Should().Be( 1 );
            sut.Report.GetRentedNodes().Should().BeEmpty();
        }
    }

    [Fact]
    public void SequenceDispose_ShouldReturnEmptyTailNodePrecededByFragmentedNodeToThePool()
    {
        var sut = new MemorySequencePool<int>( 16 );
        var a = sut.Rent( 8 );
        var b = sut.Rent( 1 );
        var result = sut.GreedyRent();

        a.Dispose();
        b.Dispose();
        result.Dispose();

        using ( new AssertionScope() )
        {
            sut.Report.CachedNodes.Should().Be( 3 );
            sut.Report.FragmentedNodes.Should().Be( 0 );
            sut.Report.GetRentedNodes().Should().BeEmpty();
        }
    }

    [Fact]
    public void SequenceDispose_ShouldReturnEmptyTailNodePrecededByActiveNodeToThePool()
    {
        var sut = new MemorySequencePool<int>( 16 );
        var a = sut.Rent( 8 );
        var result = sut.GreedyRent();

        result.Dispose();

        using ( new AssertionScope() )
        {
            sut.Report.CachedNodes.Should().Be( 1 );
            sut.Report.GetRentedNodes().Should().BeSequentiallyEqualTo( a );
        }
    }

    [Fact]
    public void SequenceDispose_ShouldReturnEmptyHeadNodeSucceededByActiveNodeToThePool()
    {
        var sut = new MemorySequencePool<int>( 16 );
        var result = sut.GreedyRent();
        var a = sut.Rent( 8 );

        result.Dispose();

        using ( new AssertionScope() )
        {
            sut.Report.CachedNodes.Should().Be( 1 );
            sut.Report.FragmentedNodes.Should().Be( 0 );
            sut.Report.GetRentedNodes().Should().BeSequentiallyEqualTo( a );
        }
    }

    [Fact]
    public void SequenceDispose_ShouldReturnEmptyHeadNodeSucceededByFragmentedNodeToThePool()
    {
        var sut = new MemorySequencePool<int>( 16 );
        var result = sut.GreedyRent();
        var a = sut.Rent( 8 );
        var b = sut.Rent( 1 );

        a.Dispose();
        result.Dispose();

        using ( new AssertionScope() )
        {
            sut.Report.CachedNodes.Should().Be( 1 );
            sut.Report.GetFragmentedNodeSizes().Should().BeSequentiallyEqualTo( 8 );
            sut.Report.GetRentedNodes().Should().BeSequentiallyEqualTo( b );
        }
    }

    [Fact]
    public void SequenceDispose_ShouldReturnEmptyIntermediateNodeBetweenFragmentedNodesToThePool()
    {
        var sut = new MemorySequencePool<int>( 16 );
        var a = sut.Rent( 4 );
        var result = sut.GreedyRent();
        var b = sut.Rent( 8 );
        var c = sut.Rent( 1 );

        a.Dispose();
        b.Dispose();
        result.Dispose();

        using ( new AssertionScope() )
        {
            sut.Report.CachedNodes.Should().Be( 2 );
            sut.Report.GetFragmentedNodeSizes().Should().BeSequentiallyEqualTo( 12 );
            sut.Report.GetRentedNodes().Should().BeSequentiallyEqualTo( c );
        }
    }

    [Fact]
    public void SequenceDispose_ShouldReturnEmptyIntermediateNodeBetweenActiveNodesToThePool()
    {
        var sut = new MemorySequencePool<int>( 16 );
        var a = sut.Rent( 4 );
        var result = sut.GreedyRent();
        var b = sut.Rent( 8 );

        result.Dispose();

        using ( new AssertionScope() )
        {
            sut.Report.CachedNodes.Should().Be( 1 );
            sut.Report.FragmentedNodes.Should().Be( 0 );
            sut.Report.GetRentedNodes().Should().BeSequentiallyEqualTo( b, a );
        }
    }

    [Fact]
    public void SequenceDispose_ShouldReturnEmptyIntermediateNodeBetweenFragmentedAndActiveNodesToThePool()
    {
        var sut = new MemorySequencePool<int>( 16 );
        var a = sut.Rent( 4 );
        var result = sut.GreedyRent();
        var b = sut.Rent( 8 );

        a.Dispose();
        result.Dispose();

        using ( new AssertionScope() )
        {
            sut.Report.CachedNodes.Should().Be( 1 );
            sut.Report.GetFragmentedNodeSizes().Should().BeSequentiallyEqualTo( 4 );
            sut.Report.GetRentedNodes().Should().BeSequentiallyEqualTo( b );
        }
    }

    [Fact]
    public void SequenceDispose_ShouldReturnEmptyIntermediateNodeBetweenActiveAndFragmentedNodesToThePool()
    {
        var sut = new MemorySequencePool<int>( 16 );
        var a = sut.Rent( 4 );
        var result = sut.GreedyRent();
        var b = sut.Rent( 8 );
        var c = sut.Rent( 1 );

        b.Dispose();
        result.Dispose();

        using ( new AssertionScope() )
        {
            sut.Report.CachedNodes.Should().Be( 1 );
            sut.Report.GetFragmentedNodeSizes().Should().BeSequentiallyEqualTo( 8 );
            sut.Report.GetRentedNodes().Should().BeSequentiallyEqualTo( c, a );
        }
    }

    [Theory]
    [InlineData( 5, 7 )]
    [InlineData( 5, 11 )]
    [InlineData( 5, 16 )]
    [InlineData( 5, 27 )]
    [InlineData( 5, 48 )]
    [InlineData( 5, 59 )]
    [InlineData( 16, 5 )]
    [InlineData( 16, 16 )]
    [InlineData( 16, 21 )]
    [InlineData( 16, 32 )]
    [InlineData( 16, 53 )]
    [InlineData( 16, 64 )]
    public void ClearReturnedSequences_ShouldClearReturnedSequences_WhenSetToTrue(int firstLength, int length)
    {
        var sut = new MemorySequencePool<int>( 16 ) { ClearReturnedSequences = true };

        using var first = sut.Rent( firstLength );
        foreach ( var s in first.Segments )
            Array.Fill( s.Array!, 1, s.Offset, s.Count );

        using ( var second = sut.Rent( length ) )
        {
            foreach ( var s in second.Segments )
                Array.Fill( s.Array!, 2, s.Offset, s.Count );
        }

        using var result = sut.Rent( length );

        using ( new AssertionScope() )
        {
            first.Should().AllBeEquivalentTo( 1 );
            result.Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Theory]
    [InlineData( 5, 7 )]
    [InlineData( 5, 11 )]
    [InlineData( 5, 16 )]
    [InlineData( 5, 27 )]
    [InlineData( 5, 48 )]
    [InlineData( 5, 59 )]
    [InlineData( 16, 5 )]
    [InlineData( 16, 16 )]
    [InlineData( 16, 21 )]
    [InlineData( 16, 32 )]
    [InlineData( 16, 53 )]
    [InlineData( 16, 64 )]
    public void ClearReturnedSequences_ShouldLeaveOldDataInReturnedSequences_WhenSetToFalse(int firstLength, int length)
    {
        var sut = new MemorySequencePool<int>( 16 ) { ClearReturnedSequences = false };

        using var first = sut.Rent( firstLength );
        foreach ( var s in first.Segments )
            Array.Fill( s.Array!, 1, s.Offset, s.Count );

        using ( var second = sut.Rent( length ) )
        {
            foreach ( var s in second.Segments )
                Array.Fill( s.Array!, 2, s.Offset, s.Count );
        }

        using var result = sut.Rent( length );

        using ( new AssertionScope() )
        {
            first.Should().AllBeEquivalentTo( 1 );
            result.Should().AllBeEquivalentTo( 2 );
        }
    }

    [Fact]
    public void TrimExcess_ShouldRemoveAllReturnedTailSegments_WhenNoActiveSequenceIsRented()
    {
        var sut = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };

        using ( var r = sut.Rent( 48 ) )
        {
            foreach ( var s in r.Segments )
                Array.Fill( s.Array!, 1, s.Offset, s.Count );
        }

        sut.TrimExcess();
        using var result = sut.Rent( 48 );

        result.Should().AllBeEquivalentTo( default( int ) );
    }

    [Fact]
    public void TrimExcess_ShouldRemoveAllReturnedTailSegments_WhenActiveRentedSequenceExists()
    {
        var sut = new MemorySequencePool<int>( 8 ) { ClearReturnedSequences = false };

        using var first = sut.Rent( 48 );
        foreach ( var s in first.Segments )
            Array.Fill( s.Array!, 1, s.Offset, s.Count );

        using ( var r = sut.Rent( 48 ) )
        {
            foreach ( var s in r.Segments )
                Array.Fill( s.Array!, 2, s.Offset, s.Count );
        }

        sut.TrimExcess();
        using var result = sut.Rent( 48 );

        using ( new AssertionScope() )
        {
            first.Length.Should().Be( 48 );
            first.Should().AllBeEquivalentTo( 1 );
            result.Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Fact]
    public void ReportInfo_Default_ShouldReturnCorrectData()
    {
        var sut = default( MemorySequencePool<int>.ReportInfo );

        using ( new AssertionScope() )
        {
            sut.AllocatedSegments.Should().Be( 0 );
            sut.ActiveSegments.Should().Be( 0 );
            sut.CachedNodes.Should().Be( 0 );
            sut.ActiveNodes.Should().Be( 0 );
            sut.FragmentedNodes.Should().Be( 0 );
            sut.ActiveElements.Should().Be( 0 );
            sut.FragmentedElements.Should().Be( 0 );
            sut.GetFragmentedNodeSizes().Should().BeEmpty();
            sut.GetRentedNodes().Should().BeEmpty();
        }
    }

    [Fact]
    public void ReportInfo_ForEmptyPool_ShouldReturnCorrectData()
    {
        var sut = new MemorySequencePool<int>( 16 ).Report;

        using ( new AssertionScope() )
        {
            sut.AllocatedSegments.Should().Be( 0 );
            sut.ActiveSegments.Should().Be( 0 );
            sut.CachedNodes.Should().Be( 0 );
            sut.ActiveNodes.Should().Be( 0 );
            sut.FragmentedNodes.Should().Be( 0 );
            sut.ActiveElements.Should().Be( 0 );
            sut.FragmentedElements.Should().Be( 0 );
            sut.GetFragmentedNodeSizes().Should().BeEmpty();
            sut.GetRentedNodes().Should().BeEmpty();
        }
    }

    [Fact]
    public void ReportInfo_ForPoolInUse_ShouldReturnCorrectData()
    {
        var pool = new MemorySequencePool<int>( 8 );
        var a = pool.Rent( 5 );
        var b = pool.Rent( 17 );
        var c = pool.Rent( 9 );
        var d = pool.Rent( 6 );
        var e = pool.Rent( 14 );
        var f = pool.Rent( 20 );
        var g = pool.Rent( 2 );
        var h = pool.Rent( 8 );
        var i = pool.Rent( 18 );
        var j = pool.Rent( 3 );

        a.Dispose();
        c.Dispose();
        e.Dispose();
        i.Dispose();
        j.Dispose();

        var sut = pool.Report;

        using ( new AssertionScope() )
        {
            sut.AllocatedSegments.Should().Be( 13 );
            sut.ActiveSegments.Should().Be( 11 );
            sut.CachedNodes.Should().Be( 2 );
            sut.ActiveNodes.Should().Be( 8 );
            sut.FragmentedNodes.Should().Be( 3 );
            sut.ActiveElements.Should().Be( 81 );
            sut.FragmentedElements.Should().Be( 28 );
            sut.GetFragmentedNodeSizes().Should().BeSequentiallyEqualTo( 14, 5, 9 );
            sut.GetRentedNodes().Should().BeSequentiallyEqualTo( h, g, f, d, b );
        }
    }
}
