using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Tests.Extensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class BoundsTests
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void Ctor_ShouldCreateWithDistinctMinAndMax()
        {
            var (min, max) = _fixture.CreateDistinctPair<int>();

            var sut = new Bounds<int>( min, max );

            sut.Should()
                .BeEquivalentTo(
                    new
                    {
                        Min = min,
                        Max = max
                    } );
        }

        [Fact]
        public void Ctor_ShouldCreateWithTheSameMinAndMax()
        {
            var value = _fixture.Create<int>();

            var sut = new Bounds<int>( value, value );

            sut.Should()
                .BeEquivalentTo(
                    new
                    {
                        Min = value,
                        Max = value
                    } );
        }

        [Fact]
        public void Ctor_ShouldThrow_WhenMinIsNull()
        {
            var max = _fixture.Create<string>();

            Action action = () => new Bounds<string>( null!, max );

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_ShouldThrow_WhenMaxIsNull()
        {
            var min = _fixture.Create<string>();

            Action action = () => new Bounds<string>( min, null! );

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_ShouldThrow_WhenMinIsGreaterThanMax()
        {
            var (max, min) = _fixture.CreateDistinctPair<int>();

            Action action = () => new Bounds<int>( min, max );

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var min = 987654321;
            var max = 1234567890;
            var sut = new Bounds<int>( min, max );

            var result = sut.GetHashCode();

            result.Should().Be( -553869366 );
        }

        [Theory]
        [InlineData( 1, 2, 1, 2, true )]
        [InlineData( 1, 2, 1, 3, false )]
        [InlineData( 0, 2, 1, 2, false )]
        [InlineData( 1, 2, 3, 4, false )]
        public void Equals_ShouldReturnCorrectResult(int min1, int max1, int min2, int max2, bool expected)
        {
            var a = new Bounds<int>( min1, max1 );
            var b = new Bounds<int>( min2, max2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Fact]
        public void SetMin_ShouldReturnCorrectResult()
        {
            var (min, newMin, max) = _fixture.CreateDistinctTriple<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.SetMin( newMin );

            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Min = newMin,
                        Max = max
                    } );
        }

        [Fact]
        public void SetMax_ShouldReturnCorrectResult()
        {
            var (min, max, newMax) = _fixture.CreateDistinctTriple<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.SetMax( newMax );

            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Min = min,
                        Max = newMax
                    } );
        }

        [Theory]
        [InlineData( 1, 3, 0, false )]
        [InlineData( 1, 3, 1, true )]
        [InlineData( 1, 3, 2, true )]
        [InlineData( 1, 3, 3, true )]
        [InlineData( 1, 3, 4, false )]
        public void Contains_ShouldReturnCorrectResult(int min, int max, int value, bool expected)
        {
            var sut = new Bounds<int>( min, max );

            var result = sut.Contains( value );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 4, 1, 4, true )]
        [InlineData( 1, 4, 1, 3, true )]
        [InlineData( 1, 4, 2, 4, true )]
        [InlineData( 1, 4, 2, 3, true )]
        [InlineData( 1, 4, 0, 4, false )]
        [InlineData( 1, 4, 1, 5, false )]
        [InlineData( 1, 4, 0, 5, false )]
        [InlineData( 1, 4, -1, 0, false )]
        [InlineData( 1, 4, -1, 1, false )]
        [InlineData( 1, 4, 5, 6, false )]
        [InlineData( 1, 4, 4, 6, false )]
        public void Contains_ForBounds_ShouldReturnCorrectResult(int min1, int max1, int min2, int max2, bool expected)
        {
            var sut = new Bounds<int>( min1, max1 );

            var result = sut.Contains( new Bounds<int>( min2, max2 ) );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 4, 1, 4, true )]
        [InlineData( 1, 4, 1, 3, true )]
        [InlineData( 1, 4, 2, 4, true )]
        [InlineData( 1, 4, 2, 3, true )]
        [InlineData( 1, 4, 0, 4, true )]
        [InlineData( 1, 4, 1, 5, true )]
        [InlineData( 1, 4, 0, 5, true )]
        [InlineData( 1, 4, -1, 0, false )]
        [InlineData( 1, 4, -1, 1, true )]
        [InlineData( 1, 4, 5, 6, false )]
        [InlineData( 1, 4, 4, 6, true )]
        public void Intersects_ShouldReturnCorrectResult(int min1, int max1, int min2, int max2, bool expected)
        {
            var sut = new Bounds<int>( min1, max1 );

            var result = sut.Intersects( new Bounds<int>( min2, max2 ) );

            result.Should().Be( expected );
        }

        [Theory]
        [MemberData( nameof( GetIntersectionTestData ) )]
        public void GetIntersection_ShouldReturnCorrectResult(int min1, int max1, int min2, int max2, Bounds<int>? expected)
        {
            var sut = new Bounds<int>( min1, max1 );

            var result = sut.GetIntersection( new Bounds<int>( min2, max2 ) );

            result.Should().BeEquivalentTo( expected );
        }

        [Theory]
        [InlineData( 1, 3, 0, 1 )]
        [InlineData( 1, 3, 1, 1 )]
        [InlineData( 1, 3, 2, 2 )]
        [InlineData( 1, 3, 3, 3 )]
        [InlineData( 1, 3, 4, 3 )]
        public void Clamp_ShouldReturnCorrectResult(int min, int max, int value, int expected)
        {
            var sut = new Bounds<int>( min, max );

            var result = sut.Clamp( value );

            result.Should().Be( expected );
        }

        public static IEnumerable<object?[]> GetIntersectionTestData()
        {
            return new[]
            {
                new object?[] { 1, 4, 1, 4, new Bounds<int>( 1, 4 ) },
                new object?[] { 1, 4, 1, 3, new Bounds<int>( 1, 3 ) },
                new object?[] { 1, 4, 2, 4, new Bounds<int>( 2, 4 ) },
                new object?[] { 1, 4, 2, 3, new Bounds<int>( 2, 3 ) },
                new object?[] { 1, 4, 0, 4, new Bounds<int>( 1, 4 ) },
                new object?[] { 1, 4, 1, 5, new Bounds<int>( 1, 4 ) },
                new object?[] { 1, 4, 0, 5, new Bounds<int>( 1, 4 ) },
                new object?[] { 1, 4, -1, 0, null },
                new object?[] { 1, 4, -1, 1, new Bounds<int>( 1, 1 ) },
                new object?[] { 1, 4, 5, 6, null },
                new object?[] { 1, 4, 4, 6, new Bounds<int>( 4, 4 ) }
            };
        }
    }
}
