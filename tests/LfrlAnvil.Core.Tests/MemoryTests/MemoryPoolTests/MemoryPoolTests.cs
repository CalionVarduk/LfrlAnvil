using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Internal;
using LfrlAnvil.Memory;

namespace LfrlAnvil.Tests.MemoryTests.MemoryPoolTests;

public class MemoryPoolTests : TestsBase
{
    [Theory]
    [InlineData( -1, 4 )]
    [InlineData( 0, 4 )]
    [InlineData( 1, 4 )]
    [InlineData( 2, 4 )]
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
    [InlineData( (1 << 30) + 1, int.MaxValue )]
    public void Ctor_ShouldComputeSegmentLengthByRoundingUpMinSegmentLengthToPowerOf2(int minLength, int expected)
    {
        var sut = new MemoryPool<int>( minLength );
        sut.SegmentLength.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Rent_ShouldReturnEmptySequence_WhenLengthIsLessThanOne(int length)
    {
        var sut = new MemoryPool<int>( 16 );
        using var result = sut.Rent( length );

        Assertion.All(
                result.Owner.TestNull(),
                result.Clear.TestFalse(),
                result.AsMemory().TestEmpty(),
                result.AsSpan().TestEmpty() )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 4 )]
    [InlineData( 7 )]
    [InlineData( 15 )]
    [InlineData( 16 )]
    [InlineData( 17 )]
    [InlineData( 40 )]
    public void Rent_ShouldReturnCorrectFirstToken(int length)
    {
        var sut = new MemoryPool<int>( 16 );
        var result = sut.Rent( length );

        Assertion.All(
                result.Owner.TestRefEquals( sut ),
                result.Clear.TestFalse(),
                result.AsMemory().Length.TestEquals( length ),
                result.AsSpan().Length.TestEquals( length ) )
            .Go();
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 4, 10 )]
    [InlineData( 7, 5 )]
    [InlineData( 9, 6 )]
    [InlineData( 15, 1 )]
    [InlineData( 16, 1 )]
    [InlineData( 10, 7 )]
    [InlineData( 11, 16 )]
    [InlineData( 12, 40 )]
    public void Rent_ShouldReturnCorrectSecondToken(int firstLength, int length)
    {
        var sut = new MemoryPool<int>( 16 );
        var first = sut.Rent( firstLength );
        first.AsSpan().Fill( 1 );

        var result = sut.Rent( length );

        Assertion.All(
                result.Owner.TestRefEquals( sut ),
                result.Clear.TestFalse(),
                result.AsMemory().Length.TestEquals( length ),
                result.AsSpan().Length.TestEquals( length ),
                result.AsSpan().TestAll( (e, _) => e.TestEquals( default ) ) )
            .Go();
    }

    [Theory]
    [InlineData( 1, 1, 1 )]
    [InlineData( 3, 10, 10 )]
    [InlineData( 5, 10, 14 )]
    [InlineData( 7, 10, 6 )]
    [InlineData( 10, 22, 22 )]
    [InlineData( 10, 22, 21 )]
    [InlineData( 10, 21, 22 )]
    public void Rent_ShouldCorrectlyReuseReturnedTailToken(int firstLength, int tailLength, int length)
    {
        var sut = new MemoryPool<int>( 32 );

        var first = sut.Rent( firstLength );
        first.AsSpan().Fill( 1 );

        using ( var tail = sut.Rent( tailLength ) )
            tail.AsSpan().Fill( 2 );

        var result = sut.Rent( length );

        Assertion.All(
                result.AsSpan().Length.TestEquals( length ),
                result.AsSpan().ToArray().Take( tailLength ).TestAll( (e, _) => e.TestEquals( 2 ) ),
                result.AsSpan().ToArray().Skip( tailLength ).TestAll( (e, _) => e.TestEquals( default ) ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedFragmentedHeadTokens()
    {
        var sut = new MemoryPool<int>( 16 );

        var first = sut.Rent( 16 );
        FillSequence( first, 1 );

        var second = sut.Rent( 8 );
        second.AsSpan().Fill( -1 );

        first.Dispose();

        var third = sut.Rent( 10 );
        var fourth = sut.Rent( 4 );
        var fifth = sut.Rent( 2 );
        var sixth = sut.Rent( 8 );

        Assertion.All(
                second.AsSpan().TestAll( (e, _) => e.TestEquals( -1 ) ),
                third.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ] ),
                fourth.AsSpan().TestSequence( [ 11, 12, 13, 14 ] ),
                fifth.AsSpan().TestSequence( [ 15, 16 ] ),
                sixth.AsSpan().TestAll( (e, _) => e.TestEquals( default ) ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedFragmentedMergedHeadTokens()
    {
        var sut = new MemoryPool<int>( 16 );

        var first = sut.Rent( 8 );
        FillSequence( first, 1 );

        var second = sut.Rent( 8 );
        FillSequence( second, 9 );

        var third = sut.Rent( 16 );
        third.AsSpan().Fill( -1 );

        second.Dispose();
        first.Dispose();

        var fourth = sut.Rent( 10 );
        var fifth = sut.Rent( 4 );
        var sixth = sut.Rent( 2 );
        var seventh = sut.Rent( 8 );

        Assertion.All(
                third.AsSpan().TestAll( (e, _) => e.TestEquals( -1 ) ),
                fourth.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ] ),
                fifth.AsSpan().TestSequence( [ 11, 12, 13, 14 ] ),
                sixth.AsSpan().TestSequence( [ 15, 16 ] ),
                seventh.AsSpan().TestAll( (e, _) => e.TestEquals( default ) ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedFragmentedIntermediateTokens()
    {
        var sut = new MemoryPool<int>( 16 );

        var first = sut.Rent( 8 );
        first.AsSpan().Fill( -1 );

        var second = sut.Rent( 8 );
        FillSequence( second, 1 );

        var third = sut.Rent( 8 );
        third.AsSpan().Fill( -2 );

        second.Dispose();

        var fourth = sut.Rent( 5 );
        var fifth = sut.Rent( 2 );
        var sixth = sut.Rent( 1 );
        var seventh = sut.Rent( 8 );

        Assertion.All(
                first.AsSpan().TestAll( (e, _) => e.TestEquals( -1 ) ),
                third.AsSpan().TestAll( (e, _) => e.TestEquals( -2 ) ),
                fourth.AsSpan().TestSequence( [ 1, 2, 3, 4, 5 ] ),
                fifth.AsSpan().TestSequence( [ 6, 7 ] ),
                sixth.AsSpan().TestSequence( [ 8 ] ),
                seventh.AsSpan().TestAll( (e, _) => e.TestEquals( default ) ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedFragmentedIntermediateMergedWithPrevTokens()
    {
        var sut = new MemoryPool<int>( 16 );

        var first = sut.Rent( 8 );
        FillSequence( first, 1 );

        var second = sut.Rent( 8 );
        FillSequence( second, 9 );

        var third = sut.Rent( 16 );
        third.AsSpan().Fill( -1 );

        first.Dispose();
        second.Dispose();

        var fourth = sut.Rent( 10 );
        var fifth = sut.Rent( 4 );
        var sixth = sut.Rent( 2 );
        var seventh = sut.Rent( 8 );

        Assertion.All(
                third.AsSpan().TestAll( (e, _) => e.TestEquals( -1 ) ),
                fourth.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ] ),
                fifth.AsSpan().TestSequence( [ 11, 12, 13, 14 ] ),
                sixth.AsSpan().TestSequence( [ 15, 16 ] ),
                seventh.AsSpan().TestAll( (e, _) => e.TestEquals( default ) ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedFragmentedIntermediateMergedWithNextTokens()
    {
        var sut = new MemoryPool<int>( 16 );

        var first = sut.Rent( 4 );
        FillSequence( first, 1 );

        var second = sut.Rent( 8 );
        FillSequence( second, 5 );

        var third = sut.Rent( 4 );
        FillSequence( third, 13 );

        var fourth = sut.Rent( 8 );
        fourth.AsSpan().Fill( -1 );

        third.Dispose();
        second.Dispose();

        var fifth = sut.Rent( 7 );
        var sixth = sut.Rent( 3 );
        var seventh = sut.Rent( 2 );
        var eighth = sut.Rent( 8 );

        Assertion.All(
                fourth.AsSpan().TestAll( (e, _) => e.TestEquals( -1 ) ),
                fifth.AsSpan().TestSequence( [ 5, 6, 7, 8, 9, 10, 11 ] ),
                sixth.AsSpan().TestSequence( [ 12, 13, 14 ] ),
                seventh.AsSpan().TestSequence( [ 15, 16 ] ),
                eighth.AsSpan().TestAll( (e, _) => e.TestEquals( default ) ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedFragmentedIntermediateMergedWithPrevAndNextTokens()
    {
        var sut = new MemoryPool<int>( 16 );

        var first = sut.Rent( 4 );
        FillSequence( first, 1 );

        var second = sut.Rent( 8 );
        FillSequence( second, 5 );

        var third = sut.Rent( 4 );
        FillSequence( third, 13 );

        var fourth = sut.Rent( 8 );
        fourth.AsSpan().Fill( -1 );

        first.Dispose();
        third.Dispose();
        second.Dispose();

        var fifth = sut.Rent( 10 );
        var sixth = sut.Rent( 4 );
        var seventh = sut.Rent( 2 );
        var eighth = sut.Rent( 8 );

        Assertion.All(
                fourth.AsSpan().TestAll( (e, _) => e.TestEquals( -1 ) ),
                fifth.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ] ),
                sixth.AsSpan().TestSequence( [ 11, 12, 13, 14 ] ),
                seventh.AsSpan().TestSequence( [ 15, 16 ] ),
                eighth.AsSpan().TestAll( (e, _) => e.TestEquals( default ) ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedTailMergedWithHeadTokens()
    {
        var sut = new MemoryPool<int>( 16 );

        var first = sut.Rent( 8 );
        FillSequence( first, 1 );

        var second = sut.Rent( 8 );
        FillSequence( second, 9 );

        first.Dispose();
        second.Dispose();

        var third = sut.Rent( 10 );
        var fourth = sut.Rent( 4 );
        var fifth = sut.Rent( 2 );
        var sixth = sut.Rent( 8 );

        Assertion.All(
                third.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ] ),
                fourth.AsSpan().TestSequence( [ 11, 12, 13, 14 ] ),
                fifth.AsSpan().TestSequence( [ 15, 16 ] ),
                sixth.AsSpan().TestAll( (e, _) => e.TestEquals( default ) ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseReturnedTailMergedWithNonHeadPrevTokens()
    {
        var sut = new MemoryPool<int>( 16 );

        var first = sut.Rent( 16 );
        first.AsSpan().Fill( -1 );

        var second = sut.Rent( 8 );
        FillSequence( second, 1 );

        var third = sut.Rent( 8 );
        FillSequence( third, 9 );

        second.Dispose();
        third.Dispose();

        var fourth = sut.Rent( 10 );
        var fifth = sut.Rent( 4 );
        var sixth = sut.Rent( 2 );
        var seventh = sut.Rent( 8 );

        Assertion.All(
                first.AsSpan().TestAll( (e, _) => e.TestEquals( -1 ) ),
                fourth.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ] ),
                fifth.AsSpan().TestSequence( [ 11, 12, 13, 14 ] ),
                sixth.AsSpan().TestSequence( [ 15, 16 ] ),
                seventh.AsSpan().TestAll( (e, _) => e.TestEquals( default ) ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldNotReuseFragmentedTokens_WhenRequestedLengthIsLargerThenLargestToken()
    {
        var sut = new MemoryPool<int>( 16 );

        var first = sut.Rent( 8 );
        first.AsSpan().Fill( -1 );

        var second = sut.Rent( 8 );
        FillSequence( second, 1 );

        var third = sut.Rent( 7 );
        third.AsSpan().Fill( -2 );

        second.Dispose();

        var fourth = sut.Rent( 9 );
        var fifth = sut.Rent( 8 );

        Assertion.All(
                first.AsSpan().TestAll( (e, _) => e.TestEquals( -1 ) ),
                third.AsSpan().TestAll( (e, _) => e.TestEquals( -2 ) ),
                fourth.AsSpan().TestAll( (e, _) => e.TestEquals( default ) ),
                fifth.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8 ] ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldCorrectlyReuseFragmentedTokensInComplexScenario()
    {
        var sut = new MemoryPool<int>( 8 );

        var all = sut.Rent( 150 );
        FillSequence( all, 1 );
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

        var p = sut.Rent( 20 );
        var q = sut.Rent( 3 );
        var r = sut.Rent( 9 );
        var s = sut.Rent( 15 );
        var t = sut.Rent( 15 );
        var u = sut.Rent( 10 );
        var v = sut.Rent( 4 );
        var w = sut.Rent( 7 );
        var x = sut.Rent( 4 );
        var y = sut.Rent( 1 );
        var z = sut.Rent( 8 );

        Assertion.All(
                a.AsSpan().TestSequence( [ 1, 2, 3 ] ),
                e.AsSpan().TestSequence( [ 19, 20, 21, 22, 23, 24, 25 ] ),
                g.AsSpan().TestSequence( [ 34, 35, 36, 37, 38, 39, 40, 41, 42 ] ),
                i.AsSpan().TestSequence( [ 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63 ] ),
                m.AsSpan().TestSequence( [ 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117 ] ),
                o.AsSpan().TestSequence( [ 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150 ] ),
                p.AsSpan().TestSequence( [ 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83 ] ),
                q.AsSpan().TestSequence( [ 84, 85, 86 ] ),
                r.AsSpan().TestSequence( [ 87, 88, 89, 90, 91, 92, 93, 94, 95 ] ),
                s.AsSpan().TestSequence( [ 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132 ] ),
                t.AsSpan().TestSequence( [ 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 ] ),
                u.AsSpan().TestSequence( [ 43, 44, 45, 46, 47, 48, 49, 50, 51, 52 ] ),
                v.AsSpan().TestSequence( [ 26, 27, 28, 29 ] ),
                w.AsSpan().TestSequence( [ 96, 97, 98, 99, 100, 101, 102 ] ),
                x.AsSpan().TestSequence( [ 30, 31, 32, 33 ] ),
                y.AsSpan().TestSequence( [ 133 ] ),
                z.AsSpan().TestAll( (el, _) => el.TestEquals( default ) ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldAllocateLargeSegment_WithoutActiveSegments()
    {
        var sut = new MemoryPool<int>( 8 );
        var a = sut.Rent( 6 );
        a.Dispose();
        var b = sut.Rent( 9 );

        Assertion.All(
                b.TryGetInfo()
                    .TestEquals(
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 1,
                            segmentLength: 16,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Create( 1 ),
                            startIndex: 0,
                            length: 9,
                            isFragmented: false ) ),
                ToArray( sut.Report.Nodes )
                    .TestSequence(
                    [
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 1,
                            segmentLength: 16,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Create( 1 ),
                            startIndex: 0,
                            length: 9,
                            isFragmented: false ),
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 0,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 1,
                            nodeId: NullableIndex.Create( 0 ),
                            startIndex: 0,
                            length: 8,
                            isFragmented: true ),
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 1,
                            segmentLength: 16,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Null,
                            startIndex: 9,
                            length: 7,
                            isFragmented: false )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldAllocateLargeSegment_WithActiveSegments()
    {
        var sut = new MemoryPool<int>( 8 );
        var a = sut.Rent( 8 );
        var b = sut.Rent( 6 );
        var c = sut.Rent( 6 );
        b.Dispose();
        c.Dispose();
        var d = sut.Rent( 9 );

        Assertion.All(
                a.TryGetInfo()
                    .TestEquals(
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 0,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Create( 0 ),
                            startIndex: 0,
                            length: 8,
                            isFragmented: false ) ),
                d.TryGetInfo()
                    .TestEquals(
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 3,
                            segmentLength: 16,
                            isSegmentActive: true,
                            version: 1,
                            nodeId: NullableIndex.Create( 1 ),
                            startIndex: 0,
                            length: 9,
                            isFragmented: false ) ),
                ToArray( sut.Report.Nodes )
                    .TestSequence(
                    [
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 3,
                            segmentLength: 16,
                            isSegmentActive: true,
                            version: 1,
                            nodeId: NullableIndex.Create( 1 ),
                            startIndex: 0,
                            length: 9,
                            isFragmented: false ),
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 2,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 1,
                            nodeId: NullableIndex.Create( 2 ),
                            startIndex: 0,
                            length: 8,
                            isFragmented: true ),
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 1,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Create( 3 ),
                            startIndex: 0,
                            length: 8,
                            isFragmented: true ),
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 0,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Create( 0 ),
                            startIndex: 0,
                            length: 8,
                            isFragmented: false ),
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 3,
                            segmentLength: 16,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Null,
                            startIndex: 9,
                            length: 7,
                            isFragmented: false )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldAllocateLargeSegment_WithRemainingFreeTail()
    {
        var sut = new MemoryPool<int>( 8 );
        var a = sut.Rent( 7 );
        var b = sut.Rent( 6 );
        b.Dispose();
        var c = sut.Rent( 9 );

        Assertion.All(
                a.TryGetInfo()
                    .TestEquals(
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 0,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Create( 0 ),
                            startIndex: 0,
                            length: 7,
                            isFragmented: false ) ),
                c.TryGetInfo()
                    .TestEquals(
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 2,
                            segmentLength: 16,
                            isSegmentActive: true,
                            version: 1,
                            nodeId: NullableIndex.Create( 1 ),
                            startIndex: 0,
                            length: 9,
                            isFragmented: false ) ),
                ToArray( sut.Report.Nodes )
                    .TestSequence(
                    [
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 2,
                            segmentLength: 16,
                            isSegmentActive: true,
                            version: 1,
                            nodeId: NullableIndex.Create( 1 ),
                            startIndex: 0,
                            length: 9,
                            isFragmented: false ),
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 1,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Create( 2 ),
                            startIndex: 0,
                            length: 8,
                            isFragmented: true ),
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 0,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Create( 3 ),
                            startIndex: 7,
                            length: 1,
                            isFragmented: true ),
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 0,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Create( 0 ),
                            startIndex: 0,
                            length: 7,
                            isFragmented: false ),
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 2,
                            segmentLength: 16,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Null,
                            startIndex: 9,
                            length: 7,
                            isFragmented: false )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldReusePreviouslyAllocatedLargeSegment()
    {
        var sut = new MemoryPool<int>( 8 );
        var a = sut.Rent( 8 );
        FillSequence( a, 1 );
        var b = sut.Rent( 9 );
        FillSequence( b, 9 );
        b.Dispose();

        var result = sut.Rent( 9 );

        Assertion.All(
                result.AsSpan().TestSequence( [ 9, 10, 11, 12, 13, 14, 15, 16, 17 ] ),
                ToArray( sut.Report.Nodes )
                    .TestSequence(
                    [
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 1,
                            segmentLength: 16,
                            isSegmentActive: true,
                            version: 1,
                            nodeId: NullableIndex.Create( 1 ),
                            startIndex: 0,
                            length: 9,
                            isFragmented: false ),
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 0,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Create( 0 ),
                            startIndex: 0,
                            length: 8,
                            isFragmented: false ),
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 1,
                            segmentLength: 16,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Null,
                            startIndex: 9,
                            length: 7,
                            isFragmented: false )
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void GreedyRent_ShouldReturnEmptySequence_WhenLengthIsLessThanOne(int length)
    {
        var sut = new MemoryPool<int>( 16 );
        using var result = sut.GreedyRent( length );

        Assertion.All(
                result.Owner.TestNull(),
                result.Clear.TestFalse(),
                result.AsMemory().TestEmpty(),
                result.AsSpan().TestEmpty() )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 3 )]
    [InlineData( 10 )]
    [InlineData( 16 )]
    public void GreedyRent_ShouldReturnCorrectFirstSequence(int length)
    {
        var sut = new MemoryPool<int>( 8 );
        using var result = sut.GreedyRent( length );

        Assertion.All(
                result.AsSpan().Length.TestEquals( length ),
                result.Owner.TestRefEquals( sut ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 4 )]
    [InlineData( 7 )]
    [InlineData( 8 )]
    public void GreedyRent_ShouldAllocateAtTail_WhenValidFragmentedSegmentExists(int length)
    {
        var sut = new MemoryPool<int>( 8 );

        var first = sut.Rent( 8 );
        FillSequence( first, 1 );

        var second = sut.Rent( 1 );
        second.AsSpan().Fill( -1 );
        first.Dispose();

        using var result = sut.GreedyRent( length );
        var other = sut.Rent( 8 );

        Assertion.All(
                result.AsSpan().Length.TestEquals( length ),
                result.AsSpan().TestAll( (e, _) => e.TestEquals( 0 ) ),
                other.AsSpan().TestSequence( [ 1, 2, 3, 4, 5, 6, 7, 8 ] ),
                second.AsSpan().TestAll( (e, _) => e.TestEquals( -1 ) ) )
            .Go();
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
    public void TokenClear_ShouldClearReturnedToken_WhenEnabled(int firstLength, int length)
    {
        var sut = new MemoryPool<int>( 64 );

        using var first = sut.Rent( firstLength ).EnableClearing();
        first.AsSpan().Fill( 1 );

        using ( var second = sut.Rent( length ).EnableClearing() )
            second.AsSpan().Fill( 2 );

        using var result = sut.Rent( length );

        Assertion.All(
                first.AsSpan().TestAll( (e, _) => e.TestEquals( 1 ) ),
                result.AsSpan().TestAll( (e, _) => e.TestEquals( default ) ) )
            .Go();
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
    public void TokenClear_ShouldLeaveOldDataInReturnedTokens_WhenDisabled(int firstLength, int length)
    {
        var sut = new MemoryPool<int>( 64 );

        using var first = sut.Rent( firstLength ).EnableClearing( false );
        first.AsSpan().Fill( 1 );

        using ( var second = sut.Rent( length ).EnableClearing( false ) )
            second.AsSpan().Fill( 2 );

        using var result = sut.Rent( length );

        Assertion.All(
                first.AsSpan().TestAll( (e, _) => e.TestEquals( 1 ) ),
                result.AsSpan().TestAll( (e, _) => e.TestEquals( 2 ) ) )
            .Go();
    }

    [Fact]
    public void TrimExcess_ShouldRemoveAllReturnedTailTokens_WhenNoActiveTokenIsRented()
    {
        var sut = new MemoryPool<int>( 16 );

        using ( var r = sut.Rent( 12 ) )
            r.AsSpan().Fill( 1 );

        sut.TrimExcess();
        using var result = sut.Rent( 12 );

        result.AsSpan().TestAll( (e, _) => e.TestEquals( default ) ).Go();
    }

    [Fact]
    public void TrimExcess_ShouldRemoveAllReturnedTailTokens_WhenActiveRentedTokenExists()
    {
        var sut = new MemoryPool<int>( 16 );

        using var first = sut.Rent( 12 );
        first.AsSpan().Fill( 1 );

        using ( var r = sut.Rent( 12 ) )
            r.AsSpan().Fill( 2 );

        sut.TrimExcess();
        using var result = sut.Rent( 12 );

        Assertion.All(
                first.AsSpan().Length.TestEquals( 12 ),
                first.AsSpan().TestAll( (e, _) => e.TestEquals( 1 ) ),
                result.AsSpan().TestAll( (e, _) => e.TestEquals( default ) ) )
            .Go();
    }

    [Fact]
    public void TrimExcess_ShouldReorganizeFreeNodes_WhenFreeNodesExistInBetweenActiveNodes()
    {
        var sut = new MemoryPool<int>( 8 );
        var a = sut.Rent( 5 );
        var b = sut.Rent( 5 );
        var c = sut.Rent( 4 );
        var d = sut.Rent( 4 );
        var e = sut.Rent( 5 );
        a.Dispose();
        b.Dispose();
        c.Dispose();
        d.Dispose();
        e.Dispose();
        a = sut.Rent( 3 );
        b = sut.Rent( 5 );
        c = sut.Rent( 7 );

        sut.TrimExcess();

        d = sut.Rent( 1 );
        e = sut.Rent( 1 );

        Assertion.All(
                a.TryGetInfo()
                    .TestEquals(
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 0,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Create( 2 ),
                            startIndex: 0,
                            length: 3,
                            isFragmented: false ) ),
                b.TryGetInfo()
                    .TestEquals(
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 0,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 0,
                            nodeId: NullableIndex.Create( 4 ),
                            startIndex: 3,
                            length: 5,
                            isFragmented: false ) ),
                c.TryGetInfo()
                    .TestEquals(
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 1,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 1,
                            nodeId: NullableIndex.Create( 3 ),
                            startIndex: 0,
                            length: 7,
                            isFragmented: false ) ),
                d.TryGetInfo()
                    .TestEquals(
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 1,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 1,
                            nodeId: NullableIndex.Create( 0 ),
                            startIndex: 7,
                            length: 1,
                            isFragmented: false ) ),
                e.TryGetInfo()
                    .TestEquals(
                        new MemoryPool<int>.ReportInfo.Node(
                            pool: sut,
                            segmentIndex: 2,
                            segmentLength: 8,
                            isSegmentActive: true,
                            version: 1,
                            nodeId: NullableIndex.Create( 1 ),
                            startIndex: 0,
                            length: 1,
                            isFragmented: false ) ) )
            .Go();
    }

    [Fact]
    public void ReportInfo_Default_ShouldReturnCorrectData()
    {
        var sut = default( MemoryPool<int>.ReportInfo );

        Assertion.All(
                sut.AllocatedSegments.TestEquals( 0 ),
                sut.ActiveSegments.TestEquals( 0 ),
                ToArray( sut.Nodes ).TestEmpty(),
                sut.FragmentedNodes.Count.TestEquals( 0 ),
                ToArray( sut.FragmentedNodes ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void ReportInfo_ForEmptyPool_ShouldReturnCorrectData()
    {
        var sut = new MemoryPool<int>( 16 ).Report;

        Assertion.All(
                sut.AllocatedSegments.TestEquals( 0 ),
                sut.ActiveSegments.TestEquals( 0 ),
                ToArray( sut.Nodes ).TestEmpty(),
                sut.FragmentedNodes.Count.TestEquals( 0 ),
                ToArray( sut.FragmentedNodes ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void ReportInfo_ForPoolInUse_ShouldReturnCorrectData()
    {
        var pool = new MemoryPool<int>( 16 );
        var a = pool.Rent( 5 );
        _ = pool.Rent( 17 );
        var c = pool.Rent( 9 );
        _ = pool.Rent( 6 );
        var e = pool.Rent( 14 );
        _ = pool.Rent( 20 );
        _ = pool.Rent( 2 );
        _ = pool.Rent( 8 );
        var i = pool.Rent( 18 );
        var j = pool.Rent( 3 );

        a.Dispose();
        c.Dispose();
        e.Dispose();
        i.Dispose();
        j.Dispose();

        var sut = pool.Report;
        var nodes = ToArray( sut.Nodes );
        var fragmentedNodes = ToArray( sut.FragmentedNodes );

        Assertion.All(
                sut.AllocatedSegments.TestEquals( 5 ),
                sut.ActiveSegments.TestEquals( 4 ),
                sut.FragmentedNodes.Count.TestEquals( 3 ),
                fragmentedNodes.TestSequence(
                [
                    new MemoryPool<int>.ReportInfo.FragmentedNode( segmentIndex: 0, segmentLength: 16, startIndex: 0, length: 16 ),
                    new MemoryPool<int>.ReportInfo.FragmentedNode( segmentIndex: 1, segmentLength: 32, startIndex: 25, length: 7 ),
                    new MemoryPool<int>.ReportInfo.FragmentedNode( segmentIndex: 2, segmentLength: 16, startIndex: 0, length: 16 )
                ] ),
                nodes.TestSequence(
                [
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 3,
                        segmentLength: 32,
                        isSegmentActive: true,
                        version: 0,
                        nodeId: NullableIndex.Create( 10 ),
                        startIndex: 20,
                        length: 8,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 3,
                        segmentLength: 32,
                        isSegmentActive: true,
                        version: 0,
                        nodeId: NullableIndex.Create( 7 ),
                        startIndex: 0,
                        length: 20,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 2,
                        segmentLength: 16,
                        isSegmentActive: true,
                        version: 0,
                        nodeId: NullableIndex.Create( 8 ),
                        startIndex: 0,
                        length: 16,
                        isFragmented: true ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 1,
                        segmentLength: 32,
                        isSegmentActive: true,
                        version: 0,
                        nodeId: NullableIndex.Create( 13 ),
                        startIndex: 25,
                        length: 7,
                        isFragmented: true ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 1,
                        segmentLength: 32,
                        isSegmentActive: true,
                        version: 0,
                        nodeId: NullableIndex.Create( 6 ),
                        startIndex: 23,
                        length: 2,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 1,
                        segmentLength: 32,
                        isSegmentActive: true,
                        version: 0,
                        nodeId: NullableIndex.Create( 4 ),
                        startIndex: 17,
                        length: 6,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 1,
                        segmentLength: 32,
                        isSegmentActive: true,
                        version: 0,
                        nodeId: NullableIndex.Create( 1 ),
                        startIndex: 0,
                        length: 17,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 0,
                        segmentLength: 16,
                        isSegmentActive: true,
                        version: 1,
                        nodeId: NullableIndex.Create( 0 ),
                        startIndex: 0,
                        length: 16,
                        isFragmented: true ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 3,
                        segmentLength: 32,
                        isSegmentActive: true,
                        version: 0,
                        nodeId: NullableIndex.Null,
                        startIndex: 28,
                        length: 4,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 4,
                        segmentLength: 32,
                        isSegmentActive: false,
                        version: 0,
                        nodeId: NullableIndex.Null,
                        startIndex: 0,
                        length: 32,
                        isFragmented: false )
                ] ) )
            .Go();
    }

    [Fact]
    public void ReportInfo_ForPoolWithSingleFullyUsedSegment_ShouldReturnCorrectData()
    {
        var pool = new MemoryPool<int>( 16 );
        _ = pool.Rent( 16 );

        var sut = pool.Report;
        var nodes = ToArray( sut.Nodes );
        var fragmentedNodes = ToArray( sut.FragmentedNodes );

        Assertion.All(
                sut.AllocatedSegments.TestEquals( 1 ),
                sut.ActiveSegments.TestEquals( 1 ),
                sut.FragmentedNodes.Count.TestEquals( 0 ),
                fragmentedNodes.TestEmpty(),
                nodes.TestSequence(
                [
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 0,
                        segmentLength: 16,
                        isSegmentActive: true,
                        version: 0,
                        nodeId: NullableIndex.Create( 0 ),
                        startIndex: 0,
                        length: 16,
                        isFragmented: false )
                ] ) )
            .Go();
    }

    [Fact]
    public void ReportInfoFragmentedNode_ShouldHaveCorrectProperties()
    {
        var pool = new MemoryPool<int>( 8 );
        var token = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        token.Dispose();

        var sut = ToArray( pool.Report.FragmentedNodes )[0];

        Assertion.All(
                sut.SegmentIndex.TestEquals( 0 ),
                sut.SegmentLength.TestEquals( 8 ),
                sut.StartIndex.TestEquals( 0 ),
                sut.Length.TestEquals( 3 ),
                sut.EndIndex.TestEquals( 3 ),
                sut.ToString().TestEquals( "Segment: @0 (Length: 8), Node: [0:2] (3)" ) )
            .Go();
    }

    [Fact]
    public void ReportInfoNode_ShouldHaveCorrectProperties_ForActiveNode()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 5 );

        var sut = ToArray( pool.Report.Nodes )[0];

        Assertion.All(
                sut.SegmentIndex.TestEquals( 0 ),
                sut.SegmentLength.TestEquals( 8 ),
                sut.StartIndex.TestEquals( 0 ),
                sut.Length.TestEquals( 5 ),
                sut.EndIndex.TestEquals( 5 ),
                sut.IsSegmentActive.TestTrue(),
                sut.IsFragmented.TestFalse(),
                sut.ToString().TestEquals( "Segment: @0 (Length: 8), Node: [0:4] (5)" ) )
            .Go();
    }

    [Fact]
    public void ReportInfoNode_ShouldHaveCorrectProperties_ForFragmentedNode()
    {
        var pool = new MemoryPool<int>( 8 );
        var token = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        token.Dispose();

        var sut = ToArray( pool.Report.Nodes )[1];

        Assertion.All(
                sut.SegmentIndex.TestEquals( 0 ),
                sut.SegmentLength.TestEquals( 8 ),
                sut.StartIndex.TestEquals( 0 ),
                sut.Length.TestEquals( 3 ),
                sut.EndIndex.TestEquals( 3 ),
                sut.IsSegmentActive.TestTrue(),
                sut.IsFragmented.TestTrue(),
                sut.ToString().TestEquals( "Segment: @0 (Length: 8), Node: [0:2] (3) (fragmented)" ) )
            .Go();
    }

    [Fact]
    public void ReportInfoNode_ShouldHaveCorrectProperties_ForFreeTailSegmentNode()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 5 );

        var sut = ToArray( pool.Report.Nodes )[1];

        Assertion.All(
                sut.SegmentIndex.TestEquals( 0 ),
                sut.SegmentLength.TestEquals( 8 ),
                sut.StartIndex.TestEquals( 5 ),
                sut.Length.TestEquals( 3 ),
                sut.EndIndex.TestEquals( 8 ),
                sut.IsSegmentActive.TestTrue(),
                sut.IsFragmented.TestFalse(),
                sut.ToString().TestEquals( "Segment: @0 (Length: 8), Node: [5:7] (3) (free tail)" ) )
            .Go();
    }

    [Fact]
    public void ReportInfoNode_ShouldHaveCorrectProperties_ForInactiveSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        var token = pool.Rent( 8 );
        token.Dispose();

        var sut = ToArray( pool.Report.Nodes )[0];

        Assertion.All(
                sut.SegmentIndex.TestEquals( 0 ),
                sut.SegmentLength.TestEquals( 8 ),
                sut.StartIndex.TestEquals( 0 ),
                sut.Length.TestEquals( 8 ),
                sut.EndIndex.TestEquals( 8 ),
                sut.IsSegmentActive.TestFalse(),
                sut.IsFragmented.TestFalse(),
                sut.ToString().TestEquals( "Segment: @0 (Length: 8) (inactive)" ) )
            .Go();
    }

    [Fact]
    public void ReportInfoNode_TryGetToken_ShouldReturnToken_ForActiveNode()
    {
        var pool = new MemoryPool<int>( 8 );
        var token = pool.Rent( 5 );
        var sut = ToArray( pool.Report.Nodes )[0];

        var result = sut.TryGetToken();

        result.TestEquals( token ).Go();
    }

    [Fact]
    public void ReportInfoNode_TryGetToken_ShouldReturnNull_ForFragmentedNode()
    {
        var pool = new MemoryPool<int>( 8 );
        var token = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        token.Dispose();
        var sut = ToArray( pool.Report.Nodes )[1];

        var result = sut.TryGetToken();

        result.TestNull().Go();
    }

    [Fact]
    public void ReportInfoNode_TryGetToken_ShouldReturnNull_ForFreeTailSegmentNode()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 5 );
        var sut = ToArray( pool.Report.Nodes )[1];

        var result = sut.TryGetToken();

        result.TestNull().Go();
    }

    [Fact]
    public void ReportInfoNode_TryGetToken_ShouldReturnNull_ForInactiveSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        var token = pool.Rent( 8 );
        token.Dispose();
        var sut = ToArray( pool.Report.Nodes )[0];

        var result = sut.TryGetToken();

        result.TestNull().Go();
    }

    [Pure]
    private static MemoryPool<T>.ReportInfo.Node[] ToArray<T>(MemoryPool<T>.ReportInfo.NodeCollection source)
    {
        var result = new List<MemoryPool<T>.ReportInfo.Node>();
        foreach ( var n in source )
            result.Add( n );

        return result.ToArray();
    }

    [Pure]
    private static MemoryPool<T>.ReportInfo.FragmentedNode[] ToArray<T>(MemoryPool<T>.ReportInfo.FragmentedNodeCollection source)
    {
        var result = new List<MemoryPool<T>.ReportInfo.FragmentedNode>();
        foreach ( var n in source )
            result.Add( n );

        return result.ToArray();
    }

    private static void FillSequence(MemoryPoolToken<int> rent, int start)
    {
        var span = rent.AsSpan();
        for ( var i = 0; i < span.Length; ++i )
            span[i] = start + i;
    }
}
