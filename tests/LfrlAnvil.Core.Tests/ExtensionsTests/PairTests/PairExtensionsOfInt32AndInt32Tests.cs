using LfrlAnvil.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.ExtensionsTests.PairTests;

public class PairExtensionsOfInt32AndInt32Tests : GenericPairExtensionsTests<int, int>
{
    [Fact]
    public void AsEnumerable_ShouldReturnCorrectResult_WhenNonNullable()
    {
        var (first, second) = Fixture.CreateDistinctCollection<int>( 2 );
        var sut = Pair.Create( first, second );

        var result = sut.AsEnumerable();

        result.Should().BeSequentiallyEqualTo( first, second );
    }

    [Fact]
    public void AsEnumerable_ShouldReturnCorrectResult_WhenSecondIsNullableWithValue()
    {
        var (first, second) = Fixture.CreateDistinctCollection<int>( 2 );
        var sut = Pair.Create( first, ( int? )second );

        var result = sut.AsEnumerable();

        result.Should().BeSequentiallyEqualTo( first, second );
    }

    [Fact]
    public void AsEnumerable_ShouldReturnCorrectResult_WhenSecondIsNullableWithoutValue()
    {
        var first = Fixture.Create<int>();
        var sut = Pair.Create( first, ( int? )null );

        var result = sut.AsEnumerable();

        result.Should().BeSequentiallyEqualTo( first );
    }

    [Fact]
    public void AsEnumerable_ShouldReturnCorrectResult_WhenFirstIsNullableWithValue()
    {
        var (first, second) = Fixture.CreateDistinctCollection<int>( 2 );
        var sut = Pair.Create( ( int? )first, second );

        var result = sut.AsEnumerable();

        result.Should().BeSequentiallyEqualTo( first, second );
    }

    [Fact]
    public void AsEnumerable_ShouldReturnCorrectResult_WhenFirstIsNullableWithoutValue()
    {
        var second = Fixture.Create<int>();
        var sut = Pair.Create( ( int? )null, second );

        var result = sut.AsEnumerable();

        result.Should().BeSequentiallyEqualTo( second );
    }

    [Fact]
    public void AsEnumerable_ShouldReturnCorrectResult_WhenNullableWithBothValues()
    {
        var (first, second) = Fixture.CreateDistinctCollection<int>( 2 );
        var sut = Pair.Create( ( int? )first, ( int? )second );

        var result = sut.AsEnumerable();

        result.Should().BeSequentiallyEqualTo( first, second );
    }

    [Fact]
    public void AsEnumerable_ShouldReturnCorrectResult_WhenNullableWithOnlyFirstValue()
    {
        var first = Fixture.Create<int>();
        var sut = Pair.Create( ( int? )first, ( int? )null );

        var result = sut.AsEnumerable();

        result.Should().BeSequentiallyEqualTo( first );
    }

    [Fact]
    public void AsEnumerable_ShouldReturnCorrectResult_WhenNullableWithOnlySecondValue()
    {
        var second = Fixture.Create<int>();
        var sut = Pair.Create( ( int? )null, ( int? )second );

        var result = sut.AsEnumerable();

        result.Should().BeSequentiallyEqualTo( second );
    }

    [Fact]
    public void AsEnumerable_ShouldReturnCorrectResult_WhenNullableWithNoValues()
    {
        var sut = Pair.Create( ( int? )null, ( int? )null );
        var result = sut.AsEnumerable();
        result.Should().BeEmpty();
    }
}
