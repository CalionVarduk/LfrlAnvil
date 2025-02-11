using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.ObjectTests;

public abstract class GenericObjectExtensionsOfComparableTypeTests<T> : GenericObjectExtensionsTests<T>
    where T : IComparable<T>
{
    [Fact]
    public void Min_ShouldReturnSource_WhenBothValuesAreEqual()
    {
        var sut = Fixture.CreateNotDefault<T>();
        var result = sut.Min( sut );
        result.TestEquals( sut ).Go();
    }

    [Fact]
    public void Min_ShouldReturnSource_WhenSourceIsLesser()
    {
        var (sut, other) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var result = sut.Min( other );
        result.TestEquals( sut ).Go();
    }

    [Fact]
    public void Min_ShouldReturnOther_WhenSourceIsGreater()
    {
        var (other, sut) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var result = sut.Min( other );
        result.TestEquals( other ).Go();
    }

    [Fact]
    public void Max_ShouldReturnOther_WhenBothValuesAreEqual()
    {
        var sut = Fixture.CreateNotDefault<T>();
        var result = sut.Max( sut );
        result.TestEquals( sut ).Go();
    }

    [Fact]
    public void Max_ShouldReturnSource_WhenSourceIsGreater()
    {
        var (other, sut) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var result = sut.Max( other );
        result.TestEquals( sut ).Go();
    }

    [Fact]
    public void Max_ShouldReturnOther_WhenSourceIsLesser()
    {
        var (sut, other) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var result = sut.Max( other );
        result.TestEquals( other ).Go();
    }

    [Fact]
    public void Clamp_ShouldReturnMin_WhenSourceIsLesser()
    {
        var (sut, min, max) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var result = sut.Clamp( min, max );
        result.TestEquals( min ).Go();
    }

    [Fact]
    public void Clamp_ShouldReturnMax_WhenSourceIsGreater()
    {
        var (min, max, sut) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var result = sut.Clamp( min, max );
        result.TestEquals( max ).Go();
    }

    [Fact]
    public void Clamp_ShouldReturnSource_WhenSourceIsGreaterThanMinAndLesserThanMax()
    {
        var (min, sut, max) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var result = sut.Clamp( min, max );
        result.TestEquals( sut ).Go();
    }

    [Fact]
    public void MinMax_ShouldReturnSourceAsMinAndOtherAsMax_WhenBothValuesAreEqual()
    {
        var sut = Fixture.CreateNotDefault<T>();
        var result = sut.MinMax( sut );
        result.TestEquals( (sut, sut) ).Go();
    }

    [Fact]
    public void MinMax_ShouldReturnOtherAsMinAndSourceAsMax_WhenSourceIsGreater()
    {
        var (other, sut) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var result = sut.MinMax( other );
        result.TestEquals( (other, sut) ).Go();
    }

    [Fact]
    public void MinMax_ShouldReturnSourceAsMinAndOtherAsMax_WhenSourceIsLesser()
    {
        var (sut, other) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var result = sut.MinMax( other );
        result.TestEquals( (sut, other) ).Go();
    }
}
