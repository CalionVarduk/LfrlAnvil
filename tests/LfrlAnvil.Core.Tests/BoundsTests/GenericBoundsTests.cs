using System;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using Xunit;

namespace LfrlAnvil.Tests.BoundsTests;

[GenericTestClass( typeof( GenericBoundsTestsData<> ) )]
public abstract class GenericBoundsTests<T> : TestsBase
    where T : IComparable<T>
{
    [Fact]
    public void Create_ShouldCreateCorrectBounds()
    {
        var (min, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );

        var sut = Bounds.Create( min, max );

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
    public void Ctor_ShouldThrowArgumentException_WhenMinIsGreaterThanMax()
    {
        var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        var action = Lambda.Of( () => new Bounds<T>( min, max ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void SetMin_ShouldReturnBoundsWithNewMinAndOldMax()
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
    public void SetMax_ShouldReturnBoundsWithOldMinAndNewMax()
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
    public void GetHashCode_ShouldReturnMixOfMinAndMax()
    {
        var (min, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );

        var sut = new Bounds<T>( min, max );
        var expected = Hash.Default.Add( min ).Add( max ).Value;

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
    {
        var a = new Bounds<T>( min1, max1 );
        var b = new Bounds<T>( min2, max2 );

        var result = a.Equals( b );

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetClampData ) )]
    public void Clamp_ShouldReturnCorrectResult(T min, T max, T value, T expected)
    {
        var sut = new Bounds<T>( min, max );
        var result = sut.Clamp( value );
        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetContainsData ) )]
    public void Contains_ShouldReturnCorrectResult(T min, T max, T value, bool expected)
    {
        var sut = new Bounds<T>( min, max );
        var result = sut.Contains( value );
        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetContainsExclusivelyData ) )]
    public void ContainsExclusively_ShouldReturnCorrectResult(T min, T max, T value, bool expected)
    {
        var sut = new Bounds<T>( min, max );
        var result = sut.ContainsExclusively( value );
        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetContainsForBoundsData ) )]
    public void Contains_ForBounds_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
    {
        var sut = new Bounds<T>( min1, max1 );
        var other = new Bounds<T>( min2, max2 );

        var result = sut.Contains( other );

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetContainsExclusivelyForBoundsData ) )]
    public void ContainsExclusively_ForBounds_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
    {
        var sut = new Bounds<T>( min1, max1 );
        var other = new Bounds<T>( min2, max2 );

        var result = sut.ContainsExclusively( other );

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetIntersectsForBoundsData ) )]
    public void Intersects_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
    {
        var sut = new Bounds<T>( min1, max1 );
        var other = new Bounds<T>( min2, max2 );

        var result = sut.Intersects( other );

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetIntersectionData ) )]
    public void GetIntersection_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, Bounds<T>? expected)
    {
        var sut = new Bounds<T>( min1, max1 );
        var other = new Bounds<T>( min2, max2 );

        var result = sut.GetIntersection( other );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetMergeWithData ) )]
    public void MergeWith_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, Bounds<T>? expected)
    {
        var sut = new Bounds<T>( min1, max1 );
        var other = new Bounds<T>( min2, max2 );

        var result = sut.MergeWith( other );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetSplitAtData ) )]
    public void SplitAt_ShouldReturnCorrectResult(T min, T max, T value, Bounds<T> expectedFirst, Bounds<T>? expectedSecond)
    {
        var sut = new Bounds<T>( min, max );

        var result = sut.SplitAt( value );

        using ( new AssertionScope() )
        {
            result.First.Should().BeEquivalentTo( expectedFirst );
            result.Second.Should().BeEquivalentTo( expectedSecond );
        }
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetRemoveData ) )]
    public void Remove_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, Bounds<T>? expectedFirst, Bounds<T>? expectedSecond)
    {
        var sut = new Bounds<T>( min1, max1 );
        var other = new Bounds<T>( min2, max2 );

        var result = sut.Remove( other );

        using ( new AssertionScope() )
        {
            result.First.Should().BeEquivalentTo( expectedFirst );
            result.Second.Should().BeEquivalentTo( expectedSecond );
        }
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
    {
        var a = new Bounds<T>( min1, max1 );
        var b = new Bounds<T>( min2, max2 );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
    {
        var a = new Bounds<T>( min1, max1 );
        var b = new Bounds<T>( min2, max2 );

        var result = a != b;

        result.Should().Be( expected );
    }
}
