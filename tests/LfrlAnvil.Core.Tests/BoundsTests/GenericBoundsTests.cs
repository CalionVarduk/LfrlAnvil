using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Tests.BoundsTests;

[GenericTestClass( typeof( GenericBoundsTestsData<> ) )]
public abstract class GenericBoundsTests<T> : TestsBase
    where T : IComparable<T>
{
    [Fact]
    public void Create_ShouldCreateCorrectBounds()
    {
        var (min, max) = Fixture.CreateManyDistinctSorted<T>( count: 2 );

        var sut = Bounds.Create( min, max );

        Assertion.All( sut.Min.TestEquals( min ), sut.Max.TestEquals( max ) ).Go();
    }

    [Fact]
    public void Ctor_ShouldCreateWithDistinctMinAndMax()
    {
        var (min, max) = Fixture.CreateManyDistinctSorted<T>( count: 2 );

        var sut = new Bounds<T>( min, max );

        Assertion.All( sut.Min.TestEquals( min ), sut.Max.TestEquals( max ) ).Go();
    }

    [Fact]
    public void Ctor_ShouldCreateWithTheSameMinAndMax()
    {
        var value = Fixture.Create<T>();

        var sut = new Bounds<T>( value, value );

        Assertion.All( sut.Min.TestEquals( value ), sut.Max.TestEquals( value ) ).Go();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentException_WhenMinIsGreaterThanMax()
    {
        var (max, min) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var action = Lambda.Of( () => new Bounds<T>( min, max ) );
        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void SetMin_ShouldReturnBoundsWithNewMinAndOldMax()
    {
        var (min, newMin, max) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var sut = new Bounds<T>( min, max );

        var result = sut.SetMin( newMin );

        Assertion.All( result.Min.TestEquals( newMin ), result.Max.TestEquals( max ) ).Go();
    }

    [Fact]
    public void SetMax_ShouldReturnBoundsWithOldMinAndNewMax()
    {
        var (min, max, newMax) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var sut = new Bounds<T>( min, max );

        var result = sut.SetMax( newMax );

        Assertion.All( result.Min.TestEquals( min ), result.Max.TestEquals( newMax ) ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnMixOfMinAndMax()
    {
        var (min, max) = Fixture.CreateManyDistinctSorted<T>( count: 2 );

        var sut = new Bounds<T>( min, max );
        var expected = Hash.Default.Add( min ).Add( max ).Value;

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
    {
        var a = new Bounds<T>( min1, max1 );
        var b = new Bounds<T>( min2, max2 );

        var result = a.Equals( b );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetClampData ) )]
    public void Clamp_ShouldReturnCorrectResult(T min, T max, T value, T expected)
    {
        var sut = new Bounds<T>( min, max );
        var result = sut.Clamp( value );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetContainsData ) )]
    public void Contains_ShouldReturnCorrectResult(T min, T max, T value, bool expected)
    {
        var sut = new Bounds<T>( min, max );
        var result = sut.Contains( value );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetContainsExclusivelyData ) )]
    public void ContainsExclusively_ShouldReturnCorrectResult(T min, T max, T value, bool expected)
    {
        var sut = new Bounds<T>( min, max );
        var result = sut.ContainsExclusively( value );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetContainsForBoundsData ) )]
    public void Contains_ForBounds_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
    {
        var sut = new Bounds<T>( min1, max1 );
        var other = new Bounds<T>( min2, max2 );

        var result = sut.Contains( other );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetContainsExclusivelyForBoundsData ) )]
    public void ContainsExclusively_ForBounds_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
    {
        var sut = new Bounds<T>( min1, max1 );
        var other = new Bounds<T>( min2, max2 );

        var result = sut.ContainsExclusively( other );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetIntersectsForBoundsData ) )]
    public void Intersects_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
    {
        var sut = new Bounds<T>( min1, max1 );
        var other = new Bounds<T>( min2, max2 );

        var result = sut.Intersects( other );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetIntersectionData ) )]
    public void GetIntersection_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, Bounds<T>? expected)
    {
        var sut = new Bounds<T>( min1, max1 );
        var other = new Bounds<T>( min2, max2 );

        var result = sut.GetIntersection( other );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetMergeWithData ) )]
    public void MergeWith_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, Bounds<T>? expected)
    {
        var sut = new Bounds<T>( min1, max1 );
        var other = new Bounds<T>( min2, max2 );

        var result = sut.MergeWith( other );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetSplitAtData ) )]
    public void SplitAt_ShouldReturnCorrectResult(T min, T max, T value, Bounds<T> expectedFirst, Bounds<T>? expectedSecond)
    {
        var sut = new Bounds<T>( min, max );

        var result = sut.SplitAt( value );

        Assertion.All(
                result.First.TestEquals( expectedFirst ),
                result.Second.TestEquals( expectedSecond ) )
            .Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetRemoveData ) )]
    public void Remove_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, Bounds<T>? expectedFirst, Bounds<T>? expectedSecond)
    {
        var sut = new Bounds<T>( min1, max1 );
        var other = new Bounds<T>( min2, max2 );

        var result = sut.Remove( other );

        Assertion.All(
                result.First.TestEquals( expectedFirst ),
                result.Second.TestEquals( expectedSecond ) )
            .Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
    {
        var a = new Bounds<T>( min1, max1 );
        var b = new Bounds<T>( min2, max2 );

        var result = a == b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericBoundsTestsData<T>.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(T min1, T max1, T min2, T max2, bool expected)
    {
        var a = new Bounds<T>( min1, max1 );
        var b = new Bounds<T>( min2, max2 );

        var result = a != b;

        result.TestEquals( expected ).Go();
    }
}
