using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.PairTests;

public abstract class GenericPairExtensionsTests<T1, T2> : TestsBase
{
    [Fact]
    public void ToPair_WithTuple_ShouldReturnCorrectResult()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = Tuple.Create( first, second );

        var result = sut.ToPair();

        result.Should()
            .BeEquivalentTo(
                new
                {
                    First = first,
                    Second = second
                } );
    }

    [Fact]
    public void ToPair_WithValueTuple_ShouldReturnCorrectResult()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = (First: first, Second: second);

        var result = sut.ToPair();

        result.Should()
            .BeEquivalentTo(
                new
                {
                    First = first,
                    Second = second
                } );
    }

    [Fact]
    public void ToTuple_ShouldReturnCorrectResult()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = Pair.Create( first, second );

        var result = sut.ToTuple();

        result.Should()
            .BeEquivalentTo(
                new
                {
                    Item1 = first,
                    Item2 = second
                } );
    }

    [Fact]
    public void ToValueTuple_ShouldReturnCorrectResult()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = Pair.Create( first, second );

        var result = sut.ToValueTuple();

        result.Should()
            .BeEquivalentTo(
                new
                {
                    Item1 = first,
                    Item2 = second
                } );
    }

    [Fact]
    public void Deconstruct_ShouldReturnCorrectValues()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = Pair.Create( first, second );

        var (a, b) = sut;

        using ( new AssertionScope() )
        {
            a.Should().Be( first );
            b.Should().Be( second );
        }
    }
}
