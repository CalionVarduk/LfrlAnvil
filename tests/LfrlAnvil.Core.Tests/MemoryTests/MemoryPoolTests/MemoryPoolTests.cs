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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Internal;
using LfrlAnvil.Memory;
using LfrlAnvil.TestExtensions.FluentAssertions;

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
        sut.SegmentLength.Should().Be( expected );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Rent_ShouldReturnEmptySequence_WhenLengthIsLessThanOne(int length)
    {
        var sut = new MemoryPool<int>( 16 );
        using var result = sut.Rent( length );

        using ( new AssertionScope() )
        {
            result.Owner.Should().BeNull();
            result.Clear.Should().BeFalse();
            result.AsMemory().ToArray().Should().BeEmpty();
            result.AsSpan().ToArray().Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.Owner.Should().BeSameAs( sut );
            result.Clear.Should().BeFalse();
            result.AsMemory().Length.Should().Be( length );
            result.AsSpan().Length.Should().Be( length );
        }
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

        using ( new AssertionScope() )
        {
            result.Owner.Should().BeSameAs( sut );
            result.Clear.Should().BeFalse();
            result.AsMemory().Length.Should().Be( length );
            result.AsSpan().Length.Should().Be( length );
            result.AsSpan().ToArray().Should().AllBeEquivalentTo( default( int ) );
        }
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

        using ( new AssertionScope() )
        {
            result.AsSpan().Length.Should().Be( length );
            result.AsSpan().ToArray().Take( tailLength ).Should().AllBeEquivalentTo( 2 );
            result.AsSpan().ToArray().Skip( tailLength ).Should().AllBeEquivalentTo( default( int ) );
        }
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

        using ( new AssertionScope() )
        {
            second.AsSpan().ToArray().Should().AllBeEquivalentTo( -1 );
            third.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 );
            fourth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 11, 12, 13, 14 );
            fifth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 15, 16 );
            sixth.AsSpan().ToArray().Should().AllBeEquivalentTo( default( int ) );
        }
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

        using ( new AssertionScope() )
        {
            third.AsSpan().ToArray().Should().AllBeEquivalentTo( -1 );
            fourth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 );
            fifth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 11, 12, 13, 14 );
            sixth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 15, 16 );
            seventh.AsSpan().ToArray().Should().AllBeEquivalentTo( default( int ) );
        }
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

        using ( new AssertionScope() )
        {
            first.AsSpan().ToArray().Should().AllBeEquivalentTo( -1 );
            third.AsSpan().ToArray().Should().AllBeEquivalentTo( -2 );
            fourth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5 );
            fifth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 6, 7 );
            sixth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 8 );
            seventh.AsSpan().ToArray().Should().AllBeEquivalentTo( default( int ) );
        }
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

        using ( new AssertionScope() )
        {
            third.AsSpan().ToArray().Should().AllBeEquivalentTo( -1 );
            fourth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 );
            fifth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 11, 12, 13, 14 );
            sixth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 15, 16 );
            seventh.AsSpan().ToArray().Should().AllBeEquivalentTo( default( int ) );
        }
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

        using ( new AssertionScope() )
        {
            fourth.AsSpan().ToArray().Should().AllBeEquivalentTo( -1 );
            fifth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 5, 6, 7, 8, 9, 10, 11 );
            sixth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 12, 13, 14 );
            seventh.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 15, 16 );
            eighth.AsSpan().ToArray().Should().AllBeEquivalentTo( default( int ) );
        }
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

        using ( new AssertionScope() )
        {
            fourth.AsSpan().ToArray().Should().AllBeEquivalentTo( -1 );
            fifth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 );
            sixth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 11, 12, 13, 14 );
            seventh.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 15, 16 );
            eighth.AsSpan().ToArray().Should().AllBeEquivalentTo( default( int ) );
        }
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

        using ( new AssertionScope() )
        {
            third.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 );
            fourth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 11, 12, 13, 14 );
            fifth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 15, 16 );
            sixth.AsSpan().ToArray().Should().AllBeEquivalentTo( default( int ) );
        }
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

        using ( new AssertionScope() )
        {
            first.AsSpan().ToArray().Should().AllBeEquivalentTo( -1 );
            fourth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 );
            fifth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 11, 12, 13, 14 );
            sixth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 15, 16 );
            seventh.AsSpan().ToArray().Should().AllBeEquivalentTo( default( int ) );
        }
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

        using ( new AssertionScope() )
        {
            first.AsSpan().ToArray().Should().AllBeEquivalentTo( -1 );
            third.AsSpan().ToArray().Should().AllBeEquivalentTo( -2 );
            fourth.AsSpan().ToArray().Should().AllBeEquivalentTo( default( int ) );
            fifth.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8 );
        }
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

        using ( new AssertionScope() )
        {
            a.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3 );
            e.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 19, 20, 21, 22, 23, 24, 25 );
            g.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 34, 35, 36, 37, 38, 39, 40, 41, 42 );
            i.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63 );
            m.AsSpan()
                .ToArray()
                .Should()
                .BeSequentiallyEqualTo( 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117 );

            o.AsSpan()
                .ToArray()
                .Should()
                .BeSequentiallyEqualTo( 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150 );

            p.AsSpan()
                .ToArray()
                .Should()
                .BeSequentiallyEqualTo( 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83 );

            q.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 84, 85, 86 );
            r.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 87, 88, 89, 90, 91, 92, 93, 94, 95 );
            s.AsSpan()
                .ToArray()
                .Should()
                .BeSequentiallyEqualTo( 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132 );

            t.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 );
            u.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 43, 44, 45, 46, 47, 48, 49, 50, 51, 52 );
            v.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 26, 27, 28, 29 );
            w.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 96, 97, 98, 99, 100, 101, 102 );
            x.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 30, 31, 32, 33 );
            y.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 133 );
            z.AsSpan().ToArray().Should().AllBeEquivalentTo( default( int ) );
        }
    }

    [Fact]
    public void Rent_ShouldAllocateLargeSegment_WithoutActiveSegments()
    {
        var sut = new MemoryPool<int>( 8 );
        var a = sut.Rent( 6 );
        a.Dispose();
        var b = sut.Rent( 9 );

        using ( new AssertionScope() )
        {
            b.TryGetInfo()
                .Should()
                .BeEquivalentTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 1,
                        segmentLength: 16,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 1 ),
                        startIndex: 0,
                        length: 9,
                        isFragmented: false ) );

            ToArray( sut.Report.Nodes )
                .Should()
                .BeSequentiallyEqualTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 1,
                        segmentLength: 16,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 1 ),
                        startIndex: 0,
                        length: 9,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 0,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 0 ),
                        startIndex: 0,
                        length: 8,
                        isFragmented: true ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 1,
                        segmentLength: 16,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Null,
                        startIndex: 9,
                        length: 7,
                        isFragmented: false ) );
        }
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

        using ( new AssertionScope() )
        {
            a.TryGetInfo()
                .Should()
                .BeEquivalentTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 0,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 0 ),
                        startIndex: 0,
                        length: 8,
                        isFragmented: false ) );

            d.TryGetInfo()
                .Should()
                .BeEquivalentTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 3,
                        segmentLength: 16,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 1 ),
                        startIndex: 0,
                        length: 9,
                        isFragmented: false ) );

            ToArray( sut.Report.Nodes )
                .Should()
                .BeSequentiallyEqualTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 3,
                        segmentLength: 16,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 1 ),
                        startIndex: 0,
                        length: 9,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 2,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 2 ),
                        startIndex: 0,
                        length: 8,
                        isFragmented: true ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 1,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 3 ),
                        startIndex: 0,
                        length: 8,
                        isFragmented: true ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 0,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 0 ),
                        startIndex: 0,
                        length: 8,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 3,
                        segmentLength: 16,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Null,
                        startIndex: 9,
                        length: 7,
                        isFragmented: false ) );
        }
    }

    [Fact]
    public void Rent_ShouldAllocateLargeSegment_WithRemainingFreeTail()
    {
        var sut = new MemoryPool<int>( 8 );
        var a = sut.Rent( 7 );
        var b = sut.Rent( 6 );
        b.Dispose();
        var c = sut.Rent( 9 );

        using ( new AssertionScope() )
        {
            a.TryGetInfo()
                .Should()
                .BeEquivalentTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 0,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 0 ),
                        startIndex: 0,
                        length: 7,
                        isFragmented: false ) );

            c.TryGetInfo()
                .Should()
                .BeEquivalentTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 2,
                        segmentLength: 16,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 1 ),
                        startIndex: 0,
                        length: 9,
                        isFragmented: false ) );

            ToArray( sut.Report.Nodes )
                .Should()
                .BeSequentiallyEqualTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 2,
                        segmentLength: 16,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 1 ),
                        startIndex: 0,
                        length: 9,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 1,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 2 ),
                        startIndex: 0,
                        length: 8,
                        isFragmented: true ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 0,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 3 ),
                        startIndex: 7,
                        length: 1,
                        isFragmented: true ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 0,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 0 ),
                        startIndex: 0,
                        length: 7,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 2,
                        segmentLength: 16,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Null,
                        startIndex: 9,
                        length: 7,
                        isFragmented: false ) );
        }
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

        using ( new AssertionScope() )
        {
            result.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 9, 10, 11, 12, 13, 14, 15, 16, 17 );

            ToArray( sut.Report.Nodes )
                .Should()
                .BeSequentiallyEqualTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 1,
                        segmentLength: 16,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 1 ),
                        startIndex: 0,
                        length: 9,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 0,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 0 ),
                        startIndex: 0,
                        length: 8,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 1,
                        segmentLength: 16,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Null,
                        startIndex: 9,
                        length: 7,
                        isFragmented: false ) );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void GreedyRent_ShouldReturnEmptySequence_WhenLengthIsLessThanOne(int length)
    {
        var sut = new MemoryPool<int>( 16 );
        using var result = sut.GreedyRent( length );

        using ( new AssertionScope() )
        {
            result.Owner.Should().BeNull();
            result.Clear.Should().BeFalse();
            result.AsMemory().ToArray().Should().BeEmpty();
            result.AsSpan().ToArray().Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            result.AsSpan().Length.Should().Be( length );
            result.Owner.Should().BeSameAs( sut );
        }
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

        using ( new AssertionScope() )
        {
            result.AsSpan().Length.Should().Be( length );
            result.AsSpan().ToArray().Should().AllBeEquivalentTo( 0 );
            other.AsSpan().ToArray().Should().BeSequentiallyEqualTo( 1, 2, 3, 4, 5, 6, 7, 8 );
            second.AsSpan().ToArray().Should().AllBeEquivalentTo( -1 );
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
    public void TokenClear_ShouldClearReturnedToken_WhenEnabled(int firstLength, int length)
    {
        var sut = new MemoryPool<int>( 64 );

        using var first = sut.Rent( firstLength ).EnableClearing();
        first.AsSpan().Fill( 1 );

        using ( var second = sut.Rent( length ).EnableClearing() )
            second.AsSpan().Fill( 2 );

        using var result = sut.Rent( length );

        using ( new AssertionScope() )
        {
            first.AsSpan().ToArray().Should().AllBeEquivalentTo( 1 );
            result.AsSpan().ToArray().Should().AllBeEquivalentTo( default( int ) );
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
    public void TokenClear_ShouldLeaveOldDataInReturnedTokens_WhenDisabled(int firstLength, int length)
    {
        var sut = new MemoryPool<int>( 64 );

        using var first = sut.Rent( firstLength ).EnableClearing( false );
        first.AsSpan().Fill( 1 );

        using ( var second = sut.Rent( length ).EnableClearing( false ) )
            second.AsSpan().Fill( 2 );

        using var result = sut.Rent( length );

        using ( new AssertionScope() )
        {
            first.AsSpan().ToArray().Should().AllBeEquivalentTo( 1 );
            result.AsSpan().ToArray().Should().AllBeEquivalentTo( 2 );
        }
    }

    [Fact]
    public void TrimExcess_ShouldRemoveAllReturnedTailTokens_WhenNoActiveTokenIsRented()
    {
        var sut = new MemoryPool<int>( 16 );

        using ( var r = sut.Rent( 12 ) )
            r.AsSpan().Fill( 1 );

        sut.TrimExcess();
        using var result = sut.Rent( 12 );

        result.AsSpan().ToArray().Should().AllBeEquivalentTo( default( int ) );
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

        using ( new AssertionScope() )
        {
            first.AsSpan().Length.Should().Be( 12 );
            first.AsSpan().ToArray().Should().AllBeEquivalentTo( 1 );
            result.AsSpan().ToArray().Should().AllBeEquivalentTo( default( int ) );
        }
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

        using ( new AssertionScope() )
        {
            a.TryGetInfo()
                .Should()
                .BeEquivalentTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 0,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 2 ),
                        startIndex: 0,
                        length: 3,
                        isFragmented: false ) );

            b.TryGetInfo()
                .Should()
                .BeEquivalentTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 0,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 4 ),
                        startIndex: 3,
                        length: 5,
                        isFragmented: false ) );

            c.TryGetInfo()
                .Should()
                .BeEquivalentTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 1,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 3 ),
                        startIndex: 0,
                        length: 7,
                        isFragmented: false ) );

            d.TryGetInfo()
                .Should()
                .BeEquivalentTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 1,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 0 ),
                        startIndex: 7,
                        length: 1,
                        isFragmented: false ) );

            e.TryGetInfo()
                .Should()
                .BeEquivalentTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: sut,
                        segmentIndex: 2,
                        segmentLength: 8,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Create( 1 ),
                        startIndex: 0,
                        length: 1,
                        isFragmented: false ) );
        }
    }

    [Fact]
    public void ReportInfo_Default_ShouldReturnCorrectData()
    {
        var sut = default( MemoryPool<int>.ReportInfo );

        using ( new AssertionScope() )
        {
            sut.AllocatedSegments.Should().Be( 0 );
            sut.ActiveSegments.Should().Be( 0 );
            ToArray( sut.Nodes ).Should().BeEmpty();
            sut.FragmentedNodes.Count.Should().Be( 0 );
            ToArray( sut.FragmentedNodes ).Should().BeEmpty();
        }
    }

    [Fact]
    public void ReportInfo_ForEmptyPool_ShouldReturnCorrectData()
    {
        var sut = new MemoryPool<int>( 16 ).Report;

        using ( new AssertionScope() )
        {
            sut.AllocatedSegments.Should().Be( 0 );
            sut.ActiveSegments.Should().Be( 0 );
            ToArray( sut.Nodes ).Should().BeEmpty();
            sut.FragmentedNodes.Count.Should().Be( 0 );
            ToArray( sut.FragmentedNodes ).Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            sut.AllocatedSegments.Should().Be( 5 );
            sut.ActiveSegments.Should().Be( 4 );
            sut.FragmentedNodes.Count.Should().Be( 3 );

            fragmentedNodes.Should()
                .BeSequentiallyEqualTo(
                    new MemoryPool<int>.ReportInfo.FragmentedNode(
                        segmentIndex: 0,
                        segmentLength: 16,
                        startIndex: 0,
                        length: 16 ),
                    new MemoryPool<int>.ReportInfo.FragmentedNode(
                        segmentIndex: 1,
                        segmentLength: 32,
                        startIndex: 25,
                        length: 7 ),
                    new MemoryPool<int>.ReportInfo.FragmentedNode(
                        segmentIndex: 2,
                        segmentLength: 16,
                        startIndex: 0,
                        length: 16 ) );

            nodes.Should()
                .BeSequentiallyEqualTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 3,
                        segmentLength: 32,
                        isSegmentActive: true,
                        nodeId: NullableIndex.CreateUnsafe( 10 ),
                        startIndex: 20,
                        length: 8,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 3,
                        segmentLength: 32,
                        isSegmentActive: true,
                        nodeId: NullableIndex.CreateUnsafe( 7 ),
                        startIndex: 0,
                        length: 20,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 2,
                        segmentLength: 16,
                        isSegmentActive: true,
                        nodeId: NullableIndex.CreateUnsafe( 8 ),
                        startIndex: 0,
                        length: 16,
                        isFragmented: true ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 1,
                        segmentLength: 32,
                        isSegmentActive: true,
                        nodeId: NullableIndex.CreateUnsafe( 13 ),
                        startIndex: 25,
                        length: 7,
                        isFragmented: true ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 1,
                        segmentLength: 32,
                        isSegmentActive: true,
                        nodeId: NullableIndex.CreateUnsafe( 6 ),
                        startIndex: 23,
                        length: 2,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 1,
                        segmentLength: 32,
                        isSegmentActive: true,
                        nodeId: NullableIndex.CreateUnsafe( 4 ),
                        startIndex: 17,
                        length: 6,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 1,
                        segmentLength: 32,
                        isSegmentActive: true,
                        nodeId: NullableIndex.CreateUnsafe( 1 ),
                        startIndex: 0,
                        length: 17,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 0,
                        segmentLength: 16,
                        isSegmentActive: true,
                        nodeId: NullableIndex.CreateUnsafe( 0 ),
                        startIndex: 0,
                        length: 16,
                        isFragmented: true ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 3,
                        segmentLength: 32,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Null,
                        startIndex: 28,
                        length: 4,
                        isFragmented: false ),
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 4,
                        segmentLength: 32,
                        isSegmentActive: false,
                        nodeId: NullableIndex.Null,
                        startIndex: 0,
                        length: 32,
                        isFragmented: false ) );
        }
    }

    [Fact]
    public void ReportInfo_ForPoolWithSingleFullyUsedSegment_ShouldReturnCorrectData()
    {
        var pool = new MemoryPool<int>( 16 );
        _ = pool.Rent( 16 );

        var sut = pool.Report;
        var nodes = ToArray( sut.Nodes );
        var fragmentedNodes = ToArray( sut.FragmentedNodes );

        using ( new AssertionScope() )
        {
            sut.AllocatedSegments.Should().Be( 1 );
            sut.ActiveSegments.Should().Be( 1 );
            sut.FragmentedNodes.Count.Should().Be( 0 );
            fragmentedNodes.Should().BeEmpty();

            nodes.Should()
                .BeSequentiallyEqualTo(
                    new MemoryPool<int>.ReportInfo.Node(
                        pool: pool,
                        segmentIndex: 0,
                        segmentLength: 16,
                        isSegmentActive: true,
                        nodeId: NullableIndex.CreateUnsafe( 0 ),
                        startIndex: 0,
                        length: 16,
                        isFragmented: false ) );
        }
    }

    [Fact]
    public void ReportInfoFragmentedNode_ShouldHaveCorrectProperties()
    {
        var pool = new MemoryPool<int>( 8 );
        var token = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        token.Dispose();

        var sut = ToArray( pool.Report.FragmentedNodes )[0];

        using ( new AssertionScope() )
        {
            sut.SegmentIndex.Should().Be( 0 );
            sut.SegmentLength.Should().Be( 8 );
            sut.StartIndex.Should().Be( 0 );
            sut.Length.Should().Be( 3 );
            sut.EndIndex.Should().Be( 3 );
            sut.ToString().Should().Be( "Segment: @0 (Length: 8), Node: [0:2] (3)" );
        }
    }

    [Fact]
    public void ReportInfoNode_ShouldHaveCorrectProperties_ForActiveNode()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 5 );

        var sut = ToArray( pool.Report.Nodes )[0];

        using ( new AssertionScope() )
        {
            sut.SegmentIndex.Should().Be( 0 );
            sut.SegmentLength.Should().Be( 8 );
            sut.StartIndex.Should().Be( 0 );
            sut.Length.Should().Be( 5 );
            sut.EndIndex.Should().Be( 5 );
            sut.IsSegmentActive.Should().BeTrue();
            sut.IsFragmented.Should().BeFalse();
            sut.ToString().Should().Be( "Segment: @0 (Length: 8), Node: [0:4] (5)" );
        }
    }

    [Fact]
    public void ReportInfoNode_ShouldHaveCorrectProperties_ForFragmentedNode()
    {
        var pool = new MemoryPool<int>( 8 );
        var token = pool.Rent( 3 );
        _ = pool.Rent( 4 );
        token.Dispose();

        var sut = ToArray( pool.Report.Nodes )[1];

        using ( new AssertionScope() )
        {
            sut.SegmentIndex.Should().Be( 0 );
            sut.SegmentLength.Should().Be( 8 );
            sut.StartIndex.Should().Be( 0 );
            sut.Length.Should().Be( 3 );
            sut.EndIndex.Should().Be( 3 );
            sut.IsSegmentActive.Should().BeTrue();
            sut.IsFragmented.Should().BeTrue();
            sut.ToString().Should().Be( "Segment: @0 (Length: 8), Node: [0:2] (3) (fragmented)" );
        }
    }

    [Fact]
    public void ReportInfoNode_ShouldHaveCorrectProperties_ForFreeTailSegmentNode()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 5 );

        var sut = ToArray( pool.Report.Nodes )[1];

        using ( new AssertionScope() )
        {
            sut.SegmentIndex.Should().Be( 0 );
            sut.SegmentLength.Should().Be( 8 );
            sut.StartIndex.Should().Be( 5 );
            sut.Length.Should().Be( 3 );
            sut.EndIndex.Should().Be( 8 );
            sut.IsSegmentActive.Should().BeTrue();
            sut.IsFragmented.Should().BeFalse();
            sut.ToString().Should().Be( "Segment: @0 (Length: 8), Node: [5:7] (3) (free tail)" );
        }
    }

    [Fact]
    public void ReportInfoNode_ShouldHaveCorrectProperties_ForInactiveSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        var token = pool.Rent( 8 );
        token.Dispose();

        var sut = ToArray( pool.Report.Nodes )[0];

        using ( new AssertionScope() )
        {
            sut.SegmentIndex.Should().Be( 0 );
            sut.SegmentLength.Should().Be( 8 );
            sut.StartIndex.Should().Be( 0 );
            sut.Length.Should().Be( 8 );
            sut.EndIndex.Should().Be( 8 );
            sut.IsSegmentActive.Should().BeFalse();
            sut.IsFragmented.Should().BeFalse();
            sut.ToString().Should().Be( "Segment: @0 (Length: 8) (inactive)" );
        }
    }

    [Fact]
    public void ReportInfoNode_TryGetToken_ShouldReturnToken_ForActiveNode()
    {
        var pool = new MemoryPool<int>( 8 );
        var token = pool.Rent( 5 );
        var sut = ToArray( pool.Report.Nodes )[0];

        var result = sut.TryGetToken();

        result.Should().BeEquivalentTo( token );
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

        result.Should().BeNull();
    }

    [Fact]
    public void ReportInfoNode_TryGetToken_ShouldReturnNull_ForFreeTailSegmentNode()
    {
        var pool = new MemoryPool<int>( 8 );
        _ = pool.Rent( 5 );
        var sut = ToArray( pool.Report.Nodes )[1];

        var result = sut.TryGetToken();

        result.Should().BeNull();
    }

    [Fact]
    public void ReportInfoNode_TryGetToken_ShouldReturnNull_ForInactiveSegment()
    {
        var pool = new MemoryPool<int>( 8 );
        var token = pool.Rent( 8 );
        token.Dispose();
        var sut = ToArray( pool.Report.Nodes )[0];

        var result = sut.TryGetToken();

        result.Should().BeNull();
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
