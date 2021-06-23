using System;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Bounds
{
    [GenericTestClass( typeof( BoundsTestsData<> ) )]
    public abstract class BoundsTests<T> : TestsBase
        where T : IComparable<T>
    {
        [Fact]
        public void Create_ShouldCreateCorrectBounds()
        {
            var (min, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );

            var sut = Common.Bounds.Create( min, max );

            sut.Should()
                .BeEquivalentTo(
                    new
                    {
                        Min = min,
                        Max = max
                    } );
        }

        [Fact]
        public void Ctor_ShouldCreateWithDistinctMinAndMax()
        {
            var (min, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );

            var sut = new Bounds<T>( min, max );

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
            var value = Fixture.Create<T>();

            var sut = new Bounds<T>( value, value );

            sut.Should()
                .BeEquivalentTo(
                    new
                    {
                        Min = value,
                        Max = value
                    } );
        }

        [Fact]
        public void Ctor_ShouldThrow_WhenMinIsGreaterThanMax()
        {
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );

            Action action = () => new Bounds<T>( min, max );

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SetMin_ShouldReturnCorrectResult()
        {
            var (min, newMin, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );

            var sut = new Bounds<T>( min, max );

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
            var (min, max, newMax) = Fixture.CreateDistinctSortedCollection<T>( 3 );

            var sut = new Bounds<T>( min, max );

            var result = sut.SetMax( newMax );

            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Min = min,
                        Max = newMax
                    } );
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var (min, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );

            var sut = new Bounds<T>( min, max );
            var expected = Common.Hash.Default.Add( min ).Add( max ).Value;

            var result = sut.GetHashCode();

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( BoundsTestsData<T>.CreateEqualsTestData ) )]
        public void Equals_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
        {
            var a = new Bounds<T>( min1, max1 );
            var b = new Bounds<T>( min2, max2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( BoundsTestsData<T>.CreateClampTestData ) )]
        public void Clamp_ShouldReturnCorrectResult(T min, T max, T value, T expected)
        {
            var sut = new Bounds<T>( min, max );

            var result = sut.Clamp( value );

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( BoundsTestsData<T>.CreateContainsTestData ) )]
        public void Contains_ShouldReturnCorrectResult(T min, T max, T value, bool expected)
        {
            var sut = new Bounds<T>( min, max );

            var result = sut.Contains( value );

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( BoundsTestsData<T>.CreateContainsForBoundsTestData ) )]
        public void Contains_ForBounds_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
        {
            var sut = new Bounds<T>( min1, max1 );
            var other = new Bounds<T>( min2, max2 );

            var result = sut.Contains( other );

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( BoundsTestsData<T>.CreateIntersectsForBoundsTestData ) )]
        public void Intersects_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
        {
            var sut = new Bounds<T>( min1, max1 );
            var other = new Bounds<T>( min2, max2 );

            var result = sut.Intersects( other );

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( BoundsTestsData<T>.CreateIntersectionTestData ) )]
        public void GetIntersection_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, Bounds<T>? expected)
        {
            var sut = new Bounds<T>( min1, max1 );
            var other = new Bounds<T>( min2, max2 );

            var result = sut.GetIntersection( other );

            result.Should().BeEquivalentTo( expected );
        }

        [Theory]
        [GenericMethodData( nameof( BoundsTestsData<T>.CreateEqualsTestData ) )]
        public void EqualityOperator_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
        {
            var a = new Bounds<T>( min1, max1 );
            var b = new Bounds<T>( min2, max2 );

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( BoundsTestsData<T>.CreateNotEqualsTestData ) )]
        public void InequalityOperator_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
        {
            var a = new Bounds<T>( min1, max1 );
            var b = new Bounds<T>( min2, max2 );

            var result = a != b;

            result.Should().Be( expected );
        }
    }
}
